using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    protected InteractType interactType;
    protected string shownString;

    public event Action OnInteractStart;
    public event Action OnInteractEnd;

    public InteractType InteractType => interactType;

    public string ShownString => shownString;

    public bool IsNowInteractable { get; private set; }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        Singleton.Get<InteractIndicator>().SetShowIndicator(true, 10000043);
        Singleton.Get<InteractManager>().SetInteract(this);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        Singleton.Get<InteractIndicator>().SetShowIndicator(false);
        Singleton.Get<InteractManager>().UnSetInteract(this);
    }

    public void SetInteractType(InteractType _type)
    {
        interactType = _type;
    }

    public virtual void DoAction()
    {
        //interact action
    }

    public virtual void EndAction()
    {

    }

    public void ShowIndicator(bool _show)
    {

    }
}
