using UnityEngine;

public class NPCLevelUP : NPC
{

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void FixedUpdate()
    {
        //기존 행동 원천 제거
    }

    public override void EndAction()
    {
        base.EndAction();
    }

    public override void PlayAnimation(string _animaitonName)
    {
        base.PlayAnimation(_animaitonName);
    }
}
