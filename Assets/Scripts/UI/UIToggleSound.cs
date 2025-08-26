using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIToggleSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool playOnlyIsOn = false;
    [Header("Toggle Override")]
    [SerializeField] private bool useOnClip = false;
    [SerializeField] private AudioClip onClip = null;
    [SerializeField] private bool useOffClip = false;
    [SerializeField] private AudioClip offClip = null;
    [Header("Hover Override")]
    [SerializeField] private bool useHoverClip = false;
    [SerializeField] private AudioClip hoverClip = null;
    [Header("Unhover Override")]
    [SerializeField] private bool useUnhoverClip = false;
    [SerializeField] private AudioClip unhoverClip = null;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager == null) return;

        if (isOn)
        {
            if (useOnClip && onClip != null)
                soundManager.PlayUIClip(onClip);
        }
        else if (!playOnlyIsOn)
        {
            if (useOffClip && offClip != null)
                soundManager.PlayUIClip(offClip);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var soundManager = Singleton.Get<SoundManager>();

        if (useHoverClip && hoverClip != null)
            soundManager.PlayUIClip(hoverClip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var soundManager = Singleton.Get<SoundManager>();

        if (useUnhoverClip && unhoverClip != null)
            soundManager.PlayUIClip(unhoverClip);
    }
}
