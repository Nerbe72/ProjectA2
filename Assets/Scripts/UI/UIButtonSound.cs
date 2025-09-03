using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Click")]
    [SerializeField] private AudioClip clickClip = null;
    [Header("Hover Settings")]
    [SerializeField] private bool playHover = true;
    [SerializeField] private AudioClip hoverClip = null;
    [SerializeField] private bool playUnhover = true;
    [SerializeField] private AudioClip unhoverClip = null;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button?.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            button?.onClick.AddListener(OnClick);
        }
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager != null && clickClip != null)
        {
            soundManager.PlayUIClip(clickClip);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (playHover && soundManager != null && hoverClip != null)
        {
            soundManager.PlayUIClip(hoverClip);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (playUnhover && soundManager != null && unhoverClip != null)
        {
            soundManager.PlayUIClip(unhoverClip);
        }
    }
}
