public partial class Player : Character
{
    private TargetManager targetManager;
    private CameraManager cameraManager;

    private void SetTargeted()
    {
        if (InputManager.IgnoreInput) return;

        if (IsFlagged(StateFlags.Targeted))
        {
            SetFlag(StateFlags.Targeted, false);

            if (targetManager.CurrentTarget != null)
                targetManager.UnSetTarget();
            else
                targetManager.SetTarget(cameraManager.main.transform);
        }

        animator.SetBool(AnimationHash.GetHash(ActionType.Guided), targetManager.CurrentTarget != null);

        if (cameraManager.CurrentCamType == CameraType.Main)
        {

        }
        cameraManager.SetCamera(targetManager.CurrentTarget == null ? CameraType.Main : CameraType.Target, targetManager.CurrentTarget);

    }
}
