using System.Threading.Tasks;
using UnityEngine;

public interface IItemInstance
{
    int ID { get; set; }
    string Name { get; set; }
    ItemType Type { get; set; }
    ItemData Data { get; set; }
    public Rare Rarity { get; set; }

    void OnUse();
    Task<GameObject> InstantiateAsync();
}
