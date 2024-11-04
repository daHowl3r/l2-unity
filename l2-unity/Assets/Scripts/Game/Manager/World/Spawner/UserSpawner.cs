using System.Threading;
using UnityEngine;

public class UserSpawner : EntitySpawnStrategy<PlayerAppearance, Stats, PlayerStatus>
{
    private Transform _usersContainer;
    public UserSpawner(EventProcessor eventProcessor, Transform usersContainer)
        : base(eventProcessor)
    {
        _usersContainer = usersContainer;
    }

    #region Spawn
    protected override void SpawnEntity(NetworkIdentity identity, PlayerStatus status,
        Stats stats, PlayerAppearance appearance, bool running)
    {
        Debug.Log("Spawn User");
        identity.SetPosY(World.Instance.GetGroundHeight(identity.Position));
        identity.EntityType = EntityType.User;

        CharacterRace race = (CharacterRace)appearance.Race;
        CharacterModelType raceId = CharacterModelTypeParser.ParseRace(race, appearance.Race, identity.IsMage);

        GameObject go = CharacterBuilder.Instance.BuildCharacterBase(raceId, appearance, identity.EntityType);
        InitializeGameObject(go, identity);

        NetworkHumanoidEntity user = go.GetComponent<NetworkHumanoidEntity>();
        InitializeUser(user, identity, status, stats, appearance, race, raceId, running);

        go.SetActive(true);
        go.transform.SetParent(_usersContainer.transform);

        UpdateEntityComponents(user, identity, status, stats, appearance, running);

        AddEntity(identity, user);
    }

    private void InitializeGameObject(GameObject go, NetworkIdentity identity)
    {
        go.transform.position = identity.Position;
        float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        go.transform.eulerAngles = new Vector3(go.transform.eulerAngles.x, rotation, go.transform.eulerAngles.z);
        go.transform.name = identity.Name;
    }

    private void InitializeUser(NetworkHumanoidEntity user, NetworkIdentity identity, PlayerStatus status,
        Stats stats, PlayerAppearance appearance, CharacterRace race, CharacterModelType raceId, bool running)
    {
        user.Status = status;
        user.Identity = identity;
        user.Appearance = appearance;
        user.Stats = stats;
        user.Race = race;
        user.RaceId = raceId;
        user.UpdateMoveType(running);

        ((NetworkEntityReferenceHolder)user.ReferenceHolder).NetworkTransformReceive.enabled = true;

        user.ReferenceHolder.AnimationController.Initialize();
        user.ReferenceHolder.Gear.Initialize(user.Identity.Id, user.RaceId);
        user.Initialize();
    }

    protected override void AddEntity(NetworkIdentity identity, Entity player)
    {
        WorldSpawner.Instance.AddObject(identity.Id, player);
        WorldSpawner.Instance.AddPlayer(identity.Id, player);
    }
    #endregion

    #region Update
    protected override void FindAndUpdateEntity(NetworkIdentity identity, PlayerStatus status, Stats stats,
    PlayerAppearance appearance, bool running)
    {
        UpdateEntity(PlayerEntity.Instance, identity, status, stats, appearance, running);
    }

    protected override void UpdateEntity(Entity entity, NetworkIdentity identity,
        PlayerStatus status, Stats stats, PlayerAppearance appearance, bool running)
    {
        Debug.LogWarning("[" + Thread.CurrentThread.ManagedThreadId + "] UPDATE ENTITY FUNC");

        UpdateEntityComponents((NetworkHumanoidEntity)entity, identity, status, stats, appearance, running);
    }

    private void UpdateEntityComponents(NetworkHumanoidEntity entity, NetworkIdentity identity, PlayerStatus status,
    Stats stats, PlayerAppearance appearance, bool running)
    {
        Debug.LogWarning("UpdateEntityComponents");
        UpdateIdentityAndStatus(entity, identity, status);
        CheckAndHandleLevelUp(entity, stats);
        UpdateStatsAndAppearance(entity, stats, appearance, running);
    }

    private void UpdateIdentityAndStatus(NetworkHumanoidEntity entity, NetworkIdentity identity, PlayerStatus status)
    {
        Debug.LogWarning("UpdateIdentityAndStatus");
        entity.Identity.UpdateEntityPartial(identity);
        ((NetworkEntityReferenceHolder)entity.ReferenceHolder).NetworkTransformReceive.SetNewPosition(identity.Position);
        ((PlayerStatus)entity.Status).UpdateStatus(status);
    }

    private void UpdateStatsAndAppearance(NetworkHumanoidEntity entity, Stats stats,
        PlayerAppearance appearance, bool running)
    {
        entity.Stats.UpdateStats(stats);
        entity.Running = running;
        ((PlayerAppearance)entity.Appearance).UpdateAppearance(appearance);

        Debug.LogWarning("===");
        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
        ((NetworkHumanoidEntity)entity).EquipAllArmors();
    }
    #endregion
}