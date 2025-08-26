using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class ItemFrame : ClickableFrame
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!selected) return;

        base.OnPointerClick(eventData);
    }

    public override async Task SetFrameData(ItemInstance _item)
    {
        await base.SetFrameData(_item);

        if (_item is WeaponItemInstance weaponInstance)
        {
            var weaponAdapter = Singleton.Inventory.GetWeaponAdapter(weaponInstance);
            if (weaponAdapter != null && weaponAdapter.EnhancedLevel > 0)
            {
                SetEnhancementText($"+{weaponAdapter.EnhancedLevel}");
            }
            else
            {
                SetEnhancementText("");
            }
        }
        else
        {
            SetEnhancementText("");
        }
    }

    public void SetEquipped()
    {
        equippedText.gameObject.SetActive(true);
    }

    public void UnEquipped()
    {
        equippedText.gameObject.SetActive(false);
    }
}
