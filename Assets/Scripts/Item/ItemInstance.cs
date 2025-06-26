using System;

[Serializable]
public abstract class ItemInstance// : IItemInstance
{
    public Guid InventoryID;
    public string InventoryIDString;
    public int ItemID;

    public ItemInstance()
    {
        InventoryID = Guid.NewGuid();
        InventoryIDString = InventoryID.ToString();
    }

    public virtual void OnUse() { }
}
