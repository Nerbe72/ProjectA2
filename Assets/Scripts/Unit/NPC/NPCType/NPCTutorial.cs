using UnityEngine;

public class NPCTutorial : NPC
{
    public override void DoAction()
    {
        base.DoAction();
        // 앉아있는 npc이기 때문에 서는 동작을 수행함
        animator.SetBool(AnimationHash.GetHash("Stand"), true);
        RotateToPlayer();
    }

    public override void EndAction()
    {
        base.EndAction();
        animator.SetBool(AnimationHash.GetHash("Stand"), false);
    }

    public override void PlayAnimation(string _animaitonName)
    {
        base.PlayAnimation(_animaitonName);
    }
}
