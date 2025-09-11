using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class UIButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    private int successHash = Animator.StringToHash("Success");
    private int failureHash = Animator.StringToHash("Failure");
    private int hoverHash = Animator.StringToHash("Hover");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlaySuccessAnimation()
    {
        animator.SetBool(successHash, true);
    }

    public void PlayFailureAnimation()
    {
        animator.SetBool(failureHash, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool(hoverHash, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool(hoverHash, false);
    }

    public void ResetAnimations()
    {
        animator.SetBool(successHash, false);
        animator.SetBool(failureHash, false);
    }
}
