using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleResult : MonoBehaviour, IFlag
{
    public bool FlagEnd { get; set; }

    public int InitializationPriority => 3;

    private List<(TableItem.Info Info, RandomWeaponData Data)> items;
    private List<UnityEngine.Color> colors;

    [SerializeField] private Image weaponImage;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text weaponRarity;
    [SerializeField] private TMP_Text weaponDamage;
    [SerializeField] private TMP_Text weaponDefense;
    [SerializeField] private Button nextButton;

    private Animator animator;
    private int hashSingle;

    private int weaponIndex;

    private UnityEngine.Color targetColor;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        hashSingle = Animator.StringToHash("Single");
        weaponIndex = 0;

        nextButton.onClick.AddListener(MoveNext);
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveAllListeners();
    }

    private void MoveNext()
    {
        //animator.Rebind();

        if (weaponIndex >= items.Count)
        {
            CloseSingle();
            return;
        }

        weaponImage.sprite = ResourceLoader.Load<Sprite>(items[weaponIndex].Info.Icon, LoadType.ItemIcon);
        weaponName.text = Singleton.Get<TableDataManager>().Table.Locale.Get(items[weaponIndex].Info.Name, GameManager.CurrentLocale);
        weaponRarity.text = ((Rare)items[weaponIndex].Info.Rarity).ToString();
        weaponDamage.text = items[weaponIndex].Data.Damage.ToString();
        weaponDefense.text = items[weaponIndex].Data.Defense.ToString();

        weaponRarity.color = Color.clear;
        targetColor = colors[weaponIndex];
        animator.Play(hashSingle, 0, 0);
        //animator.SetTrigger(hashSingle);

        weaponIndex += 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_items">Result ID List</param>
    public void InitData(List<(TableItem.Info Info, RandomWeaponData Data)> _items, List<UnityEngine.Color> _colors)
    {
        weaponIndex = 0;
        items = _items;
        colors = _colors;
    }

    public void StartSingle()
    {
        gameObject.SetActive(true);
        MoveNext();
    }

    public void CloseSingle()
    {
        FlagEnd = true;
        gameObject.SetActive(false);
    }

    private void ChangeRareColor()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeRareColorCo());
    }

    private IEnumerator ChangeRareColorCo()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            weaponRarity.color = UnityEngine.Color.Lerp(UnityEngine.Color.clear, targetColor, time);

            if (time >= 0.6f)
            {
                break;
            }

            yield return null;
        }

        weaponRarity.color = targetColor;

        yield break;
    }

}
