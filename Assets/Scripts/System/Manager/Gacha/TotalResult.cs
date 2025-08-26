using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;
using System.Linq;

public class TotalResult : MonoBehaviour, IFlag
{
    public bool FlagEnd { get; set; }

    public int InitializationPriority => 3;

    [Header("결과 관리")]
    [SerializeField] private GameObject characterGroup;
    private List<HoverableFrame> resultUnits;

    private Animator totalAnimator;
    private int hashSlide;

    private void Awake()
    {
        totalAnimator = GetComponent<Animator>();
        hashSlide = Animator.StringToHash("Slide");

        resultUnits = characterGroup.GetComponentsInChildren<HoverableFrame>(true).ToList();
    }

    public void PlaySlide()
    {
        totalAnimator.Play(hashSlide, 0, 0);
        //totalAnimator.SetTrigger(hashSlide);
    }

    public void InitDatas(List<(TableItem.Info Info, RandomWeaponData Data)> _weapons, List<UnityEngine.Color> _colors)
    {
        int count = _weapons.Count;
        for (int i = 0; i < count; i++)
        {
            resultUnits[i].SetFrameDataFromRandom(_weapons[i].Info, _weapons[i].Data);
            //resultUnits[i].Image.sprite = await ResourceLoader.LoadAsync<Sprite>(_weapons[i].Info.Icon, LoadType.ItemIcon);
            //resultUnits[i].Frame.color = _colors[i];
        }
    }

    public void StartTotal()
    {
        gameObject.SetActive(true);
        PlaySlide();
    }

    public void CloseTotal()
    {
        FlagEnd = true;
    }

    public void EndTotal()
    {
        FlagEnd = true;
    }

    //애니메이션 호출
    private void SetAnimationEnd()
    {
        EndTotal();
    }
}
