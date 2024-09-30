using UnityEngine;

public class MonsterStateBase : StateMachineBehaviour
{
    public MonsterAudioHandler audioHandler;
    protected NetworkAnimationController _networkAnimationController;
    public Animator animator;
    private Entity _entity;

    public void LoadComponents(Animator animator)
    {
        if (audioHandler == null)
        {
            audioHandler = animator.gameObject.GetComponent<MonsterAudioHandler>();
        }
        if (this.animator == null)
        {
            this.animator = animator;
        }
        if (_entity == null)
        {
            _entity = animator.gameObject.GetComponent<Entity>();
        }
        if (_networkAnimationController == null)
        {
            _networkAnimationController = animator.gameObject.GetComponent<NetworkAnimationController>();
        }
    }

    public void PlaySoundAtRatio(EntitySoundEvent soundEvent, float ratio)
    {
        audioHandler.PlaySoundAtRatio(soundEvent, ratio);
    }

    public void SetBool(string name, bool value)
    {
        if (animator.GetBool(name) != value)
        {
            animator.SetBool(name, value);
        }
    }
}
