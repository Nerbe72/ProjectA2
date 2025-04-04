using UnityEngine;

public partial class Player : Character, IAttackable, IHurtable
{
    private void Awake()
    {
        if (SingletonManager.player != null)
        {
            Destroy(gameObject);
            return;
        }

        SingletonManager.player = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
