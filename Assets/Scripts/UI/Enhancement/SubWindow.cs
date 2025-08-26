using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class SubWindow : MonoBehaviour
{
    public int Index;
    public bool IsSelected;

    private Animator animator;

    protected virtual void Awake()
    {
        Index = transform.GetSiblingIndex();
        animator = GetComponent<Animator>();
    }

    public virtual void Swap(SubWindow _from)
    {
        _from?.SetSelected(false);
        SetSelected(true);
    }

    public virtual void SetSelected(bool _isSelected)
    {
        IsSelected = _isSelected;
        animator.SetBool("Selected", IsSelected);

        if (IsSelected)
            transform.SetAsLastSibling();
        else
            transform.SetAsFirstSibling();
    }
}
