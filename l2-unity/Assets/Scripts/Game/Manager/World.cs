using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _playerPlaceholder;
    [SerializeField] private GameObject _userPlaceholder;
    [SerializeField] private GameObject _npcPlaceHolder;
    [SerializeField] private GameObject _monsterPlaceholder;

    [SerializeField] private GameObject _monstersContainer;
    [SerializeField] private GameObject _npcsContainer;
    [SerializeField] private GameObject _usersContainer;

    private EventProcessor _eventProcessor;

    private Dictionary<int, Entity> _players = new Dictionary<int, Entity>();
    private Dictionary<int, Entity> _npcs = new Dictionary<int, Entity>();
    private Dictionary<int, Entity> _objects = new Dictionary<int, Entity>();

    [Header("Layer Masks")]
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private LayerMask _simpleEntityMask;
    [SerializeField] private LayerMask _entityClickAreaMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _clickThroughMask;
    [SerializeField] private LayerMask _groundMask;

    [SerializeField] private bool _offlineMode = false;

    public bool OfflineMode { get { return _offlineMode; } }
    public LayerMask GroundMask { get { return _groundMask; } }

    private static World _instance;
    public static World Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }

        _eventProcessor = EventProcessor.Instance;
        _playerPlaceholder = Resources.Load<GameObject>("Prefab/Player_FDarkElf");
        _userPlaceholder = Resources.Load<GameObject>("Prefab/User_FDarkElf");
        _npcPlaceHolder = Resources.Load<GameObject>("Prefab/Npc");
        _monsterPlaceholder = Resources.Load<GameObject>("Data/Animations/LineageMonsters/gremlin/gremlin_prefab");
        _npcsContainer = GameObject.Find("Npcs");
        _monstersContainer = GameObject.Find("Monsters");
        _usersContainer = GameObject.Find("Users");
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        UpdateMasks();
    }

    void UpdateMasks()
    {
        NameplatesManager.Instance.SetMask(_entityMask);
        Geodata.Instance.ObstacleMask = _obstacleMask;
        ClickManager.Instance.SetMasks(_entityClickAreaMask, _clickThroughMask);
        CameraController.Instance.SetMask(_obstacleMask);
        TargetManager.Instance.SetMask(_simpleEntityMask);
    }

    public void ClearEntities()
    {
        _objects.Clear();
        _players.Clear();
        _npcs.Clear();
    }

    public void RemoveObject(int id)
    {
        Entity transform;
        if (_objects.TryGetValue(id, out transform))
        {
            _players.Remove(id);
            _npcs.Remove(id);
            _objects.Remove(id);

            Destroy(transform.gameObject);
        }
    }

    public void SpawnPlayerOfflineMode()
    {
        if (_offlineMode)
        {
            PlayerEntity entity = _playerPlaceholder.GetComponent<PlayerEntity>();
            entity.Identity.Position = _playerPlaceholder.transform.position;
            // TODO: Add default stats
            SpawnPlayer(entity.Identity, (PlayerStatus)entity.Status, new PlayerStats(), new PlayerAppearance(), true);
        }
    }

    public void OnReceivePlayerInfo(NetworkIdentity identity, PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        // Dont need to block thread
        Task task = new Task(async () =>
        {
            var entity = await GetEntityAsync(identity.Id);

            if (entity == null)
            {
                _eventProcessor.QueueEvent(() => SpawnPlayer(identity, status, stats, appearance, running));
            }
            else
            {
                _eventProcessor.QueueEvent(() => UpdatePlayer(entity, identity, status, stats, appearance, running));
            }
        });
        task.Start();
    }

    public void SpawnPlayer(NetworkIdentity identity, PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        identity.SetPosY(GetGroundHeight(identity.Position));
        identity.EntityType = EntityType.Player;

        CharacterRace race = (CharacterRace)appearance.Race;
        CharacterModelType raceId = CharacterModelTypeParser.ParseRace(race, appearance.Race, identity.IsMage);

        GameObject go = CharacterBuilder.Instance.BuildCharacterBase(raceId, appearance, identity.EntityType);
        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, identity.Heading, transform.eulerAngles.z);
        go.transform.position = identity.Position;
        go.transform.name = "_Player";
        go.layer = LayerMask.NameToLayer("Invisible"); //Invisible

        PlayerEntity player = go.GetComponent<PlayerEntity>();

        player.Status = status;
        player.Identity = identity;
        player.Stats = stats;
        player.Appearance = appearance;
        player.Race = race;
        player.RaceId = raceId;

        go.GetComponent<NetworkTransformShare>().enabled = true;
        go.GetComponent<PlayerController>().enabled = true;
        go.GetComponent<PlayerController>().Initialize();

        go.SetActive(true);
        go.GetComponentInChildren<PlayerAnimationController>().Initialize();
        go.GetComponent<Gear>().Initialize(player.Identity.Id, player.RaceId);

        player.Initialize();

        CameraController.Instance.enabled = true;
        CameraController.Instance.SetTarget(go);

        player.UpdateMoveType(running);

        CharacterInfoWindow.Instance.UpdateValues();

        _players.Add(identity.Id, player);
        _objects.Add(identity.Id, player);
    }

    public void UpdatePlayer(Entity entity, NetworkIdentity identity, PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        entity.gameObject.layer = LayerMask.NameToLayer("Player");

        ((PlayerEntity)entity).Identity.UpdateEntity(identity);
        ((PlayerStatus)entity.Status).UpdateStatus(status);
        ((PlayerStats)entity.Stats).UpdateStats(stats);
        ((PlayerAppearance)entity.Appearance).UpdateAppearance(appearance);
        entity.UpdateMoveType(running);

        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
        entity.EquipAllArmors();

        CharacterInfoWindow.Instance.UpdateValues();

        GameManager.Instance.OnPlayerInfoReceive();
    }

    public void OnReceiveUserInfo(NetworkIdentity identity, PlayerStatus status, Stats stats, PlayerAppearance appearance, bool running)
    {
        // Dont need to block thread
        Debug.LogWarning("OnReceiveUserInfo");
        Task task = new Task(async () =>
        {
            var entity = await GetEntityAsync(identity.Id);

            if (entity == null)
            {
                _eventProcessor.QueueEvent(() => SpawnUser(identity, status, stats, appearance, running));
            }
            else
            {
                Debug.LogWarning("UpdatePlayer");
                _eventProcessor.QueueEvent(() => UpdateUser(entity, identity, status, stats, appearance, running));
            }
        });
        task.Start();
    }

    public void SpawnUser(NetworkIdentity identity, Status status, Stats stats, PlayerAppearance appearance, bool running)
    {
        Debug.Log("Spawn User");
        identity.SetPosY(GetGroundHeight(identity.Position));
        identity.EntityType = EntityType.User;

        CharacterRace race = (CharacterRace)appearance.Race;
        CharacterModelType raceId = CharacterModelTypeParser.ParseRace(race, appearance.Race, identity.IsMage);

        GameObject go = CharacterBuilder.Instance.BuildCharacterBase(raceId, appearance, identity.EntityType);
        go.transform.position = identity.Position;
        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, identity.Heading, transform.eulerAngles.z);

        NetworkHumanoidEntity user = go.GetComponent<NetworkHumanoidEntity>();

        user.Status = status;
        user.Identity = identity;
        user.Appearance = appearance;
        user.Stats = stats;
        user.Race = race;
        user.RaceId = raceId;
        user.UpdateMoveType(running);

        ((NetworkEntityReferenceHolder)user.ReferenceHolder).NetworkTransformReceive.enabled = true;

        go.transform.name = identity.Name;
        go.SetActive(true);

        user.ReferenceHolder.AnimationController.Initialize();
        user.ReferenceHolder.Gear.Initialize(user.Identity.Id, user.RaceId);
        user.Initialize();

        go.transform.SetParent(_usersContainer.transform);

        _players.Add(identity.Id, user);
        _objects.Add(identity.Id, user);
    }

    public void UpdateUser(Entity entity, NetworkIdentity identity, PlayerStatus status, Stats stats, PlayerAppearance appearance, bool running)
    {
        entity.Identity.UpdateEntityPartial(identity);
        ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetNewPosition(identity.Position);

        ((PlayerStatus)entity.Status).UpdateStatus(status);
        entity.Stats.UpdateStats(stats);
        entity.Running = running;
        ((PlayerAppearance)entity.Appearance).UpdateAppearance(appearance);

        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
        ((NetworkHumanoidEntity)entity).EquipAllArmors();
    }

    public void OnReceiveNpcInfo(NetworkIdentity identity, NpcStatus status, Stats stats, Appearance appearance, bool running)
    {
        // Dont need to block thread
        Task task = new Task(async () =>
        {
            var entity = await GetEntityAsync(identity.Id);

            if (entity == null)
            {
                _eventProcessor.QueueEvent(() => SpawnNpc(identity, status, stats, appearance, running));
            }
            else
            {
                _eventProcessor.QueueEvent(() => UpdateNpc(entity, identity, status, stats, appearance, running));
            }
        });
        task.Start();
    }

    public void SpawnNpc(NetworkIdentity identity, NpcStatus status, Stats stats, Appearance appearance, bool running)
    {
        Npcgrp npcgrp = NpcgrpTable.Instance.GetNpcgrp(identity.NpcId);
        NpcName npcName = NpcNameTable.Instance.GetNpcName(identity.NpcId);
        if (npcName == null || npcgrp == null)
        {
            Debug.LogError($"Npc {identity.NpcId} could not be loaded correctly.");
            return;
        }

        GameObject go = ModelTable.Instance.GetNpc(npcgrp.Mesh);
        if (go == null)
        {
            Debug.LogError($"Npc {identity.NpcId} could not be loaded correctly.");
            return;
        }

        identity.SetPosY(GetGroundHeight(identity.Position));
        GameObject npcGo = Instantiate(go, identity.Position, Quaternion.identity);
        //NpcData npcData = new NpcData(npcName, npcgrp);

        identity.EntityType = npcgrp.Type;

        Entity npc;

        if (identity.EntityType == EntityType.NPC)
        {
            npcGo.transform.SetParent(_npcsContainer.transform);
            npc = npcGo.GetComponent<NetworkHumanoidEntity>();
            // ((NetworkEntity)npc).NpcData = npcData;
        }
        else
        {
            npcGo.transform.SetParent(_monstersContainer.transform);
            npc = npcGo.GetComponent<NetworkMonsterEntity>();
            //((MonsterEntity)npc).NpcData = npcData;
        }

        if (appearance.RHand == 0)
        {
            appearance.RHand = npcgrp.Rhand;
        }
        if (appearance.LHand == 0)
        {
            appearance.LHand = npcgrp.Lhand;
        }

        if (appearance.CollisionRadius == 0)
        {
            appearance.CollisionRadius = npcgrp.CollisionRadius;
        }
        if (appearance.CollisionHeight == 0)
        {
            appearance.CollisionHeight = npcgrp.CollisionHeight;
        }

        npc.Status = status;
        npc.Status.Hp = (int)npcgrp.MaxHp;
        npc.Stats = stats;
        npc.Stats.MaxHp = (int)npcgrp.MaxHp;
        npc.Identity = identity;
        npc.Identity.NpcClass = npcgrp.ClassName;

        if (npc.Identity.Name == null || npc.Identity.Name.Length == 0)
        {
            npc.Identity.Name = npcName.Name;
        }
        if (npc.Identity.Title == null || npc.Identity.Title.Length == 0)
        {
            npc.Identity.Title = npcName.Title;
        }
        if (npc.Identity.Title == null || npc.Identity.Title.Length == 0)
        {
            if (identity.EntityType == EntityType.Monster)
            {
                npc.Identity.Title = npcName.Title;
            }
        }

        npc.Identity.TitleColor = npcName.TitleColor;
        npc.Appearance = appearance;

        npcGo.transform.eulerAngles = new Vector3(npcGo.transform.eulerAngles.x, identity.Heading, npcGo.transform.eulerAngles.z);
        npcGo.transform.name = identity.Name;
        npcGo.SetActive(true);

        npc.ReferenceHolder.AnimationController.Initialize();
        npc.ReferenceHolder.Gear.Initialize(npc.Identity.Id, npc.RaceId);

        npc.Initialize();

        _npcs.Add(identity.Id, npc);
        _objects.Add(identity.Id, npc);
    }

    public void UpdateNpc(Entity entity, NetworkIdentity identity, NpcStatus status, Stats stats, Appearance appearance, bool running)
    {
        entity.Identity.UpdateEntityPartial(identity);
        ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetNewPosition(identity.Position);
        //entity.Status.UpdateStatus(status);
        entity.Stats.UpdateStats(stats);
        entity.Running = running;

        //entity.Appearance.UpdateAppearance(appearance);

        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
    }

    public float GetGroundHeight(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 1.0f, Vector3.down, out hit, 2.5f, _groundMask))
        {
            return hit.point.y;
        }

        return pos.y;
    }

    public Task UpdateObjectPosition(int id, Vector3 position)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
        });
    }

    public Task AdjustObjectPositionAndRotation(int id, Vector3 position, int heading)
    {
        return ExecuteWithEntityAsync(id, e =>
        {

            e.Identity.Position = position;
            e.Identity.Heading = heading;
            e.transform.position = new Vector3(position.x, GetGroundHeight(position), position.z);

            float rotation = VectorUtils.ConvertRotToUnity(heading);
            Debug.LogWarning($"ADJUST POSITION AND ROTATION Pos:{position} Heading:{heading} Rotation:{rotation}");

            if (id == GameClient.Instance.CurrentPlayerId)
            {
                NetworkCharacterControllerShare.Instance.Heading = heading;
            }
            else
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
            }
        });
    }

    public Task UpdateObjectRotation(int id, float angle)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(angle);
        });
    }

    public Task UpdateObjectDestination(int id, Vector3 currentPosition, Vector3 destination)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            e.Identity.Position = currentPosition;
            StartCoroutine(HandleUpdateDestination(e, currentPosition, destination));
        });
    }

    IEnumerator HandleUpdateDestination(Entity e, Vector3 currentPosition, Vector3 destination)
    {
        //sync current position with server
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.ResumePositionSync();
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(currentPosition);

        //wait for position to be updated
        yield return new WaitForFixedUpdate();

        //tell the entity to move to location
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.PausePositionSync();
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.SetDestination(destination);

        //set the entity expected position to destination
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(destination);

        //look at destination
        ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.LookAt(destination);

    }

    public Task UpdateObjectAnimation(int id, int animId, float value)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            e.ReferenceHolder.AnimationController.SetAnimationProperty(animId, value);
        });
    }

    public Task InflictDamageTo(int sender, Hit hit)
    {
        return ExecuteWithEntitiesAsync(sender, hit.TargetId, (senderEntity, targetEntity) =>
        {
            if (senderEntity != null)
            {
                WorldCombat.Instance.InflictAttack(senderEntity, targetEntity, hit);
            }
            else
            {
                WorldCombat.Instance.InflictAttack(targetEntity, hit);
            }
        });
    }

    public Task UpdateObjectMoveDirection(int id, int speed, Vector3 direction)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            if (e.Running && speed != e.Stats.RunSpeed)
            {
                // e.UpdateRunSpeed(speed);
            }
            else if (!e.Running && speed != e.Stats.WalkSpeed)
            {
                // e.UpdateWalkSpeed(speed);
            }

            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.UpdateMoveDirection(direction);
        });
    }

    public Task UpdateEntityTarget(int id, int targetId, Vector3 position)
    {
        return ExecuteWithEntitiesAsync(id, targetId, (targeter, targeted) =>
        {
            ((NetworkEntityReferenceHolder)targeter.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
            targeter.Combat.TargetId = targetId;
            targeter.Combat.Target = targeted.transform;
        });
    }

    public Task UpdateMyTarget(int id, int targetId)
    {
        return ExecuteWithEntitiesAsync(id, targetId, (targeter, targeted) =>
        {
            targeter.Combat.TargetId = targetId;
            targeter.Combat.Target = targeted.transform;
        });
    }

    public Task UnsetEntityTarget(int id)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            e.Combat.TargetId = -1;
            e.Combat.Target = null;
        });
    }

    public Task EntityStartAutoAttacking(int id)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            WorldCombat.Instance.EntityStartAutoAttacking(e);
        });
    }

    public Task EntityStopAutoAttacking(int id)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            WorldCombat.Instance.EntityStopAutoAttacking(e);
        });
    }


    public Task ChangeWaitType(int owner, ChangeWaitTypePacket.WaitType moveType, float posX, float posY, float posZ)
    {
        return ExecuteWithEntityAsync(owner, e =>
        {
            e.transform.position = new Vector3(posX, e.transform.position.y, posZ);
            e.UpdateWaitType(moveType);
        });
    }

    public Task StatusUpdate(int id, List<StatusUpdatePacket.Attribute> attributes)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            WorldCombat.Instance.StatusUpdate(e, attributes);
            if (e == PlayerEntity.Instance)
            {
                CharacterInfoWindow.Instance.UpdateValues();
            }
        });
    }

    public Task ChangeMoveType(int owner, bool running)
    {
        return ExecuteWithEntityAsync(owner, e =>
                {
                    e.UpdateMoveType(running);
                });
    }

    // Wait for entity to be fully loaded
    private async Task<Entity> GetEntityAsync(int id)
    {
        Entity entity;
        lock (_objects)
        {
            if (!_objects.TryGetValue(id, out entity))
            {
                //Debug.LogWarning($"GetEntityAsync - Entity {id} not found, retrying...");
            }
        }

        if (entity == null)
        {
            await Task.Delay(500); // Wait for 150 ms retrying

            lock (_objects)
            {
                if (!_objects.TryGetValue(id, out entity))
                {
                    Debug.LogWarning($"GetEntityAsync - Entity {id} not found after retry");
                    return null;
                }
                else
                {
                    // Debug.LogWarning($"GetEntityAsync - Entity {id} found after retry");
                }
            }
        }

        return entity;
    }

    // Execute action after entity is loaded
    private async Task ExecuteWithEntityAsync(int id, Action<Entity> action)
    {
        if (id == GameClient.Instance.CurrentPlayerId)
        {
            _eventProcessor.QueueEvent(() => action(PlayerEntity.Instance));
            return;
        }

        var entity = await GetEntityAsync(id);
        if (entity != null)
        {
            try
            {
                _eventProcessor.QueueEvent(() => action(entity));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Operation failed - Target {id} - Error {ex.Message}");
            }
        }
    }

    // Execute action after 2 entities are loaded
    private async Task ExecuteWithEntitiesAsync(int id1, int id2, Action<Entity, Entity> action)
    {
        var entity1Task = GetEntityAsync(id1);
        var entity2Task = GetEntityAsync(id2);

        await Task.WhenAll(entity1Task, entity2Task);

        var entity1 = await entity1Task;
        var entity2 = await entity2Task;

        if (entity1 != null && entity2 != null)
        {
            try
            {
                _eventProcessor.QueueEvent(() => action(entity1, entity2));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Operation failed - Target {id1} or {id2} - Error {ex.Message}");
            }
        }
    }
}
