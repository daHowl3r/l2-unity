using System;
using UnityEngine;

// Used by LOCAL PLAYER
[System.Serializable]
public class PlayerCombat : Combat
{
    private static PlayerCombat _instance;
    public static PlayerCombat Instance { get => _instance; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        PlayerStateMachine.Instance.NotifyEvent(Event.DEAD);
    }

    public override void OnRevive()
    {
        base.OnDeath();
        PlayerStateMachine.Instance.NotifyEvent(Event.REVIVED);
    }

    protected override void OnHit(Hit hit)
    {
        base.OnHit(hit);
        //AudioHandler.PlaySound(EntitySoundEvent.Dmg);
    }
}
