#if UNITY_EDITOR
using UnityEditor;
#endif

public class TableMgr
{
    public TableLocale Locale = new TableLocale();
    public TableDialogue Dialogue = new TableDialogue();
    public TableDialogueLocale DialogueLocale = new TableDialogueLocale();
    public TableRequireCurrency RequireCurrency = new TableRequireCurrency();
    public TableEnemy Enemy = new TableEnemy();
    public TableItem Item = new TableItem();
    public TablePotion Potion = new TablePotion();
    public TableWeapon Weapon = new TableWeapon();
    public TableSkill Skill = new TableSkill();
    public TableProjectile Projectile = new TableProjectile();

    public TableNPC NPC = new TableNPC();
    public TableNPCDialogueCondition NPCDialogueCondition = new TableNPCDialogueCondition();

    public TableScroll Scroll = new TableScroll();

    public TableEnhancement Enhancement = new TableEnhancement();

    public void Init()
    {
#if UNITY_EDITOR
        Locale.Init_CSV("Locale", 1, 0);
        Dialogue.Init_CSV("Dialogue", 1, 0);
        DialogueLocale.Init_CSV("DialogueLocale", 1, 0);
        RequireCurrency.Init_CSV("RequireCurrency", 1, 0);
        Enemy.Init_CSV("Enemy", 1, 0);
        Item.Init_CSV("Item", 1, 0);
        Potion.Init_CSV("Potion", 1, 0);
        Weapon.Init_CSV("Weapon", 1, 0);
        Skill.Init_CSV("Skill", 1, 0);
        Projectile.Init_CSV("Projectile", 1, 0);
        NPC.Init_CSV("NPC", 1, 0);
        NPCDialogueCondition.Init_CSV("NPCDialogueCondition", 1, 0);
        Scroll.Init_CSV("Scroll", 1, 0);
        Enhancement.Init_CSV("Enhancement", 1, 0);
#else
        Locale.Init_Binary("Locale");
        Dialogue.Init_Binary("Dialogue");
        DialogueLocale.Init_Binary("DialogueLocale");
        RequireCurrency.Init_Binary("RequireCurrency");
        Enemy.Init_Binary("Enemy");
        Item.Init_Binary("Item");
        Potion.Init_Binary("Potion");
        Weapon.Init_Binary("Weapon");
        Skill.Init_Binary("Skill");
        Projectile.Init_Binary("Projectile");
        NPC.Init_Binary("NPC");
        NPCDialogueCondition.Init_Binary("NPCDialogueCondition");
        Scroll.Init_Binary("Scroll");
        Enhancement.Init_Binary("Enhancement");
#endif
    }

    public void Save()
    {
        Locale.Save_Binary("Locale");
        Dialogue.Save_Binary("Dialogue");
        DialogueLocale.Save_Binary("DialogueLocale");
        RequireCurrency.Save_Binary("RequireCurrency");
        Enemy.Save_Binary("Enemy");
        Item.Save_Binary("Item");
        Potion.Save_Binary("Potion");
        Weapon.Save_Binary("Weapon");
        Skill.Save_Binary("Skill");
        Projectile.Save_Binary("Projectile");
        NPC.Save_Binary("NPC");
        NPCDialogueCondition.Save_Binary("NPCDialogueCondition");
        Scroll.Save_Binary("Scroll");
        Enhancement.Save_Binary("Enhancement");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
