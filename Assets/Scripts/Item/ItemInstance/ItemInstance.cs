using System;

[Serializable]
public abstract class ItemInstance
{
    public Guid InventoryID;
    public string InventoryIDString;
    public int ItemID;

    public ItemInstance()
    {
        InventoryID = Guid.NewGuid();
        InventoryIDString = InventoryID.ToString();
    }

    public abstract bool OnUse(int _useAmount = 1);
}
