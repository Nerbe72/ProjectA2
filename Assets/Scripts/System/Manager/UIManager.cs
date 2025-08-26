public static class UIManager
{
    public static void OffBasicUI()
    {
        var status = Singleton.Get<StatusUI>();
        if (status != null) status.gameObject.SetActive(false);

        var menu = Singleton.Get<MenuUI>();
        if (menu != null) menu.gameObject.SetActive(false);

        var skill = Singleton.Get<SkillIndicator>();
        if (skill != null) skill.gameObject.SetActive(false);
    }

    public static void OnBasicUI()
    {
        //게임 종료 시 일부 UI 오브젝트가 파괴되었을 수 있으므로 개별 체크
        var status = Singleton.Get<StatusUI>();
        if (status != null) status.gameObject.SetActive(true);

        var menu = Singleton.Get<MenuUI>();
        if (menu != null) menu.gameObject.SetActive(true);

        var skill = Singleton.Get<SkillIndicator>();
        if (skill != null) skill.gameObject.SetActive(true);
    }
}
