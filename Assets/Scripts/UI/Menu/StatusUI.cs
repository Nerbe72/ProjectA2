using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private Button levelButton;

    [SerializeField] private TMP_Text currencyText;

    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthSlider;

    Coroutine currencyCo = null;
    Coroutine healthCo = null;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        levelButton.onClick.AddListener(ClickLevel);

        gameObject.SetActive(false);
    }

    private async void Start()
    {
        while(!Singleton.Player.IsInstantiated)
        {
            await Task.Yield();
        }

        currencyText.text = Singleton.Inventory.GetCurrency().ToString();
        healthText.text = $"{Singleton.Player.GetCurrentHealth()}/{Singleton.Player.CurrentMaxHp}";
        healthSlider.value = (float)Singleton.Player.GetCurrentHealth() / Singleton.Player.CurrentMaxHp;

        Singleton.Inventory.OnCurrencyChanged += ChangeCurrency;
        Singleton.Player.OnHealthChanged += UpdateHealth;
        Singleton.Player.OnLevelChanged += UpdateLevel;

        UpdateLevel();
    }

    private void OnDestroy()
    {
        Singleton.Inventory.OnCurrencyChanged -= ChangeCurrency;
        Singleton.Player.OnHealthChanged -= UpdateHealth;
    }

    private void ClickLevel()
    {
        if (!Singleton.Get<PlayerStatusUI>().gameObject.activeSelf)
        {
            (Singleton.Get<PlayerStatusUI>() as IWindowStack)?.ShowWindow();
        }
    }

    private void ChangeCurrency(uint _latest)
    {
        if (!gameObject.activeSelf)
        {
            currencyText.text = _latest.ToString();
            return;
        }

        if (currencyCo != null)
            StopCoroutine(currencyCo);
        currencyCo = StartCoroutine(LerpCurrency((int)_latest));
    }

    private void UpdateHealth(int _current, int _max)
    {
        if (!gameObject.activeSelf)
        {
            healthSlider.value = (float)_current / _max;
            healthText.text = $"{_current}/{_max}";
            return;
        }

        if (healthCo != null)
            StopCoroutine(healthCo);
        healthCo = StartCoroutine(LerpHealthCoroutine(_current, _max));
    }

    private void UpdateLevel()
    {
        levelButton.GetComponentInChildren<TMP_Text>().text = Singleton.Player.CurrentLevels.GetTotal().ToString();
    }

    private IEnumerator LerpCurrency(int _latest)
    {
        float time = 0;

        int currency_before = int.Parse(currencyText.text);
        int currency_after = _latest;

        while (true)
        {
            time += Time.deltaTime * 2.5f;

            int lerpedCurrency = Mathf.RoundToInt(Mathf.Lerp(currency_before, currency_after, time));
            currencyText.text = lerpedCurrency.ToString();

            if (time >= 1f)
            {
                currencyText.text = currency_after.ToString();
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        currencyCo = null;
        yield break;
    }

    private IEnumerator LerpHealthCoroutine(int targetCurrent, int targetMax)
    {
        float duration = 0.5f;
        float time = 0f;

        float startValue = healthSlider.value;
        float endValue = (float)targetCurrent / targetMax;

        int startHp = Mathf.RoundToInt(startValue * targetMax);
        int endHp = targetCurrent;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float lerpedValue = Mathf.Lerp(startValue, endValue, t);
            int lerpedHp = Mathf.RoundToInt(Mathf.Lerp(startHp, endHp, t));

            healthSlider.value = lerpedValue;
            healthText.text = $"{lerpedHp}/{targetMax}";

            yield return null;
        }

        healthSlider.value = endValue;
        healthText.text = $"{targetCurrent}/{targetMax}";
        healthCo = null;
    }
}
