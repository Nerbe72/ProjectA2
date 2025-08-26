using System.Collections;
using TMPro;
using UnityEngine;

public class AlertTemplate : MonoBehaviour
{
    [SerializeField] private TMP_Text message;
    [SerializeField] private float fixedHeight = -50;
    [SerializeField] private float timeMultiply;

    public void Show(int count, string _message, Color _color)
    {
        message.text = _message;
        StartCoroutine(FadeOutLerp(_color));
    }

    private IEnumerator FadeOutLerp(Color _textColor)
    {
        float time = 0f;

        Color from = _textColor;
        Color to = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

        message.color = from;
        while (true)
        {
            time += Time.deltaTime * timeMultiply;

            message.color = Color.Lerp(from, to, time);

            if (time >= 1f)
                break;

            yield return null;
        }

        message.color = to;
        yield break;
    }
}
