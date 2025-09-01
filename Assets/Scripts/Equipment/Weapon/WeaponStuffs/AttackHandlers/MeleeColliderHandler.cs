using UnityEngine;

// [RequireComponent(typeof(CapsuleCollider))]
public class MeleeColliderHandler : MonoBehaviour
{
    CapsuleCollider capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void OnAttackStart()
    {
        if (capsuleCollider != null)
            capsuleCollider.enabled = true;
    }

    public void OnAttackEnd()
    {
        if (capsuleCollider != null)
            capsuleCollider.enabled = false;
    }
}
