using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestColumn : MonoBehaviour
{
    [SerializeField] private Image titleBackground;
    [SerializeField] private float backgroundLerpSpeed = 1.5f;
    public TMP_Text Title;
    public TMP_Text Detail;

    private void OnEnable()
    {
        StartCoroutine(backgroundLerp());
    }

    private IEnumerator backgroundLerp()
    {
        titleBackground.fillAmount = 0f;
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * backgroundLerpSpeed;
            titleBackground.fillAmount = Mathf.Lerp(0f, 1f, time);
            if (titleBackground.fillAmount >= 1f)
            {
                break;
            }
            yield return null;
        }

        titleBackground.fillAmount = 1f;
        yield break;
    }
}
