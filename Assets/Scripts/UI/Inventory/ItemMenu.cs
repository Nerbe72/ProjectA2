using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class ItemMenu : MonoBehaviour
{
    [SerializeField] private Button UseButton;
    [SerializeField] private Button GrindButton;
    private RectTransform rectTransform;

    private ItemInstance currentItem;

    private void Awake()
    {
        UseButton.onClick.AddListener(ClickUse);
        GrindButton.onClick.AddListener(ClickGrind);
        rectTransform = GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }

    public void ShowItemMenu(ItemInstance _item, Vector2 _position)
    {
        currentItem = _item;

        // 화면 밖으로 나가지 않도록 위치 조정
        Vector2 pos = _position;
        Vector2 size = rectTransform.sizeDelta;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // 오른쪽 끝이나 위쪽 끝에 너무 가까우면 위치 조정
        if (pos.x + size.x > screenSize.x)
            pos.x = screenSize.x - size.x;
        if (pos.y + size.y > screenSize.y)
            pos.y = screenSize.y - size.y;

        rectTransform.position = pos;

        var item_selected = (ItemType)Singleton.Get<TableDataManager>().Table.Item.Get(_item.ItemID).ItemType;
        switch (item_selected)
        {
            case ItemType.Weapon:
                UseButton.interactable = true;
                GrindButton.interactable = true;
                break;
            case ItemType.Potion:
                UseButton.interactable = true;
                GrindButton.interactable = false;
                break;
        }

        gameObject.SetActive(true);
    }

    private void ClickUse()
    {
        if (currentItem == null) return;

        int index = Singleton.Inventory.GetItemIndex(currentItem);
        if (index != -1)
        {
            Singleton.Inventory.UseItem(index);
        }
        gameObject.SetActive(false);
    }

    private void ClickGrind()
    {
        if (currentItem == null) return;

        int index = Singleton.Inventory.GetItemIndex(currentItem);
        if (index != -1)
        {
            Singleton.Inventory.GrindWeapon(currentItem as WeaponItemInstance);
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // 메뉴가 열려있을 때 다른 곳을 클릭하면 메뉴 닫기
        if (gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
