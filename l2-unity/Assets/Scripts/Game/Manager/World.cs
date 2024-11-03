using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

    private List<int> _idBag = new List<int>();
    private ConcurrentDictionary<int, Entity> _players = new ConcurrentDictionary<int, Entity>();
    private ConcurrentDictionary<int, Entity> _npcs = new ConcurrentDictionary<int, Entity>();
    private ConcurrentDictionary<int, Entity> _objects = new ConcurrentDictionary<int, Entity>();

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
        // _npcPlaceHolder = Resources.Load<GameObject>("Prefab/Npc");
        // _monsterPlaceholder = Resources.Load<GameObject>("Data/Animations/LineageMonsters/gremlin/gremlin_prefab");
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

    public Task RemoveObject(int id)
    {
        Debug.LogWarning("DESTROY ID: " + id);
        if (IsEntityPresent(id, true))
        {
            return ExecuteWithEntityAsync(id, e =>
            {
                _players.TryRemove(id, out Entity removed);
                _npcs.TryRemove(id, out Entity removed2);
                _objects.TryRemove(id, out Entity removed3);

                Debug.LogWarning("Gameobject destroyed : " + e.gameObject.name);

                NameplatesManager.Instance.RemoveNameplate(id);

                Destroy(e.gameObject);
            });
        }
        else
        {
            return null;
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
        if (!IsEntityPresent(identity.Id))
        {
            _eventProcessor.QueueEvent(() => SpawnPlayer(identity, status, stats, appearance, running));
        }
        else
        {
            // Dont need to block thread
            Task task = new Task(async () =>
            {
                var entity = await GetEntityAsync(identity.Id);

                if (entity != null)
                {
                    _eventProcessor.QueueEvent(() => UpdatePlayer(entity, identity, status, stats, appearance, running));
                }
            });
            task.Start();
        }
    }

    public void SpawnPlayer(NetworkIdentity identity, PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        identity.SetPosY(GetGroundHeight(identity.Position));
        identity.EntityType = EntityType.Player;

        CharacterRace race = (CharacterRace)appearance.Race;
        CharacterModelType raceId = CharacterModelTypeParser.ParseRace(race, appearance.Race, identity.IsMage);

        GameObject go = CharacterBuilder.Instance.BuildCharacterBase(raceId, appearance, identity.EntityType);

        float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);

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

        _players.TryAdd(identity.Id, player);
        _objects.TryAdd(identity.Id, player);
    }

    public void UpdatePlayer(Entity entity, NetworkIdentity identity, PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        entity.gameObject.layer = LayerMask.NameToLayer("Player");

        ((PlayerEntity)entity).Identity.UpdateEntity(identity);
        ((PlayerStatus)entity.Status).UpdateStatus(status);

        if (entity.Stats.Level != 0 && stats.Level > entity.Stats.Level)
        {
            Debug.LogWarning("Entity level up!");
            WorldCombat.Instance.EntityCastSkill(entity, 2122);
        }

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
        InventoryWindow.Instance.RefreshWeight();
        GameManager.Instance.OnPlayerInfoReceive();
    }

    public void OnReceiveUserInfo(NetworkIdentity identity, PlayerStatus status, Stats stats, PlayerAppearance appearance, bool running)
    {
        if (!IsEntityPresent(identity.Id))
        {
            Debug.LogWarning("USER NOT PRESENT");
            _eventProcessor.QueueEvent(() => SpawnUser(identity, status, stats, appearance, running));
        }
        else
        {
            Debug.LogWarning("USER PRESENT");
            // Dont need to block thread
            Task task = new Task(async () =>
            {
                var entity = await GetEntityAsync(identity.Id);

                if (entity != null)
                {
                    _eventProcessor.QueueEvent(() => UpdateUser(entity, identity, status, stats, appearance, running));
                }
            });
            task.Start();
        }
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
        float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);

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

        _players.TryAdd(identity.Id, user);
        _objects.TryAdd(identity.Id, user);
    }

    public void UpdateUser(Entity entity, NetworkIdentity identity, PlayerStatus status, Stats stats, PlayerAppearance appearance, bool running)
    {
        entity.Identity.UpdateEntityPartial(identity);
        ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetNewPosition(identity.Position);

        ((PlayerStatus)entity.Status).UpdateStatus(status);

        if (entity.Stats.Level != 0 && stats.Level > entity.Stats.Level)
        {
            Debug.LogWarning("Entity level up!");
            WorldCombat.Instance.EntityCastSkill(entity, 2122);
        }

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
        // // Dont need to block thread
        // Task task = new Task(async () =>
        // {
        //     var entity = await GetEntityAsync(identity.Id);

        //     if (entity == null)
        //     {
        //         _eventProcessor.QueueEvent(() => SpawnNpc(identity, status, stats, appearance, running));
        //     }
        //     else
        //     {
        //         _eventProcessor.QueueEvent(() => UpdateNpc(entity, identity, status, stats, appearance, running));
        //     }
        // });
        // task.Start();
        if (!IsEntityPresent(identity.Id))
        {
            _eventProcessor.QueueEvent(() => SpawnNpc(identity, status, stats, appearance, running));
        }
        else
        {
            // Dont need to block thread
            Task task = new Task(async () =>
            {
                var entity = await GetEntityAsync(identity.Id);

                if (entity != null)
                {
                    _eventProcessor.QueueEvent(() => UpdateNpc(entity, identity, status, stats, appearance, running));
                }
            });
            task.Start();
        }
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

        identity.EntityType = npcgrp.Type;

        GameObject go = ModelTable.Instance.GetNpc(npcgrp.Mesh);
        if (go == null)
        {
            if (identity.EntityType == EntityType.Monster)
            {
                go = _monsterPlaceholder;
            }
            else
            {
                go = _npcPlaceHolder;
            }

            Debug.LogError($"Npc {identity.NpcId} could not be loaded correctly, loaded placeholder instead.");
        }

        identity.SetPosY(GetGroundHeight(identity.Position));
        GameObject npcGo = Instantiate(go, identity.Position, Quaternion.identity);
        //NpcData npcData = new NpcData(npcName, npcgrp);


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

        _npcs.TryAdd(identity.Id, npc);
        _objects.TryAdd(identity.Id, npc);
    }

    public void UpdateNpc(Entity entity, NetworkIdentity identity, NpcStatus status, Stats stats, Appearance appearance, bool running)
    {
        entity.Identity.UpdateEntityPartial(identity);
        ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetNewPosition(identity.Position);
        // float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        // ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
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
        if (Physics.Raycast(pos + Vector3.up * 1.5f, Vector3.down, out hit, 3.5f, _groundMask))
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

            float rotation = VectorUtils.ConvertRotToUnity(heading);
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                PlayerTransformReceive.Instance.SetNewPosition(position);
                NetworkCharacterControllerShare.Instance.Heading = heading;
            }
            else
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
            }
        });
    }


    public Task NpcHtmlReceived(int objectId, string html, int itemId)
    {
        _eventProcessor.QueueEvent(() => NpcHtmlWindow.Instance.RefreshContent(objectId, html, itemId));
        return ExecuteWithEntityAsync(objectId, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.LookAt(PlayerEntity.Instance.transform);
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
        // Debug.LogWarning("Entity move to destination: " + destination);

        NetworkEntityReferenceHolder referenceHolder = (NetworkEntityReferenceHolder)e.ReferenceHolder;
        //sync current position with server
        referenceHolder.NetworkTransformReceive.ResumePositionSync();
        referenceHolder.NetworkTransformReceive.SetNewPosition(currentPosition);

        //wait for position to be updated
        yield return new WaitForFixedUpdate();

        //tell the entity to move to location
        referenceHolder.NetworkTransformReceive.PausePositionSync();

        if (e.Combat.AttackTarget == null)
        {
            referenceHolder.NetworkCharacterControllerReceive.SetDestination(destination, 0);

            //set the entity expected position to destination
            referenceHolder.NetworkTransformReceive.SetNewPosition(destination);
        }
        else
        {
            referenceHolder.NetworkCharacterControllerReceive.SetDestination(destination, WorldCombat.Instance.GetRealAttackRange(e, e.Combat.AttackTarget));
        }

        //look at destination
        referenceHolder.NetworkTransformReceive.LookAt(destination);
    }

    public Task UpdateObjectMoveDirection(int id, int speed, Vector3 direction)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.UpdateMoveDirection(direction);
        });
    }


    public Task ObjectStoppedMove(int id, Vector3 position, int heading)
    {
        return ExecuteWithEntityAsync(id, e =>
        {
            Debug.LogWarning($"[{e.transform.name}] ObjectStoppedMove");
            e.Identity.Position = position;
            e.Identity.Heading = heading;

            float rotation = VectorUtils.ConvertRotToUnity(heading);

            if (id == GameClient.Instance.CurrentPlayerId)
            {
                Debug.LogWarning("Should not happen");
                PlayerTransformReceive.Instance.SetNewPosition(position);
                NetworkCharacterControllerShare.Instance.Heading = heading;
            }
            else
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.ResetDestination();
                e.OnStopMoving();
            }
        });
    }

    public Task ChangeWaitType(int owner, ChangeWaitTypePacket.WaitType moveType, Vector3 entityPosition)
    {
        return ExecuteWithEntityAsync(owner, e =>
        {
            if (owner != GameClient.Instance.CurrentPlayerId)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(entityPosition);
            }
            else
            {
                PlayerTransformReceive.Instance.SetNewPosition(entityPosition);
            }

            e.UpdateWaitType(moveType);
        });
    }

    public Task ChangeMoveType(int owner, bool running)
    {
        return ExecuteWithEntityAsync(owner, e =>
                {
                    e.UpdateMoveType(running);
                });
    }

    public Task EntityTeleported(int entityId, Vector3 teleportTo, bool loadingScreen)
    {
        return ExecuteWithEntityAsync(entityId, e =>
        {
            if (entityId != GameClient.Instance.CurrentPlayerId)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(teleportTo);
            }
            else
            {
                if (loadingScreen)
                {
                    Debug.LogWarning("TODO: HANDLE TELEPORT LOADING SCREEN");
                }
                PlayerTransformReceive.Instance.SetNewPosition(teleportTo);
                GameClient.Instance.ClientPacketHandler.NotifyAppearing();
            }

            e.Identity.Position = teleportTo;
        });
    }

    // Wait for entity to be fully loaded
    private async Task<Entity> GetEntityAsync(int id)
    {
        Entity entity;
        if (!_objects.TryGetValue(id, out entity))
        {
            Debug.LogWarning($"GetEntityAsync - Entity {id} not found, retrying...");
        }

        if (entity == null)
        {
            await Task.Delay(300);
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

        return entity;
    }

    private bool IsEntityPresent(int id)
    {
        return IsEntityPresent(id, false);
    }

    private bool IsEntityPresent(int id, bool remove)
    {
        lock (_idBag)
        {
            if (_idBag.Contains(id))
            {
                if (remove)
                {
                    _idBag.Remove(id);
                }
                return true;
            }
            else
            {
                _idBag.Add(id);
                return false;
            }
        }
    }


    // Execute action after entity is loaded
    public async Task ExecuteWithEntityAsync(int id, Action<Entity> action)
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
    public async Task ExecuteWithEntitiesAsync(int id1, int id2, Action<Entity, Entity> action)
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
