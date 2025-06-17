using UnityEngine;

public class LevelUpDialogueWindow : WindowBase
{
    public int InitializationPriority => 4;

    private Animator animator;

    private LevelUP levelUp;
    private Level level;
    private Status status;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        animator = GetComponent<Animator>();
        levelUp = GetComponentInChildren<LevelUP>();
        level = GetComponentInChildren<Level>();
        status = GetComponentInChildren<Status>();

        levelUp.OnTempLevelChanged += ChangeAllValues;
        levelUp.OnApplyClicked += ReloadAllUIs;
        WindowType = WindowType.DialogueWindow;

        gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        animator.SetBool(AnimationHash.GetHash("Open"), true);
    }

    private void ChangeAllValues(Levels _tempLevels)
    {
        // temp 추가된 레벨에 맞게 능력치 및 기본능력값 변경
        level.TempUpdateLevel(_tempLevels);
        status.UpdateStatus(_tempLevels);
    }

    private void ReloadAllUIs()
    {
        levelUp.UpdateLevel();
        level.UpdateLevel();
        status.UpdateStatus();
    }
}
