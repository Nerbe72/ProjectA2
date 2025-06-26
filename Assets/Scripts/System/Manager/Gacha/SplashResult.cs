using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashResult : MonoBehaviour, IFlag
{
    public bool FlagEnd { get; set; }

    public int InitializationPriority => 3;

    private Animator splashAnimator;
    private int hashSplash;

    [SerializeField] private Button touchButton;
    [SerializeField] private Image star;
    [SerializeField] private Image shine;

    private float animationTime = 0.8f;
    private float splashTime = 3.3f; //임시 지정
    private Color targetColor = Color.white;

    private (float width, float height) starSize;
    private (float width, float height) shineSize;

    private void OnDestroy()
    {
        touchButton.onClick.RemoveAllListeners();
    }

    private void Awake()
    {
        splashAnimator = GetComponent<Animator>();
        hashSplash = Animator.StringToHash("Splash");

        //var clips = splashAnimator.runtimeAnimatorController.animationClips;
        //int count = clips.Length;
        //for (int i = 0; i < count; i++)
        //{
        //    if (clips[i].GetHashCode() == hashSplash)
        //    {
        //        splashTime = clips[i].length;
        //    }
        //}

        touchButton.onClick.RemoveAllListeners();
        touchButton.onClick.AddListener(TriggerRollEffect);

        FlagEnd = false;
    }

    public void SetColor(Color _color)
    {
        targetColor = _color;
    }

    public void StartSplash()
    {
        FlagEnd = false;
        gameObject.SetActive(true);
    }

    //애니메이션 이벤트
    public void CloseSplash()
    {
        RectTransform starRect = star.gameObject.GetComponent<RectTransform>();
        starRect.localScale = Vector3.one;

        RectTransform shineRect = shine.gameObject.GetComponent<RectTransform>();
        starRect.localScale = Vector3.one;
        FlagEnd = true;
        gameObject.SetActive(false);
    }

    private void TriggerRollEffect()
    {
        splashAnimator.SetTrigger(hashSplash);
    }

    //애니메이션 이벤트
    private void ChangeColor()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeColorCo());
    }

    private void ChangeAlpha()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeAlphaCo());
    }

    private IEnumerator ChangeColorCo()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            star.color = Color.Lerp(Color.white, targetColor, time - (animationTime * 0.2f));
            shine.color = Color.Lerp(Color.white, targetColor, time - (animationTime * 0.2f));

            if (time >= animationTime)
            {
                break;
            }

            yield return null;
        }

        shine.color = targetColor;

        yield break;
    }

    private IEnumerator ChangeAlphaCo()
    {
        float time = 0f;
        UnityEngine.Color alpha02 = new UnityEngine.Color(1f, 1f, 1f, 0.2f);

        star.color = UnityEngine.Color.white;
        while (true)
        {
            time += Time.deltaTime;


            if (time < splashTime / 2)
            {
                shine.color = UnityEngine.Color.Lerp(UnityEngine.Color.white, alpha02, time);
            }
            else if (time >= splashTime / 2)
            {
                shine.color = UnityEngine.Color.Lerp(alpha02, UnityEngine.Color.white, time - (splashTime / 2));
            }

            if (time >= splashTime)
            {
                break;
            }

            yield return null;
        }

        shine.color = UnityEngine.Color.white;

        yield break;
    }
}
