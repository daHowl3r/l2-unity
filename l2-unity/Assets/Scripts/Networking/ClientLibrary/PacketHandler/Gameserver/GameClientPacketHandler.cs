﻿using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameClientPacketHandler : ClientPacketHandler
{
    protected override void EncryptPacket(ClientPacket packet)
    {
        //base.EncryptPacket(packet);

        byte[] data = packet.GetData();

        GameClient.Instance.GameCrypt.Encrypt(data);

        if (GameClient.Instance.LogCryptography)
        {
            Debug.Log("----> [GAME] ENCRYPTED: " + StringUtils.ByteArrayToString(data));
        }

        packet.SetData(data);
    }

    public void SendPing()
    {
        PingPacket packet = new PingPacket();
        SendPacket(packet);
    }

    public void SendProtocolVersion()
    {
        ProtocolVersionPacket packet = new ProtocolVersionPacket(GameManager.Instance.ProtocolVersion);
        SendPacket(packet);
    }

    public void SendAuth()
    {
        GameAuthRequestPacket authPacket =
            new GameAuthRequestPacket(LoginClient.Instance.Account, GameClient.Instance.PlayKey1, GameClient.Instance.PlayKey2,
            GameClient.Instance.SessionKey1, GameClient.Instance.SessionKey2);


        SendPacket(authPacket);
    }

    public void SendMessage(string message)
    {
        SendMessagePacket packet = new SendMessagePacket(message, MessageType.ALL, 0);
        SendPacket(packet);
    }

    public void ValidatePosition(Vector3 position, int heading)
    {
        ValidatePositionPacket packet = new ValidatePositionPacket(position, heading);
        SendPacket(packet);
    }

    public void SendLoadWorld()
    {
        EnterWorldPacket packet = new EnterWorldPacket();
        SendPacket(packet);
    }

    public void UpdateRotation(float angle)
    {
        RequestRotatePacket packet = new RequestRotatePacket(angle);
        SendPacket(packet);
    }

    public void UpdateAnimation(byte anim, float value)
    {
        RequestAnimPacket packet = new RequestAnimPacket(anim, value);
        SendPacket(packet);
    }

    public void RequestAttackForce(int targetId)
    {
        RequestAttackPacket packet = new RequestAttackPacket(targetId);
        SendPacket(packet);
    }

    public void UpdateMoveDirection(Vector3 direction, int heading)
    {
        RequestMoveDirectionPacket packet = new RequestMoveDirectionPacket(direction, heading);
        SendPacket(packet);
    }

    public void SendRequestSetTarget(int targetId)
    {
        RequestSetTargetPacket packet = new RequestSetTargetPacket(targetId, false);
        SendPacket(packet);
    }

    public void SendRequestUnsetTarget(bool cancelCast)
    {
        RequestUnsetTargetPacket packet = new RequestUnsetTargetPacket(cancelCast);
        SendPacket(packet);
    }

    public void SendRequestAction(int objectId)
    {
        RequestActionPacket packet = new RequestActionPacket(objectId);
        SendPacket(packet);
    }

    public void SendRequestSelectCharacter(int slot)
    {
        RequestCharSelectPacket packet = new RequestCharSelectPacket(slot);
        SendPacket(packet);
    }

    public void SendRequestOpenInventory()
    {
        RequestInventoryOpenPacket packet = new RequestInventoryOpenPacket();
        SendPacket(packet);
    }

    public override void SendPacket(ClientPacket packet)
    {
        if (GameClient.Instance.LogSentPackets)
        {
            GameClientPacketType packetType = (GameClientPacketType)packet.GetPacketType();
            Debug.Log("[" + Thread.CurrentThread.ManagedThreadId + "] [GameServer] Sending packet:" + packetType);
        }

        if (GameClient.Instance.LogCryptography)
        {
            Debug.Log("----> [GAME] CLEAR: " + StringUtils.ByteArrayToString(packet.GetData()));
        }

        if (_client.CryptEnabled)
        {
            EncryptPacket(packet);
        }

        _client.SendPacket(packet);
    }

    public void UseItem(int objectId)
    {
        UseItemPacket packet = new UseItemPacket(objectId);
        SendPacket(packet);
    }

    public void UnEquipItem(int position)
    {
        RequestUnEquipPacket packet = new RequestUnEquipPacket(position);
        SendPacket(packet);
    }

    public void UpdateInventoryOrder(List<InventoryOrder> orders)
    {
        RequestInventoryUpdateOrderPacket packet = new RequestInventoryUpdateOrderPacket(orders);
        SendPacket(packet);
    }

    public void DestroyItem(int objectId, int quantity)
    {
        RequestDestroyItemPacket packet = new RequestDestroyItemPacket(objectId, quantity);
        SendPacket(packet);
    }

    public void DropItem(int objectId, int quantity)
    {
        RequestDropItemPacket packet = new RequestDropItemPacket(objectId, quantity);
        SendPacket(packet);
    }

    public void RequestDisconnect()
    {
        DisconnectPacket packet = new DisconnectPacket();
        SendPacket(packet);
    }

    public void RequestRestart()
    {
        RequestRestartPacket packet = new RequestRestartPacket();
        SendPacket(packet);
    }

    public void RequestAddShortcut(int type, int id, int slot)
    {
        RequestShortcutRegPacket packet = new RequestShortcutRegPacket(type, id, slot);
        SendPacket(packet);
    }

    public void RequestRemoveShortcut(int oldSlot)
    {
        RequestShortcutDelPacket packet = new RequestShortcutDelPacket(oldSlot);
        SendPacket(packet);
    }

    public void RequestActionUse(int actionId)
    {
        bool isControlPressed = false;
        bool isShiftPressed = false;
        RequestActionUsePacket packet = new RequestActionUsePacket(actionId, isControlPressed, isShiftPressed);
        SendPacket(packet);
    }

    public void SendRequestCreateCharacter(string name, CharacterRace race, CharacterSex sex, CharacterClass clazz, int hairstyle, int haircolor, int face)
    {
        RequestCharCreatePacket packet = new RequestCharCreatePacket(name, race, sex, clazz, hairstyle, haircolor, face);
        SendPacket(packet);
    }

    public void SendRequestRestartPoint(int restartPoint)
    {
        RequestRestartPointPacket packet = new RequestRestartPointPacket(restartPoint);
        SendPacket(packet);
    }
}