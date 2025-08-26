using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class GachaResult : MonoBehaviour
{
    public int InitializationPriority => 4;

    [Header("EffectUI")]
    public TotalResult gachaTotal { get; private set; }
    public SingleResult gachaSingle { get; private set; }
    public SplashResult gachaSplash { get; private set; }

    [Header("Button")]
    [SerializeField] private Button skip;
    [SerializeField] private Button close;

    public event Action OnResultEnd;

    private void Awake()
    {
        gachaTotal = GetComponentInChildren<TotalResult>(true);
        gachaSingle = GetComponentInChildren<SingleResult>(true);
        gachaSplash = GetComponentInChildren<SplashResult>(true);

        skip.onClick.AddListener(PressSkip);
        close.onClick.AddListener(PressClose);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gachaSplash.gameObject.activeSelf || gachaSingle.gameObject.activeSelf)
            {
                ShowSkip();
            }
        }
    }

    private void OnDestroy()
    {
        skip.onClick.RemoveAllListeners();
        close.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 플래그를 통해 비동기로 전이됨
    /// </summary>
    public async void ShowResult(GachaResultData _data)
    {
        skip.gameObject.SetActive(false);
        close.gameObject.SetActive(false);

        int bestRarity = 0;

        //splash
        int count = _data.Items.Count;
        if (count == 10)
        {
            for (int i = 0; i < count; i++)
            {
                var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_data.Items[i].ID);
                Color singleColor = ItemColor.GetColor((Rarity)item_selected.Rarity);

                if (bestRarity < item_selected.Rarity)
                    bestRarity = item_selected.Rarity;

                if ((Rarity)bestRarity == Rarity.SSR) break;
            }
        }

        Color targetColor = ItemColor.GetColor((Rarity)bestRarity);

        gachaSplash.SetColor(targetColor);
        gachaSplash.StartSplash();

        while (!gachaSplash.FlagEnd)
        {
            await Task.Yield();
        }

        gachaSplash.FlagEnd = false;

        //single
        List<(TableItem.Info Info, RandomWeaponData Data)> items = new List<(TableItem.Info Info, RandomWeaponData Data)>();
        List<Color> colors = new List<Color>();

        for (int i = 0; i < count; i++)
        {
            var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_data.Items[i].ID);
            items.Add((item_selected, _data.Items[i].Data));
            colors.Add(ItemColor.GetColor((Rarity)item_selected.Rarity));
        }

        gachaSingle.InitData(items, colors);
        gachaSingle.StartSingle();

        while (!gachaSingle.FlagEnd)
        {
            await Task.Yield();
        }

        gachaSingle.FlagEnd = false;

        if (count == 1)
        {
            return;
        }

        //total
        gachaTotal.InitDatas(items, colors);
        gachaTotal.StartTotal();

        while (!gachaTotal.FlagEnd)
        {
            await Task.Yield();
        }

        gachaTotal.FlagEnd = false;

        ShowClose();
    }

    private void PressSkip()
    {
        if (!gachaSplash.FlagEnd && gachaSplash.gameObject.activeSelf)
        {
            gachaSplash.CloseSplash();
            return;
        }
        else if (!gachaSingle.FlagEnd && gachaSingle.gameObject.activeSelf)
        {
            gachaSingle.CloseSingle();
            return;
        }
        else if (!gachaTotal.FlagEnd && gachaTotal.gameObject.activeSelf)
        {
            gachaTotal.CloseTotal();
            return;
        }
    }

    private void PressClose()
    {
        OnResultEnd?.Invoke();
    }

    public void ShowSkip()
    {
        skip.gameObject.SetActive(true);
    }

    public void ShowClose()
    {
        close.gameObject.SetActive(true);
    }
}
