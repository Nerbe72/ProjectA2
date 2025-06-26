using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractIndicatorUI : MonoBehaviour
{
    public int InitializationPriority => 10;

    private Image indicatorFrame;
    private TMP_Text indicatorText;

    [SerializeField] private float ShownDuration;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        indicatorFrame = GetComponentsInChildren<Image>()[0];
        indicatorText = GetComponentInChildren<TMP_Text>();
    }

    public void SetShowIndicator(bool _isShown, int _id = 0)
    {
        if (_id != 0)
        {
            var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
            indicatorText.text = "F: " + localeTable.Get(_id, GameManager.CurrentLocale);
        }

        if (!gameObject.activeSelf || gameObject.IsDestroyed()) return;
        StopAllCoroutines();
        StartCoroutine(ShowCoroutine(_isShown));
    }

    private IEnumerator ShowCoroutine(bool _isShown)
    {
        float time = 0;
        float speed = 1 / ShownDuration;

        Color start = _isShown ? Color.clear : Color.black;
        Color end = _isShown ? Color.black : Color.clear;

        while (true)
        {
            time += Time.deltaTime * speed;

            indicatorFrame.color = Color.Lerp(start, end, time);

            if (time >= 1f) break;
            yield return null;
        }

        yield break;
    }
}
