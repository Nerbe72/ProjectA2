using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private float blinkDelay = 0.5f;

    private Image slotImage;

    private Coroutine notenoughCo;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void SetActivated()
    {
        slotImage.color = Color.white;
    }

    public void SetDeactivated()
    {
        slotImage.color = Color.gray;
    }

    public void NotEmough()
    {
        if (notenoughCo != null)
        {
            StopCoroutine(notenoughCo);
        }
        
        notenoughCo = StartCoroutine(Blinking(blinkCount));
    }

    private IEnumerator Blinking(int _count)
    {
        int currentCount = 0;

        while (currentCount < _count)
        {
            slotImage.color = Color.red;
            yield return new WaitForSeconds(blinkDelay);
            slotImage.color = Color.white;
            yield return new WaitForSeconds(blinkDelay);
            currentCount++;
        }

        notenoughCo = null;
        yield break;
    }
}
