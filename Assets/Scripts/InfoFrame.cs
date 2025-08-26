using ExitGames.Client.Photon.StructWrapping;
using GameStuff;
using TMPro;
using UnityEngine;

public class InfoFrame : MonoBehaviour
{
    private TMP_Text nameText;
    [SerializeField] private GameObject infoFieldPrefab;
    public float heightPaddingPreset = 10f;

    private RectTransform rectTransform;

    private void Awake()
    {
        gameObject.SetActive(false);

        nameText = GetComponentInChildren<TMP_Text>(true);
        rectTransform = GetComponent<RectTransform>();
    }

    public void Show(RectTransform _transform, int _itemID, InfoDisplayType _displayType = InfoDisplayType.TableInfoWeapon, ItemInstance _instance = null)
    {
        // 기존 InfoField들 삭제
        var existingFields = GetComponentsInChildren<InfoField>(true);
        for (int i = 0; i < existingFields.Length; i++)
        {
            DestroyImmediate(existingFields[i].gameObject);
        }

        switch(_displayType)
        {
            case InfoDisplayType.TableInfoWeapon:
            case InfoDisplayType.Skill:
                SetDataFromTable(_itemID);
                break;
            case InfoDisplayType.ActualWeapon:
                SetDataFromInstance(_instance);
                break;
            default:
                gameObject.SetActive(false);
                return;
        }

        var infoFields = GetComponentsInChildren<InfoField>(true);

        // 30은 name필드
        float height = 30f;
        for(int i = 0; i < infoFields.Length; i++)
        {
            height += infoFields[i].Height;
        }

        // 필드 간 간격 및 추가 여백 고려
        float fieldSpacing = infoFields.Length * 5f; // 필드당 5px 추가 여백
        float layoutPadding = 20f; // 레이아웃 패딩 고려

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + heightPaddingPreset + fieldSpacing + layoutPadding);
        rectTransform.position = _transform.position;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetDataFromTable(int _id)
    {
        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError($"Table is null in {nameof(InfoFrame)}");
            return;
        }

        var itemData = table.Item.Get(_id);
        var tableLocale = table.Locale;

        nameText.text = tableLocale.Get(itemData.Name, GameManager.CurrentLocale);

        switch((ItemType)itemData.ItemType)
        {
            case ItemType.Weapon:
                {
                    var weapon_selected = table.Weapon.Get(_id);

                    var damageField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    damageField.SetData(AxisType.Horizontal, 
                        tableLocale.Get(10000002, GameManager.CurrentLocale),
                        $"{weapon_selected.Damage_Min} ~ {weapon_selected.Damage_Max}");

                    var defenseField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    defenseField.SetData(AxisType.Horizontal,
                        tableLocale.Get(10000003, GameManager.CurrentLocale),
                        $"{weapon_selected.Defense_Min} ~ {weapon_selected.Defense_Max}");

                    // 여백
                    var blank = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    blank.SetData(AxisType.Horizontal, "", "", 20f);

                    // 스탯 요구치
                    var strField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    strField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000006, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
                        weapon_selected.Require_STR.ToString());

                    var dexField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    dexField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000007, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
                        weapon_selected.Require_DEX.ToString());

                    var intField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    intField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000008, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
                        weapon_selected.Require_INT.ToString());

                    var blank2 = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    blank2.SetData(AxisType.Horizontal, "", "", 20f);

                    // 스탯 성장
                    var growStrField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    growStrField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000006, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
                        weapon_selected.DamageGrowth_STR.ToString());

                    var growDexField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    growDexField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000007, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
                        weapon_selected.DamageGrowth_DEX.ToString());

                    var growIntField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    growIntField.SetData(AxisType.Horizontal,
                        $"{tableLocale.Get(10000008, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
                        weapon_selected.DamageGrowth_INT.ToString());
                }
                break;
            case ItemType.Skill:
                {
                    var skill_selected = table.Skill.Get(_id);
                    var localeTable = table.Locale;

                    // 요구 슬롯
                    var cooldownField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    cooldownField.SetData(AxisType.Horizontal,
                        tableLocale.Get(10000050, GameManager.CurrentLocale),
                        $"{skill_selected.RequiredSlotCount}s");

                    // 스킬 설명
                    var descField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
                    var formattedDesc = Singleton.Get<AbilityManager>().GetDescription(_id);
                    descField.SetData(AxisType.Vertical,
                        tableLocale.Get(10000051, GameManager.CurrentLocale),
                        formattedDesc, 120f);
                }
                break;
            default:
                Debug.LogWarning($"Unsupported ItemType {itemData.ItemType} for item ID {_id} in {nameof(InfoFrame)}");
                break;
        }
    }

    private void SetDataFromInstance(ItemInstance _instance)
    {
        if (_instance == null)
        {
            Debug.LogError($"ItemInstance is null in {nameof(InfoFrame)}");
            return;
        }

        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError($"Table is null in {nameof(InfoFrame)}");
            return;
        }

        var tableLocale = table.Locale;
        var item_selected = table.Item.Get(_instance.ItemID);
        var weapon_selected = table.Weapon.Get(_instance.ItemID);
        var weaponInstance = _instance as WeaponItemInstance;

        nameText.text = tableLocale.Get(item_selected.Name, GameManager.CurrentLocale);

        // 실제 공격력 필드
        var damageField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        damageField.SetData(AxisType.Horizontal,
            tableLocale.Get(10000002, GameManager.CurrentLocale),
            weaponInstance.Damage.ToString(), 30f);

        // 실제 방어력 필드
        var defenseField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        defenseField.SetData(AxisType.Horizontal,
            tableLocale.Get(10000003, GameManager.CurrentLocale),
            weaponInstance.Defense.ToString(), 30f);

        var blank = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        blank.SetData(AxisType.Horizontal, "", "", 20f);

        // 스탯 요구치
        var strField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        strField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000006, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
            weapon_selected.Require_STR.ToString());

        var dexField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        dexField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000007, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
            weapon_selected.Require_DEX.ToString());

        var intField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        intField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000008, GameManager.CurrentLocale)} {tableLocale.Get(11000011, GameManager.CurrentLocale)}",
            weapon_selected.Require_INT.ToString());

        var blank2 = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        blank2.SetData(AxisType.Horizontal, "", "", 20f);

        // 스탯 성장치
        var growStrField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        growStrField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000006, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
            weapon_selected.DamageGrowth_STR.ToString());

        var growDexField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        growDexField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000007, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
            weapon_selected.DamageGrowth_DEX.ToString());

        var growIntField = Instantiate(infoFieldPrefab, transform).GetComponent<InfoField>();
        growIntField.SetData(AxisType.Horizontal,
            $"{tableLocale.Get(10000008, GameManager.CurrentLocale)} {tableLocale.Get(11000010, GameManager.CurrentLocale)}",
            weapon_selected.DamageGrowth_INT.ToString());
    }
}
