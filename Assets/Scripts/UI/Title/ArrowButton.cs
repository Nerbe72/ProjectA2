using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int destinationIndex;

    Button destinationButton;
    TMP_Text destinationText;

    public event Action<int> Clicked;

    private void Awake()
    {
        destinationButton = GetComponent<Button>();
        destinationText = GetComponentInChildren<TMP_Text>(true);

        destinationText.gameObject.SetActive(false);

        destinationButton.onClick.AddListener(() => { Clicked?.Invoke(destinationIndex); });
    }

    private void OnDestroy()
    {
        destinationButton.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        destinationText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        destinationText.gameObject.SetActive(false);
    }

    public void Click()
    {
        Clicked?.Invoke(destinationIndex);
    }
}
