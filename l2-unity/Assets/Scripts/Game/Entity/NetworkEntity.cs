using UnityEngine;

public abstract class NetworkEntity : Entity
{

    public override void Initialize()
    {
        base.Initialize();
    }

    public override float UpdateMAtkSpeed(int mAtkSpd)
    {
        float converted = base.UpdateMAtkSpeed(mAtkSpd);
        AnimationController.SetMAtkSpd(converted);

        return converted;
    }

    public override float UpdatePAtkSpeed(int pAtkSpd, float multiplier)
    {
        float converted = base.UpdatePAtkSpeed(pAtkSpd, multiplier);
        AnimationController.SetPAtkSpd(converted);

        return converted;
    }

    public override float UpdateRunSpeed(int speed, float multiplier)
    {
        float converted = base.UpdateRunSpeed(speed, multiplier);
        AnimationController.SetRunSpeed(converted);
        return converted;
    }
    public override float UpdateWalkSpeed(int speed, float multiplier)
    {
        float converted = base.UpdateWalkSpeed(speed, multiplier);
        AnimationController.SetWalkSpeed(converted);
        return converted;
    }
}