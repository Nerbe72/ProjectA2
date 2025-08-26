using GameStuff;
using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class DroppedItem : MonoBehaviour, IInteractable, IItemContainer
{
    public InteractType InteractType => InteractType.Item;

    public bool IsNowInteractable => throw new NotImplementedException();

    private ItemInstance item;
    private int amount;
    ItemInstance IItemContainer.Item { get => item; set => item = value; }
    int IItemContainer.Amount { get => amount; set => amount = value; }

    private string shownString;

    string IInteractable.ShownString => shownString;

    public event Action OnInteractStart;
    public event Action OnInteractEnd;
    public void DoAction()
    {
        if (item != null)
        {
            Singleton.Inventory.TakeItem(item);
            Singleton.Inventory.SaveInventoryData();
        }

        EndAction();
        Destroy(gameObject);
    }

    public void EndAction()
    {
        Singleton.Get<InteractIndicatorUI>().SetShowIndicator(false);
        Singleton.Get<InteractManager>().UnSetInteract(this);
    }

    void IItemContainer.SetItemContainer(int _id, int _itemCount)
    {
        var item_created = Singleton.Get<ItemFactory>().CreateItem(_id, true);
        item = item_created;

        if (item is IStackable _stackable)
        {
            _stackable.CurrentStack = _itemCount;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        Singleton.Get<InteractIndicatorUI>().SetShowIndicator(true, 11000001);
        Singleton.Get<InteractManager>().SetInteract(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        Singleton.Get<InteractIndicatorUI>().SetShowIndicator(false);
        Singleton.Get<InteractManager>().UnSetInteract(this);
    }
}
