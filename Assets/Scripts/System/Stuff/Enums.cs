using System;

namespace SoundStuff
{
    public enum SoundType
    {
        Player,
        Enemy,
        /// <summary>
        /// 상호작용
        /// </summary>
        Interactable,
        /// <summary>
        /// 환경
        /// </summary>
        Environment,
    }

    public enum EnemyType
    {
        Skeleton,
        Goblin,
        Dragon,
    }

    public enum PlayerActionType
    {
        Hurt,
        Dead,
        Dodge,
    }

    public enum BossAttackPattern
    {
        Dash,
        Smash,
        Bite,
        Jump,
    }

    public enum FootstepType
    {
        Grass   = 1 << 20,
        Stone   = 1 << 21,
        Water   = 1 << 22,
        Wood    = 1 << 23,
        Metal   = 1 << 24,
        All = Grass | Stone | Water | Wood | Metal // 11111 << 20
    }

    public enum Map
    {
        FrontVillage = 4,
        Village = 5,
        Dungeon = 6,
    }
}

namespace GameStuff
{
    public enum Map
    {
        None = -1,
        FrontVillage = 4,
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

    public enum VolumeType
    {
        Master,
        Music,
        Effect,
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
        Drinking = 1 << 14,
        Death = 1 << 15,
        DodgeIgnored = 1 << 16,
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
        Drink,

        //상호작용
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
        AudioClip,
        Path,
    }

    public enum ItemType
    {
        Weapon,
        Skill,
        Scroll,
        Potion,
        Material,
        Total
    }

    public enum WeaponType
    {
        Melee,
        Bow,
        Magic,
        Count
    }

    public enum WeaponFilterType
    {
        All = 0,
        Melee = 1,
        Bow = 2,
        Magic = 3
    }

    /// <summary>
    /// +100 로케일
    /// </summary>
    public enum SortType
    {
        Rarity = 0,
        Damage = 1,
        Defense = 2,
    }

    public enum SortScrollType
    {
        Rarity = 0,
        DamageMin = 1,
        DamageMax = 2,
        DefenseMin = 3,
        DefenseMax = 4,
    }

    public enum SortDirectionType
    {
        Descending = 0,
        Ascending = 1
    }

    public enum CreationScrollType
    {
        Weapon,
        Skill,
        None
    }

    public enum InfoDisplayType
    {
        ActualWeapon,
        TableInfoWeapon,
        Skill,
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

    public enum SkillType
    {
        Projectile  = 0,
        Continuous  = 1,
        Knockback   = 2,
        ExtraHit    = 3,
    }

    public enum PowerType
    {
        Percentage,
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
        Item,
        Door,
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

    /// <summary>
    /// 레어도 겸 슬롯갯수
    /// </summary>
    public enum Rarity
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

    public enum QuestConditionType
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
}