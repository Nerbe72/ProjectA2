using UnityEngine;
using System.Collections.Generic;

using GameStuff;
using System.Linq;


public abstract class Weapon : MonoBehaviour, IWeapon
{
    [HideInInspector] public int WeaponID;
    protected IAttackStrategy attackStrategy;
    protected Character owner;

    protected List<Material> materials = new List<Material>();
    //private MaterialPropertyBlock propertyBlock;

    private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");

    protected virtual void Awake()
    {
        var meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        materials = meshRenderer.sharedMaterials.ToList();
    }

    public void SetOwner(Character _owner)
    {
        owner = _owner;
    }

    public virtual void UseWeapon()
    {
        if (attackStrategy != null)
            attackStrategy.ExecuteAttack(this);
    }

    public virtual void PlayAttackSound()
    {
        var weaponData = Singleton.Get<TableDataManager>().Table?.Weapon.Get(WeaponID);

        if (weaponData != null)
        {
            var audioClip = ResourceLoader.Load<AudioClip>(weaponData.AttackSoundName, LoadType.AudioClip);
            Singleton.Get<SoundManager>()?.PlayEffectOneShot(audioClip);
        }
    }

    public void SetAttackStrategy(IAttackStrategy _strategy)
    {
        attackStrategy = _strategy;
    }

    public virtual void HandlerAnimation(AttackEvent _event) { }

    public void SetOutlineColor(int _enhancementCount)
    {
        var table = Singleton.Get<TableDataManager>()?.Table;

        if (table == null) return;

        var item_selected = table.Item.Get(WeaponID);
        var weapon_selected = table.Weapon.Get(WeaponID);

        if (item_selected == null || weapon_selected == null) return;

        Rarity rarity = (Rarity)item_selected.Rarity;
        int maxEnhancement = weapon_selected.MaxEnchantmentCount;

        Color maxColor = ItemColor.GetColor(rarity);

        for(int i = 0; i < materials.Count; i++)
        {
            Color targetColor = Color.Lerp(Color.black, maxColor, _enhancementCount/maxEnhancement);
            materials[i].SetColor(outlineColorID, targetColor);
        }
    }
}
