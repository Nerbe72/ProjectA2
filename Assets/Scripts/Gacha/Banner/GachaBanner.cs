using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class GachaBanner : MonoBehaviour
{
    public int InitializationPriority => 5;

    [Header("Banners / Banner Detail")]
    [SerializeField] private ScrollRect bannerScroll;
    [SerializeField] private GameObject contentPrefab;

    [SerializeField] private Image backgroundIMG;
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private RectTransform weaponImagePosition;
    [SerializeField] private List<Image> weaponImages;

    [Header("Roll Buttons")]
    [SerializeField] private TMP_Text currency;
    [SerializeField] private Button one_roll;
    [SerializeField] private Button ten_roll;
    [SerializeField] private TMP_Text one_roll_price;
    [SerializeField] private TMP_Text ten_roll_price;

    [SerializeField] private Button back;

    private Animator bannerAnimator;
    private readonly int selectedHash = Animator.StringToHash("Selected");

    private GachaManager gachaManager;

    public event Action<BannerData, int> OnSelectRoll;

    private void Awake()
    {
        bannerAnimator = GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        if (bannerScroll != null && bannerScroll.content != null && bannerScroll.content.childCount > 0)
        {
            GameObject firstBanner = bannerScroll.content.GetChild(0).gameObject;
            BannerContainer container = firstBanner.GetComponent<BannerContainer>();
            if (container != null)
            {
                SelectContent(container);
            }
        }
    }

    private void OnDestroy()
    {
        one_roll.onClick.RemoveAllListeners();
        ten_roll.onClick.RemoveAllListeners();
    }

    public async void Init(uint _currency)
    {
        ResetUI();

        gachaManager = Singleton.Get<GachaManager>();

        await gachaManager.InitBannerDatas();
        SetUI();
        await gachaManager.InitCount();

        SetCurrency(_currency);
        bannerText.gameObject.SetActive(false);
        one_roll.gameObject.SetActive(false);
        ten_roll.gameObject.SetActive(false);

        GameObject button_first = bannerScroll.content.GetChild(0).gameObject;
        SelectContent(button_first.GetComponent<BannerContainer>());
    }

    private void ResetUI()
    {
        int count = bannerScroll.content.childCount;
        for (int i = 0; i < count; i++)
        {
            bannerScroll.content.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void SetUI()
    {
        int scrollCount = bannerScroll.content.childCount;
        int bannerCount = gachaManager.BannerDatas.Count;

        for (int i = 0; i < scrollCount; i++)
        {
            AddContent(gachaManager.BannerDatas[i], i);
        }

        for (int i = scrollCount; i < bannerCount; i++)
        {
            AddContent(gachaManager.BannerDatas[i]);
        }
    }

    private void SetCurrency(uint _currentCurrency)
    {
        currency.text = _currentCurrency.ToString();
    }

    private async void AddContent(BannerData _data, int _index = -1)
    {
        GameObject currentContent;
        if (_index == -1)
            currentContent = Instantiate(contentPrefab, bannerScroll.content);
        else
            currentContent = bannerScroll.content.GetChild(_index).gameObject;

        currentContent.SetActive(true);

        var container = currentContent.GetComponent<BannerContainer>();
        container.SetBannerData(_data);
        currentContent.GetComponent<Button>().onClick.AddListener(() => { SelectContent(container); });
    }

    private async void SelectContent(BannerContainer _container)
    {
        if (_container == null) return;
        
        if (bannerAnimator != null)
        {
            bannerAnimator.SetTrigger(selectedHash);
            
            StopAllCoroutines();
            StartCoroutine(FadeWeaponImageLerp());
        }

        one_roll.gameObject.SetActive(true);
        ten_roll.gameObject.SetActive(true);
        bannerText.gameObject.SetActive(true);

        one_roll.onClick.RemoveAllListeners();
        ten_roll.onClick.RemoveAllListeners();

        backgroundIMG.sprite = await ResourceLoader.LoadAsync<Sprite>(_container.Data.BackgroundPath, LoadType.GachaBackground);
        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        bannerText.text = localeTable.Get(_container.Data.BannerName, GameManager.CurrentLocale);
        weaponImagePosition.position.Set(_container.Data.CharacterPosition.x, _container.Data.CharacterPosition.y, 0);

        int count = Math.Clamp(_container.Data.SSR_PickupList.Count, 0, 3);
        for (int i = 0; i < count; i++)
        {
            var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_container.Data.SSR_PickupList[i]);

            if (item_selected == null) continue;

            weaponImages[i].sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
            weaponImages[i].color = UnityEngine.Color.white;
        }

        for (int i = count; i < 3; i++)
        {
            weaponImages[i].sprite = null;
            weaponImages[i].color = UnityEngine.Color.clear;
        }
        
        one_roll.GetComponentsInChildren<TMP_Text>()[1].text = _container.Data.SinglePrice.ToString();
        ten_roll.GetComponentsInChildren<TMP_Text>()[1].text = _container.Data.TenPrice.ToString();

        one_roll.onClick.RemoveAllListeners();
        one_roll.onClick.AddListener(() =>
        {
            OnSelectRoll?.Invoke(_container.Data, 1);
        });

        ten_roll.onClick.RemoveAllListeners();
        ten_roll.onClick.AddListener(() =>
        {
            OnSelectRoll?.Invoke(_container.Data, 10);
        });
    }
    
    private IEnumerator FadeWeaponImageLerp()
    {
        yield return null;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            
            for (int i = 0; i < weaponImages.Count; i++)
            {
                if (weaponImages[i] != null && weaponImages[i].sprite != null)
                {
                    Color currentColor = weaponImages[i].color;
                    currentColor.a = Mathf.Lerp(0f, 1f, normalizedTime);
                    weaponImages[i].color = currentColor;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        for (int i = 0; i < weaponImages.Count; i++)
        {
            if (weaponImages[i] != null && weaponImages[i].sprite != null)
            {
                Color finalColor = weaponImages[i].color;
                finalColor.a = 1f;
                weaponImages[i].color = finalColor;
            }
        }
    }
}
