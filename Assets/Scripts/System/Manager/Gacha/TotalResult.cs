using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalResult : MonoBehaviour, IFlag
{
    public bool FlagEnd { get; set; }

    public int InitializationPriority => 3;

    [Header("결과 관리")]
    [SerializeField] private GameObject characterGroup;
    [SerializeField] private List<Image> characterImages; //캐릭터 이미지 변경
    [SerializeField] private List<Image> characterFrames; //등급에 맞춰 색상변경

    private Animator totalAnimator;
    private int hashSlide;

    private void Awake()
    {
        totalAnimator = GetComponent<Animator>();
        hashSlide = Animator.StringToHash("Slide");

        InitData();
    }

    private void InitData()
    {
        characterGroup = GetComponentInChildren<HorizontalLayoutGroup>(true).gameObject;
    }

    public void PlaySlide()
    {
        totalAnimator.Play(hashSlide, 0, 0);
        //totalAnimator.SetTrigger(hashSlide);
    }

    public async void InitDatas(List<(TableItem.Info Info, RandomWeaponData Data)> _weapons, List<UnityEngine.Color> _colors)
    {
        InitData();

        int count = _weapons.Count;
        for (int i = 0; i < count; i++)
        {
            characterImages[i].sprite = await ResourceLoader.LoadAsync<Sprite>(_weapons[i].Info.Icon, LoadType.ItemIcon);
            characterFrames[i].color = _colors[i];
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
