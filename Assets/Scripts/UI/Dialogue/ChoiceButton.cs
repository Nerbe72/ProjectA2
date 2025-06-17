using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    [SerializeField] private float backgroundLerpSpeed = 1.5f;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    private void OnEnable()
    {
        background.fillAmount = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(buttonBackgroundLerp(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(buttonBackgroundLerp(false));
    }

    private IEnumerator buttonBackgroundLerp(bool _isEnter)
    {
        float time = 0f;
        float start = background.fillAmount;
        float end = _isEnter ? 1f : 0f;

        while (true)
        {
            time += Time.deltaTime * backgroundLerpSpeed;

            background.fillAmount = Mathf.Lerp(start, end, time);

            if (time >= backgroundLerpSpeed) break;
            yield return null;
        }

        background.fillAmount = end; // Ensure it ends exactly at the target value
        yield break;
    }
}
