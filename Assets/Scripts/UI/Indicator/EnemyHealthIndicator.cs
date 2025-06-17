using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthIndicator : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider slider;

    private IHurtable currentTarget;

    public int InitializationPriority => 5;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Singleton.Get<TargetManager>().OnTargetChanged += OnTargetChanged;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        var targetManager = Singleton.Get<TargetManager>();
        if (targetManager != null)
        {
            targetManager.OnTargetChanged -= OnTargetChanged;
        }
        UnsubscribeCurrentTarget();
    }

    private void OnTargetChanged(Target target)
    {
        UnsubscribeCurrentTarget();

        if (target != null && target.owner != null)
        {
            currentTarget = target.owner;
            healthText.text = Singleton.Get<TableDataManager>().Table.Locale.Get((target.owner as Character).stats.ID, GameManager.CurrentLocale);
            currentTarget.OnHealthChanged += UpdateHealth;

            // 체력 즉시 갱신 (최초 타겟팅 시)
            var enemy = currentTarget as Enemy;
            if (enemy != null && enemy.stats != null) 
            {
                UpdateHealth(enemy.GetCurrentHealth(), enemy.stats.Health);
            }
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UnsubscribeCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnHealthChanged -= UpdateHealth;
            currentTarget = null;
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (slider != null)
            slider.value = (float)current / max;
    }
}
