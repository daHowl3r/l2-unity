using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GameServerPacketHandler : ServerPacketHandler
{
    public override void HandlePacket(byte[] data)
    {
        GameServerPacketType packetType = (GameServerPacketType)data[0];
        if (GameClient.Instance.LogReceivedPackets)
        {
            Debug.Log("[" + Thread.CurrentThread.ManagedThreadId + "] [GameServer] Received packet:" + packetType);
        }

        switch (packetType)
        {
            case GameServerPacketType.Ping:
                OnPingReceive();
                break;
            case GameServerPacketType.VersionCheck:
                OnKeyReceive(data);
                break;
            case GameServerPacketType.LoginFail:
                OnLoginFail(data);
                break;
            case GameServerPacketType.CharSelectionInfo:
                OnCharSelectionInfoReceive(data);
                break;
            case GameServerPacketType.CreatureSay:
                OnMessageReceive(data);
                break;
            case GameServerPacketType.SystemMessage:
                OnSystemMessageReceive(data);
                break;
            case GameServerPacketType.CharSelected:
                OnPlayerInfoReceive(data);
                break;
            case GameServerPacketType.ObjectPosition:
                OnUpdatePosition(data);
                break;
            case GameServerPacketType.RemoveObject:
                OnRemoveObject(data);
                break;
            case GameServerPacketType.ObjectRotation:
                OnUpdateRotation(data);
                break;
            // case GameServerPacketType.ObjectAnimation:
            //     OnUpdateAnimation(data);
            //     break;
            case GameServerPacketType.Attack:
                OnEntityAttack(data);
                break;
            case GameServerPacketType.NpcInfo:
                OnNpcInfoReceive(data);
                break;
            case GameServerPacketType.ObjectMoveTo:
                OnObjectMoveTo(data);
                break;
            case GameServerPacketType.UserInfo:
                OnUserInfoReceive(data);
                break;
            case GameServerPacketType.ObjectMoveDirection:
                OnUpdateMoveDirection(data);
                break;
            case GameServerPacketType.EntityTargetSet:
                OnEntityTargetSet(data);
                break;
            case GameServerPacketType.EntityTargetUnset:
                OnEntityTargetUnset(data);
                break;
            case GameServerPacketType.MyTargetSet:
                OnMyTargetSet(data);
                break;
            case GameServerPacketType.AutoAttackStart:
                OnEntityAttackStanceStart(data);
                break;
            case GameServerPacketType.AutoAttackStop:
                OnEntityAttackStanceEnd(data);
                break;
            case GameServerPacketType.ActionFailed:
                OnActionFailed(data);
                break;
            case GameServerPacketType.ServerClose:
                OnServerClose();
                break;
            case GameServerPacketType.StatusUpdate:
                OnStatusUpdate(data);
                break;
            case GameServerPacketType.ActionAllowed:
                OnActionAllowed(data);
                break;
            case GameServerPacketType.InventoryItemList:
                OnInventoryItemList(data);
                break;
            case GameServerPacketType.InventoryUpdate:
                OnInventoryUpdate(data);
                break;
            case GameServerPacketType.LeaveWorld:
                OnLeaveWorld(data);
                break;
            case GameServerPacketType.RestartReponse:
                OnRestartResponse(data);
                break;
            case GameServerPacketType.ShortcutInit:
                OnShortcutInit(data);
                break;
            case GameServerPacketType.ShortcutRegister:
                OnShortcutRegister(data);
                break;
            case GameServerPacketType.ChangeWaitType:
                OnChangeWaitType(data);
                break;
            case GameServerPacketType.ChangeMoveType:
                OnChangeMoveType(data);
                break;
            case GameServerPacketType.CharCreateOk:
                OnCharCreateOk(data);
                break;
            case GameServerPacketType.CharCreateFail:
                OnCharCreateFail(data);
                break;
            case GameServerPacketType.ValidateLocation:
                OnValidateLocation(data);
                break;
            case GameServerPacketType.DoDie:
                OnEntityDie(data);
                break;
            case GameServerPacketType.Revive:
                OnEntityRevive(data);
                break;
            case GameServerPacketType.TeleportToLocation:
                OnTeleportToLocation(data);
                break;
            case GameServerPacketType.StopMove:
                OnEntityStopMove(data);
                break;
            default:
                Debug.LogWarning($"Received unhandled packet with OPCode [{packetType}].");
                break;
        }
    }

    protected override byte[] DecryptPacket(byte[] data)
    {
        if (GameClient.Instance.LogCryptography)
        {
            Debug.Log("<---- [GAME] ENCRYPTED: " + StringUtils.ByteArrayToString(data));
        }

        GameClient.Instance.GameCrypt.Decrypt(data);

        if (GameClient.Instance.LogCryptography)
        {
            Debug.Log("<---- [GAME] CLEAR: " + StringUtils.ByteArrayToString(data));
        }

        return data;
    }

    private void OnPingReceive()
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        int ping = _timestamp != 0 ? (int)(now - _timestamp) : 0;
        GameClient.Instance.Ping = ping;

        Task.Delay(1000).ContinueWith(t =>
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                ((GameClientPacketHandler)_clientPacketHandler).SendPing();
                _timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }

            Task.Delay(GameClient.Instance.ConnectionTimeoutMs + 100).ContinueWith(t =>
            {
                if (!_tokenSource.IsCancellationRequested)
                {
                    long now2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (now2 - _timestamp >= GameClient.Instance.ConnectionTimeoutMs)
                    {
                        Debug.LogWarning("Connection timed out");
                        _client.Disconnect();
                    }
                }
            }, _tokenSource.Token);
        }, _tokenSource.Token);
    }

    private void OnKeyReceive(byte[] data)
    {
        VersionCheckPacket packet = new VersionCheckPacket(data);

        if (!packet.AuthAllowed)
        {
            Debug.LogWarning("Gameserver connect not allowed.");
            EventProcessor.Instance.QueueEvent(() => GameClient.Instance.Disconnect());
            EventProcessor.Instance.QueueEvent(() => LoginClient.Instance.Disconnect());
            return;
        }

        GameClient.Instance.EnableCrypt(packet.BlowFishKey);

        _eventProcessor.QueueEvent(() => ((GameClientPacketHandler)_clientPacketHandler).SendAuth());

        //_eventProcessor.QueueEvent(() => ((GameClientPacketHandler)_clientPacketHandler).SendPing());
    }

    private void OnLoginFail(byte[] data)
    {
        LoginFailPacket packet = new LoginFailPacket(data);
        EventProcessor.Instance.QueueEvent(() => GameClient.Instance.Disconnect());
        EventProcessor.Instance.QueueEvent(() => LoginClient.Instance.Disconnect());

        Debug.LogWarning($"Gameserver login failed reason: " +
            $"{Enum.GetName(typeof(LoginServerFailPacket.LoginFailedReason), packet.FailedReason)}");
    }

    private void OnCharSelectionInfoReceive(byte[] data)
    {
        CharSelectionInfoPacket packet = new CharSelectionInfoPacket(data);

        CharacterSelector.Instance.Characters = packet.Characters;
        CharacterSelector.Instance.DefaultSelectedSlot = packet.SelectedSlotId;

        if (GameManager.Instance.GameState != GameState.RESTARTING)
        {
            Debug.Log($"Received {packet.Characters.Count} character(s) from server.");

            EventProcessor.Instance.QueueEvent(() =>
            {
                LoginClient.Instance.Disconnect();
                GameClient.Instance.OnAuthAllowed();
            });
        }
        else
        {
            EventProcessor.Instance.QueueEvent(() => GameClient.Instance.OnCharSelectAllowed());
        }
    }

    private void OnCharCreateFail(byte[] data)
    {
        CharCreateFailPacket packet = new CharCreateFailPacket(data);
        CharCreateFailPacket.CreateFailReason reason = (CharCreateFailPacket.CreateFailReason)packet.Reason;
        Debug.LogWarning($"Character creation failed: {reason}.");
    }

    private void OnCharCreateOk(byte[] data)
    {
        CharCreateOkPacket packet = new CharCreateOkPacket(data);
        Debug.Log($"Character creation succeeded.");

        EventProcessor.Instance.QueueEvent(() => GameClient.Instance.OnCharCreateOk());

    }

    private void OnMessageReceive(byte[] data)
    {
        CreatureSayPacket packet = new CreatureSayPacket(data);
        if (packet.MessageType == MessageType.BOAT)
        {
            SystemMessageDat messageData = SystemMessageTable.Instance.GetSystemMessage(packet.SystemMessageId);
            SystemMessage systemMessage = new SystemMessage(null, messageData);
            _eventProcessor.QueueEvent(() => ChatWindow.Instance.ReceiveSystemMessage(systemMessage));
            //TODO: Handle packet SysStringId
            return;
        }

        String sender = packet.Sender;
        String text = packet.Text;

        //TODO: Handle message channel colors
        ChatMessage message = new ChatMessage(sender, text);
        _eventProcessor.QueueEvent(() => ChatWindow.Instance.ReceiveChatMessage(message));
    }

    private void OnSystemMessageReceive(byte[] data)
    {
        SystemMessagePacket packet = new SystemMessagePacket(data);
        SMParam[] smParams = packet.Params;
        int messageId = packet.Id;

        SystemMessageDat messageData = SystemMessageTable.Instance.GetSystemMessage(messageId);
        if (messageData != null)
        {
            SystemMessage systemMessage = new SystemMessage(smParams, messageData);
            _eventProcessor.QueueEvent(() => ChatWindow.Instance.ReceiveSystemMessage(systemMessage));
        }
        else
        {
            _eventProcessor.QueueEvent(() => ChatWindow.Instance.ReceiveSystemMessage(new UnhandledMessage()));
        }

    }

    private void OnPlayerInfoReceive(byte[] data)
    {
        CharSelectedPacket packet = new CharSelectedPacket(data);
        if (GameManager.Instance.GameState != GameState.IN_GAME)
        {
            _eventProcessor.QueueEvent(() =>
            {
                GameClient.Instance.PlayerInfo = packet.PacketPlayerInfo;
                GameManager.Instance.OnCharacterSelect();
            });
        }
        // else
        // {
        //     _eventProcessor.QueueEvent(() =>
        //     {
        //         GameClient.Instance.PlayerInfo = packet.PacketPlayerInfo;
        //     });
        //     World.Instance.OnReceivePlayerInfo(
        //         packet.PacketPlayerInfo.Identity,
        //         packet.PacketPlayerInfo.Status,
        //         packet.PacketPlayerInfo.Stats,
        //         packet.PacketPlayerInfo.Appearance,
        //         packet.PacketPlayerInfo.Running);
        // }
    }

    private void OnUserInfoReceive(byte[] data)
    {
        UserInfoPacket packet = new UserInfoPacket(data);
        if (packet.Identity.Owned)
        {
            World.Instance.OnReceivePlayerInfo(packet.Identity, packet.Status, packet.Stats, packet.Appearance, packet.Running);

            // Additional player information received, only now is the right time to show the UI/World to avoid visual bugs
            GameManager.Instance.OnPlayerInfoReceive();
        }
        else
        {
            World.Instance.OnReceiveUserInfo(packet.Identity, packet.Status, packet.Stats, packet.Appearance, packet.Running);
        }
    }

    private void OnUpdatePosition(byte[] data)
    {
        UpdatePositionPacket packet = new UpdatePositionPacket(data);
        int id = packet.Id;
        Vector3 position = packet.Position;
        World.Instance.UpdateObjectPosition(id, position);
    }

    private void OnValidateLocation(byte[] data)
    {
        ValidateLocationPacket packet = new ValidateLocationPacket(data);
        int id = packet.Id;
        Vector3 position = packet.Location;
        int heading = packet.Heading;
        World.Instance.AdjustObjectPositionAndRotation(id, position, heading);
    }

    private void OnRemoveObject(byte[] data)
    {
        RemoveObjectPacket packet = new RemoveObjectPacket(data);
        World.Instance.RemoveObject(packet.Id);
    }

    private void OnUpdateRotation(byte[] data)
    {
        UpdateRotationPacket packet = new UpdateRotationPacket(data);
        int id = packet.Id;
        float angle = packet.Angle;
        World.Instance.UpdateObjectRotation(id, angle);
    }

    // private void OnUpdateAnimation(byte[] data)
    // {
    //     UpdateAnimationPacket packet = new UpdateAnimationPacket(data);
    //     int id = packet.Id;
    //     int animId = packet.AnimId;
    //     float value = packet.Value;

    //     Debug.Log($"ID: {id} AnimId: {(HumanoidAnimationEvent)animId} Value: {value}");

    //     World.Instance.UpdateObjectAnimation(id, animId, value);
    // }

    private void OnEntityAttack(byte[] data)
    {
        InflictDamagePacket packet = new InflictDamagePacket(data);
        Hit[] hits = packet.Hits;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null)
            {
                WorldCombat.Instance.EntityAttacks(packet.AttackerPosition, packet.SenderId, hits[i]);
            }
        }
    }

    private void OnNpcInfoReceive(byte[] data)
    {
        NpcInfoPacket packet = new NpcInfoPacket(data);
        _eventProcessor.QueueEvent(() => World.Instance.OnReceiveNpcInfo(packet.Identity, packet.Status, packet.Stats, packet.Appearance, packet.Running));
    }

    private void OnObjectMoveTo(byte[] data)
    {
        ObjectMoveToPacket packet = new ObjectMoveToPacket(data);
        World.Instance.UpdateObjectDestination(packet.Id, packet.CurrentPosition, packet.Destination);
    }

    private void OnUpdateMoveDirection(byte[] data)
    {
        UpdateMoveDirectionPacket packet = new UpdateMoveDirectionPacket(data);
        World.Instance.UpdateObjectMoveDirection(packet.Id, packet.Speed, packet.Direction);
    }

    private void OnEntityTargetSet(byte[] data)
    {
        EntityTargetSetPacket packet = new EntityTargetSetPacket(data);
        WorldCombat.Instance.UpdateEntityTarget(packet.EntityId, packet.TargetId, packet.EntityPosition);
    }

    private void OnEntityTargetUnset(byte[] data)
    {
        EntityTargetUnsetPacket packet = new EntityTargetUnsetPacket(data);
        WorldCombat.Instance.UnsetEntityTarget(packet.EntityId);
    }

    private void OnMyTargetSet(byte[] data)
    {
        MyTargetSetPacket packet = new MyTargetSetPacket(data);
        WorldCombat.Instance.UpdateMyTarget(GameClient.Instance.CurrentPlayerId, packet.TargetId);
    }

    private void OnEntityAttackStanceStart(byte[] data)
    {
        Debug.Log("OnEntityAttackStanceStart");
        AttackStanceStartPacket packet = new AttackStanceStartPacket(data);
        WorldCombat.Instance.EntityAttackStanceStart(packet.EntityId);
    }

    private void OnEntityAttackStanceEnd(byte[] data)
    {
        AttackStanceEndPacket packet = new AttackStanceEndPacket(data);
        WorldCombat.Instance.EntityAttackStanceEnd(packet.EntityId);
    }

    private void OnActionFailed(byte[] data)
    {
        ActionFailedPacket packet = new ActionFailedPacket(data);
        Debug.Log($"Action failed");
        _eventProcessor.QueueEvent(() => PlayerEntity.Instance.OnActionFailed());
    }

    private void OnActionAllowed(byte[] data)
    {
        ActionAllowedPacket packet = new ActionAllowedPacket(data);
        _eventProcessor.QueueEvent(() => PlayerEntity.Instance.OnActionAllowed());
    }

    private void OnServerClose()
    {
        Debug.Log("ServerClose received from Gameserver");
        _client.Disconnect();
    }

    private void OnStatusUpdate(byte[] data)
    {
        StatusUpdatePacket packet = new StatusUpdatePacket(data);
        WorldCombat.Instance.StatusUpdate(packet.ObjectId, packet.Attributes);
    }

    private void OnInventoryItemList(byte[] data)
    {
        InventoryItemListPacket packet = new InventoryItemListPacket(data);
        _eventProcessor.QueueEvent(() => PlayerInventory.Instance.SetInventory(packet.Items, packet.OpenWindow));
    }

    private void OnInventoryUpdate(byte[] data)
    {
        InventoryUpdatePacket packet = new InventoryUpdatePacket(data);
        Debug.Log("Updated items: " + packet.Items.Length);
        _eventProcessor.QueueEvent(() => PlayerInventory.Instance.UpdateInventory(packet.Items));
    }

    private void OnLeaveWorld(byte[] data)
    {
#if UNITY_EDITOR
        _client.Disconnect();
#else
        _eventProcessor.QueueEvent(() => {
            Application.Quit();
        }); 
#endif
    }

    private void OnRestartResponse(byte[] data)
    {
        RestartResponsePacket packet = new RestartResponsePacket(data);
        if (packet.Allowed)
        {
            // Do nothing, handle upcoming charselect packet instead
            GameManager.Instance.GameState = GameState.RESTARTING;
        }
    }

    private void OnShortcutInit(byte[] data)
    {
        ShortcutInitPacket packet = new ShortcutInitPacket(data);
        _eventProcessor.QueueEvent(() => PlayerShortcuts.Instance.SetShortcutList(packet.Shortcuts));
    }

    private void OnShortcutRegister(byte[] data)
    {
        ShortcutRegisterPacket packet = new ShortcutRegisterPacket(data);
        _eventProcessor.QueueEvent(() => PlayerShortcuts.Instance.RegisterShortcut(packet.NewShortcut));
    }

    private void OnChangeWaitType(byte[] data)
    {
        ChangeWaitTypePacket packet = new ChangeWaitTypePacket(data);
        Debug.Log("ChangeWaitType: " + packet.Owner + " " + packet.MoveType);
        World.Instance.ChangeWaitType(packet.Owner, packet.MoveType, packet.EntityPosition);
    }

    private void OnChangeMoveType(byte[] data)
    {
        ChangeMoveTypePacket packet = new ChangeMoveTypePacket(data);
        Debug.Log("ChangeMoveType: " + packet.Owner + " running? " + packet.Running);
        World.Instance.ChangeMoveType(packet.Owner, packet.Running);
    }

    private void OnEntityDie(byte[] data)
    {
        DoDiePacket packet = new DoDiePacket(data);
        WorldCombat.Instance.EntityDied(packet.EntityId, packet.ToVillageAllowed, packet.ToClanHallAllowed, packet.ToCastleAllowed, packet.ToSiegeHQAllowed, packet.Sweepable, packet.FixedResAllowed);
    }

    private void OnEntityRevive(byte[] data)
    {
        RevivePacket packet = new RevivePacket(data);
        WorldCombat.Instance.EntityRevived(packet.EntityId);
    }

    private void OnTeleportToLocation(byte[] data)
    {
        TeleportToLocationPacket packet = new TeleportToLocationPacket(data);
        World.Instance.EntityTeleported(packet.EntityId, packet.TeleportTo, packet.LoadingScreen);
    }

    private void OnEntityStopMove(byte[] data)
    {
        ObjectStopMovePacket packet = new ObjectStopMovePacket(data);
        World.Instance.ObjectStoppedMove(packet.Id, packet.CurrentPosition, packet.Heading);
    }
}
