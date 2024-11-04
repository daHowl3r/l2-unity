using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerSpawner : EntitySpawnStrategy<PlayerAppearance, PlayerStats, PlayerStatus>
{
    public PlayerSpawner(EventProcessor eventProcessor)
        : base(eventProcessor)
    {
    }

    protected override void SpawnEntity(NetworkIdentity identity, PlayerStatus status,
        PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        identity.SetPosY(World.Instance.GetGroundHeight(identity.Position));
        identity.EntityType = EntityType.Player;

        CharacterRace race = (CharacterRace)appearance.Race;

        CharacterModelType raceId = CharacterModelTypeParser.ParseRace(race, appearance.Race, identity.IsMage);

        GameObject go = CharacterBuilder.Instance.BuildCharacterBase(raceId, appearance, identity.EntityType);
        InitializeGameObject(go, identity);

        PlayerEntity player = go.GetComponent<PlayerEntity>();
        InitializePlayer(player, identity, status, stats, appearance, race, raceId, running);

        AddEntity(identity, player);
    }

    protected override void FindAndUpdateEntity(NetworkIdentity identity, PlayerStatus status, PlayerStats stats,
    PlayerAppearance appearance, bool running)
    {
        UpdateEntity(PlayerEntity.Instance, identity, status, stats, appearance, running);
    }

    protected override void UpdateEntity(Entity entity, NetworkIdentity identity,
        PlayerStatus status, PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        Debug.LogWarning("[" + Thread.CurrentThread.ManagedThreadId + "] UPDATE ENTITY FUNC");

        Debug.LogWarning(entity);
        Debug.LogWarning(entity.transform);
        Debug.LogWarning(entity.Identity.Id);
        Debug.LogWarning(entity == PlayerEntity.Instance);
        Debug.LogWarning(entity.Identity.Id == GameClient.Instance.CurrentPlayerId);

        entity.gameObject.layer = LayerMask.NameToLayer("Player");

        UpdateEntityComponents((PlayerEntity)entity, identity, status, stats, appearance, running);

        // Player-specific updates
        CharacterInfoWindow.Instance.UpdateValues();
        InventoryWindow.Instance.RefreshWeight();
        GameManager.Instance.OnPlayerInfoReceive();
        NetworkTransformShare.Instance.SharePosition();
    }

    private void InitializeGameObject(GameObject go, NetworkIdentity identity)
    {
        float rotation = VectorUtils.ConvertRotToUnity(identity.Heading);
        go.transform.eulerAngles = new Vector3(go.transform.eulerAngles.x, rotation, go.transform.eulerAngles.z);
        go.transform.position = identity.Position;
        go.transform.name = "_Player";
        go.layer = LayerMask.NameToLayer("Invisible");
    }

    private void InitializePlayer(PlayerEntity player, NetworkIdentity identity, Status status,
        Stats stats, PlayerAppearance appearance, CharacterRace race, CharacterModelType raceId, bool running)
    {
        player.Status = status;
        player.Identity = identity;
        player.Stats = stats;
        player.Appearance = appearance;
        player.Race = race;
        player.RaceId = raceId;

        var go = player.gameObject;
        go.GetComponent<NetworkTransformShare>().enabled = true;
        go.GetComponent<PlayerController>().enabled = true;
        go.GetComponent<PlayerController>().Initialize();

        go.SetActive(true);
        go.GetComponentInChildren<PlayerAnimationController>().Initialize();
        go.GetComponent<Gear>().Initialize(player.Identity.Id, player.RaceId);

        player.Initialize();
        player.UpdateMoveType(running);

        CameraController.Instance.enabled = true;
        CameraController.Instance.SetTarget(go);
    }

    protected override void AddEntity(NetworkIdentity identity, Entity player)
    {
        WorldSpawner.Instance.AddObject(identity.Id, player);
    }

    public static void UpdateEntityComponents(PlayerEntity entity, NetworkIdentity identity, PlayerStatus status,
    PlayerStats stats, PlayerAppearance appearance, bool running)
    {
        Debug.LogWarning("UpdateEntityComponents");
        UpdateIdentityAndStatus(entity, identity, status);
        CheckAndHandleLevelUp(entity, stats);
        UpdateStatsAndAppearance(entity, stats, appearance, running);
    }

    private static void UpdateIdentityAndStatus(PlayerEntity entity, NetworkIdentity identity, PlayerStatus status)
    {
        Debug.LogWarning("UpdateIdentityAndStatus");
        entity.Identity.UpdateEntity(identity);
        ((PlayerStatus)entity.Status).UpdateStatus(status);
    }

    private static void CheckAndHandleLevelUp(PlayerEntity entity, Stats stats)
    {
        Debug.LogWarning("CheckAndHandleLevelUp");
        if (entity.Stats.Level != 0 && stats.Level > entity.Stats.Level)
        {
            Debug.LogWarning("Entity level up!");
            WorldCombat.Instance.EntityCastSkill(entity, 2122);
        }
    }

    private static void UpdateStatsAndAppearance(PlayerEntity entity, PlayerStats stats,
        PlayerAppearance appearance, bool running)
    {
        Debug.LogWarning("UpdateStatsAndAppearance");
        Debug.LogWarning(stats.MaxHp);
        Debug.LogWarning(entity);
        ((PlayerStats)entity.Stats).UpdateStats(stats);
        Debug.LogWarning(entity.Stats.MaxHp);
        entity.UpdateMoveType(running);

        ((PlayerAppearance)entity.Appearance).UpdateAppearance(appearance);

        entity.UpdatePAtkSpeed(stats.PAtkSpd);
        entity.UpdateMAtkSpeed(stats.MAtkSpd);
        entity.UpdateWalkSpeed(stats.WalkSpeed);
        entity.UpdateRunSpeed(stats.RunSpeed);
        entity.EquipAllWeapons();
        entity.EquipAllArmors();
    }
}