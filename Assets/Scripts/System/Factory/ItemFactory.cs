using UnityEngine;
using GameStuff;
using System.Threading.Tasks;

public class ItemFactory : MonoBehaviour
{
    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public ItemInstance CreateItem(int _itemId)
    {
        return CreateItemInternal(_itemId, false, null, null);
    }

    public ItemInstance CreateItem(int _itemId, bool _randomStatus, int _fixedDamage = 0, int _fixedDefense = 0)
    {
        return CreateItemInternal(_itemId, _randomStatus, _fixedDamage, _fixedDefense);
    }

    // 고정 스탯 아이템
    public ItemInstance CreateItem(int _itemId, int _fixedDamage, int _fixedDefense)
    {
        return CreateItemInternal(_itemId, false, _fixedDamage, _fixedDefense);
    }

    // 랜덤 스탯 아이템
    public ItemInstance CreateItemWithRandomStats(int _itemId)
    {
        return CreateItemInternal(_itemId, true, null, null);
    }

    private ItemInstance CreateItemInternal(int _itemId, bool _useRandomStats, int? _fixedDamage, int? _fixedDefense)
    {
        var table = Singleton.Get<TableDataManager>().Table;
        var itemData = table.Item.Get(_itemId);
        
        if (itemData == null)
        {
            Debug.LogError($"ItemFactory: 아이템 데이터를 찾을 수 없음. ItemID: {_itemId}");
            return null;
        }

        Debug.Log($"아이템 생성됨 ID: {_itemId}");

        switch ((ItemType)itemData.ItemType)
        {
            case ItemType.Weapon:
                return _useRandomStats ? CreateWeaponWithStats(itemData) : CreateWeapon(_itemId, _fixedDamage, _fixedDefense);
            case ItemType.Potion:
                return CreatePotion(_itemId);
            case ItemType.Material:
                return CreateMaterial(_itemId);
            case ItemType.Skill:
                return CreateSkill(_itemId);
            case ItemType.Scroll:
                return CreateScroll(_itemId);
            default:
                Debug.LogError($"지원하지 않는 아이템 타입 ItemID: {_itemId}, ItemType: {itemData.ItemType}");
                return null;
        }
    }

    private WeaponItemInstance CreateWeapon(int _itemId, int? _fixedDamage = null, int? _fixedDefense = null)
    {
        var weaponInstance = new WeaponItemInstance 
        { 
            ItemID = _itemId,
            InventoryID = System.Guid.NewGuid()
        };
        
        if (_fixedDamage.HasValue)
            weaponInstance.Damage = _fixedDamage.Value;
            
        if (_fixedDefense.HasValue)
            weaponInstance.Defense = _fixedDefense.Value;
            
        return weaponInstance;
    }

    private WeaponItemInstance CreateWeaponWithStats(TableItem.Info _itemData)
    {
        var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(_itemData.ID);
        if (weaponData == null)
        {
            Debug.LogError($"ItemFactory: 무기 데이터를 찾을 수 없음. ItemID: {_itemData.ID}");
            return null;
        }

        int damage = Random.Range(weaponData.Damage_Min, weaponData.Damage_Max + 1);
        int defense = Random.Range(weaponData.Defense_Min, weaponData.Defense_Max + 1);

        return new WeaponItemInstance
        {
            ItemID = weaponData.ID,
            InventoryID = System.Guid.NewGuid(),
            Damage = damage,
            Defense = defense
        };
    }

    private PotionItemInstance CreatePotion(int _itemId)
    {
        return new PotionItemInstance
        {
            ItemID = _itemId,
            InventoryID = System.Guid.NewGuid(),
            CurrentStack = 1
        };
    }

    private MaterialItemInstance CreateMaterial(int _itemId)
    {
        return new MaterialItemInstance 
        { 
            ItemID = _itemId,
            InventoryID = System.Guid.NewGuid(),
            maxStack = 99
        };
    }

    private SkillItemInstance CreateSkill(int _itemId)
    {
        return new SkillItemInstance 
        { 
            ItemID = _itemId,
            InventoryID = System.Guid.NewGuid(),
            CurrentStack = 1
        };
    }

    private ScrollItemInstance CreateScroll(int _itemId)
    {
        return new ScrollItemInstance 
        { 
            ItemID = _itemId,
            InventoryID = System.Guid.NewGuid(),
            maxStack = 99
        };
    }

    public ItemInstance CreateItemWithStats(int _itemId)
    {
        return CreateItemWithRandomStats(_itemId);
    }

    public async Task<bool> CreateAndAddToInventoryAsync(int _itemId)
    {
        var item = CreateItem(_itemId);
        return await AddItemToInventoryWithResourcesAsync(item);
    }

    public async Task<bool> CreateAndAddToInventoryAsync(int _itemId, int _fixedDamage, int _fixedDefense)
    {
        var item = CreateItem(_itemId, _fixedDamage, _fixedDefense);
        return await AddItemToInventoryWithResourcesAsync(item);
    }

    public async Task<bool> CreateAndAddToInventoryWithRandomStatsAsync(int _itemId)
    {
        var item = CreateItemWithRandomStats(_itemId);
        return await AddItemToInventoryWithResourcesAsync(item);
    }

    private async Task<bool> AddItemToInventoryWithResourcesAsync(ItemInstance _item)
    {
        if (_item == null)
        {
            Debug.LogError("ItemFactory: 아이템이 null입니다.");
            return false;
        }

        var inventory = Singleton.Inventory;
        if (inventory == null)
        {
            Debug.LogError("ItemFactory: Inventory 인스턴스를 찾을 수 없습니다.");
            return false;
        }

        // 프리팹 로드 및 탄환 프리로드
        if (_item is WeaponItemInstance weaponInstance)
        {
            // 무기 프리팹 로드
            var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(_item.ItemID);
            if (itemData != null)
            {
                weaponInstance.InstancedPrefab = await ResourceLoader.LoadAsync<GameObject>(itemData.Prefab, LoadType.ItemPrefab);
            }

            // 무기 어빌리티/탄환 프리로드
            //await PreloadWeaponAbilitiesAsync(_item.ItemID);
        }
        else if (_item is SkillItemInstance)
        {
            await PreloadSkillAbilitiesAsync(_item.ItemID);
        }

        inventory.TakeItem(_item);
        return true;
    }

    // 이제 어빌리티가 따로 관리되기 때문에 무기의 어빌리티 프리로드는 필요하지 않음
    //private async Task PreloadWeaponAbilitiesAsync(int _weaponId)
    //{
    //    var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(_weaponId);
    //    if (weaponData == null || weaponData.Abilities == null || weaponData.Abilities.Length <= 0)
    //        return;

    //    for (int i = 0; i < weaponData.Abilities.Length; i++)
    //    {
    //        var abilityData = Singleton.Get<TableDataManager>().Table.Skill.Get(weaponData.Abilities[i]);
    //        if (abilityData == null || abilityData.ProjectileID == 0) continue;

    //        var projectileData = Singleton.Get<TableDataManager>().Table.Projectile.Get(abilityData.ProjectileID);
    //        if (projectileData != null)
    //        {
    //            await ResourceLoader.LoadAsync<GameObject>(projectileData.Prefab, LoadType.ProjectilePrefab);
    //        }
    //    }
    //}

    private async Task PreloadSkillAbilitiesAsync(int _skillId)
    {
        // 스킬의 탄환 프리로드는 스킬 시스템이 구현되면 추가
        // 현재는 스킬 테이블 구조가 확실하지 않으므로 빈 구현으로 유지
        await System.Threading.Tasks.Task.CompletedTask;
        
        // TODO: 스킬 테이블 구조 확인 후 구현
        // 예상 구현:
        // 1. 스킬 데이터에서 ProjectileID 또는 관련 탄환 정보 확인
        // 2. 해당 탄환 프리팹 ResourceLoader.LoadAsync로 로드
        
        Debug.Log($"ItemFactory: 스킬 탄환 프리로드 대기 중. SkillID: {_skillId}");
    }
}
