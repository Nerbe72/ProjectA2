using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeadlHealthIndicator : MonoBehaviour
{
    private Camera main;
    private IHurtable owner;

    private Slider slider;

    private void Start()
    {
        main = Singleton.Get<CameraManager>().main;
        owner = GetComponentInParent<IHurtable>();
        slider = GetComponentInChildren<Slider>();

        owner.OnHealthChanged += UpdateHealth;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.LookAt(main.transform);
    }

    private void UpdateHealth(int current, int max)
    {
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HealthLerp(current, max));
    }

    private IEnumerator HealthLerp(int current, int max)
    {
        float time = 0f;
        float currentSlider = slider.value;

        while (true)
        {
            time += Time.deltaTime * 1.5f;

            slider.value = Mathf.Lerp(currentSlider, (float)current / max, time);

            if (time >= 1f) break;

            yield return null;
        }

        yield break;
    }
}
