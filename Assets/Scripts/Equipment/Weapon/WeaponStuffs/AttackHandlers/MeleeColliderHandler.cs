using UnityEngine;

[RequireComponent (typeof(CapsuleCollider))]
public class MeleeColliderHandler : MonoBehaviour
{
    CapsuleCollider capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void OnAttackStart()
    {
        capsuleCollider.enabled = true;
    }

    public void OnAttackEnd()
    {
        capsuleCollider.enabled = false;
    }
}
