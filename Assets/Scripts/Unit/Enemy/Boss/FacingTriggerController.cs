using UnityEngine;

public class FacingTriggerController : MonoBehaviour
{
    private Boss owner;

    private void Awake()
    {
        owner = GetComponentInParent<Boss>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        var player = other.GetComponent<Player>();

        if (player == null) return;

        owner.SetFaced();
        gameObject.SetActive(false);
    }
}
