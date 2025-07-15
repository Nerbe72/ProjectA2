using System;

public enum Map
{
    None = -1,
    World_FrontVillage = 4,
    Village = 5,
    Dungeon = 6,
}

public enum Locale
{
    Korean,
    English,
    Japanese,
    Count
}

public enum UnitType
{
    Player,
    Enemy,
    Object,
}

public enum EnemyType
{
    None,
    Goblin,
    Skeleton,
}

public enum WindowType
{
    SettingWindow,
    NormalWindow,
    GachaWindow,
    DialogueWindow,
}

public enum LevelType
{
    Total,
    Health,
    Strength,
    Dexterity,
    Intelligent,
    Count
}

public enum StatType
{
    Health,
    Damage,
    Defense,
    Count
}

[Flags]
public enum StateFlags
{
    None = 0,
    Attack = 1 << 0,
    Jump = 1 << 1,
    Run = 1 << 2,
    Dodge = 1 << 3,
    Hit = 1 << 4,
    Slope = 1 << 5,
    Grounded = 1 << 6,
    Targeted = 1 << 7,
    Attacking = 1 << 8,
    Falling = 1 << 9,
    Dodging = 1 << 10,
    Hitting = 1 << 11,
    Jumping = 1 << 13,
    Death = 1 << 14,
    DodgeIgnored = 1 << 15,
}

public enum ActionType
{
    Attack,
    Skill,
    Dead,
    Faced,
    Sit,
    Hit,
    Walk,
    Move,
    Side,
    Jump,
    Fall,
    Land,
    Dodge,
    Run,
    Vertical,
    Grounded,
    Guided,
    Pray,

    //무기관련
    Effect,
    Projectile,
    Break,
}

public enum Request
{
    writegachalog,
    readgachalog,
    banners,
    weapons,
    quests,
}

public enum LoadType
{
    ItemPrefab,
    ProjectilePrefab,
    ItemIcon,
    GachaBackground,
    GachaBanner,
    Minigame,
    HitEffect,
    Icon,
}

public enum ItemType
{
    Potion,
    Weapon
}

public enum WeaponType
{
    Melee,
    Bow,
    Magic,
    Count
}

/// <summary>
/// +110으로 로케일 고정
/// </summary>
public enum ItemFilterType
{
    All = 0,
    Weapon = 1,
    Potion = 2
}

public enum WeaponFilterType
{
    All = 0,
    Melee = 1,
    Bow = 2,
    Magic = 3
}

/// <summary>
/// +100으로 로케일 설정
/// </summary>
public enum SortMainType
{
    Rarity = 0,
    Damage = 1,
    Defense = 2,
}

public enum SortDirectionType
{
    Descending = 0,
    Ascending = 1
}

public enum AttackEffect
{
    MeleeOne,
    MeleeTwo,
    MeleeThree,
    MagicOne,
    BowOne,
}

public enum AttackType
{
    Physical,
    Magical,
    Fixed,
}

public enum BulletType
{
    Straight,
    Curve,
    Area,
    RandomBezier,
}

public enum InteractType
{
    Warp,
    NPC,
    Door,
    Text,
    Dungeon
}

public enum LayerType
{
    Ground,
    Wall,
    Water,
    Player,
    Enemy,
}

public enum AttackEvent
{
    MeleeStart,
    MeleeEnd,
    Projectile,
    Casting,
}

public enum CameraType
{
    Main = 0,
    Target,
    Sit,
    Minigame,
    Talk,
    Dead
}

public enum Rare
{
    R = 1,
    SR = 2,
    SSR = 3
}

public enum BannerType
{
    None = 0,
    Beginner,
    Pickup,
    Limited,
}

public enum MinigameType
{
    SpeedTexting,
    SpeedClicking,
}

public enum ConditionType
{
    None = 0,
    TalkCount,
    Quest,
    Item,
    Time,
    Level
}

public enum Daylight
{
    Morning,
    Day,
    Evening,
    Night
}

public enum ObjectiveType
{
    Interact,
    Kill,
    Collect,
}

public enum QuestState
{
    Available = 10000037,
    Accepted = 10000038,
    Completed = 10000039,
    Achieved = 10000041
}

public enum EffectColor
{
    White,
    Purple,
    Red,
    Blue,
    Grey
}