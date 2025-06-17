using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameTimer : MonoBehaviour
{
    public int InitializationPriority => 6;
    public event Action OnTimeOut;

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private Image sliderFill;

    public void StartTimer(float _limitTime)
    {
        StopAllCoroutines();
        StartCoroutine(SliderLerp(_limitTime));
        StartCoroutine(TimerLerp(_limitTime));
    }

    public void StopTimer()
    {
        if (!gameObject.activeSelf) return;
        StopAllCoroutines();
    }

    private IEnumerator TimerLerp(float _limitTime)
    {
        float time = _limitTime;

        timeText.text = _limitTime.ToString("0");
        while (true)
        {
            yield return new WaitForSeconds(1f);
            time -= 1f;

            timeText.text = time.ToString("0");

            if (time <= 0) break;

            yield return null;
        }

        //타임오버
        OnTimeOut?.Invoke();
        yield break;
    }

    private IEnumerator SliderLerp(float _limitTime)
    {
        float time = _limitTime;

        timeSlider.value = 1f;
        while (true)
        {
            time -= Time.deltaTime;
            timeSlider.value = time / _limitTime;
            sliderFill.color = Color.Lerp(Color.red, Color.green, timeSlider.value);

            if (time <= 0f) break;

            yield return null;
        }

        timeSlider.value = 0f;
        sliderFill.color = Color.red;
        yield break;
    }
}
