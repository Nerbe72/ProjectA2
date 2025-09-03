using System;
using TMPro;
using UnityEngine;

public class MinigameSuccessIndicator : MonoBehaviour
{
    public event Action OnAnimationFinished;

    [SerializeField] private TMP_Text successText1;
    [SerializeField] private TMP_Text successText2;
    private Animator animator;
    private int successHash = Animator.StringToHash("Success");
    private int failHash = Animator.StringToHash("Fail");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowSuccess(bool _success)
    {
        gameObject.SetActive(true);

        successText1.text = _success ? "SUCCESS" : "FAIL";
        successText2.text = _success ? "SUCCESS" : "";
        successText1.color = _success ? Color.green : Color.red;
        successText2.color = _success ? Color.green : Color.red;
        animator.SetBool(_success ? successHash : failHash, true);
    }

    /// <summary>
    /// 애니메이션 끝에서 호출
    /// </summary>
    private void OnAnimationEnd()
    {
        animator.SetBool(successHash, false);
        animator.SetBool(failHash, false);

        OnAnimationFinished?.Invoke();
        gameObject.SetActive(false);
    }
}
