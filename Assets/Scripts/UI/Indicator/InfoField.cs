using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum AxisType
{
    Horizontal,
    Vertical
}

public class InfoField : MonoBehaviour
{
    [SerializeField] private GameObject ImageFrame; // 아이콘 이미지 프레임. 아이콘이 없다면 비활성화
    // 아이콘 이미지는 imageframe의 getcomponentinchildren<image>()로 사용
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Data;

    public float Height = 30f;
    public float paddingLR = 10f;

    public void SetData(AxisType _axis, string _name, string _data, float _height = 30f, Sprite _icon = null)
    {
        var horizontal = GetComponent<HorizontalLayoutGroup>();
        var vertical = GetComponent<VerticalLayoutGroup>();

        Name.text = _name;
        Data.text = _data;

        switch (_axis)
        {
            case AxisType.Horizontal:
                {
                    if (vertical != null)
                        DestroyImmediate(vertical);

                    if (horizontal == null)
                        horizontal = gameObject.AddComponent<HorizontalLayoutGroup>();

                    horizontal.childAlignment = TextAnchor.MiddleLeft;
                    horizontal.padding.left = (int)paddingLR;
                    horizontal.padding.right = (int)paddingLR;
                    horizontal.childControlHeight = false;
                    horizontal.childControlWidth = true;
                    horizontal.childForceExpandHeight = true;
                    horizontal.childForceExpandWidth = true;
                }
                break;
            case AxisType.Vertical:
                {
                    if (horizontal != null)
                        DestroyImmediate(horizontal);

                    if (vertical == null)
                        vertical = gameObject.AddComponent<VerticalLayoutGroup>();

                    vertical.childAlignment = TextAnchor.MiddleLeft;
                    vertical.padding.left = (int)paddingLR;
                    vertical.padding.right = (int)paddingLR;
                    vertical.childControlHeight = false;
                    vertical.childControlWidth = true;
                    vertical.childForceExpandHeight = true;
                    vertical.childForceExpandWidth = true;

                    if (Data != null)
                    {
                        var dataRectTransform = Data.GetComponent<RectTransform>();
                        if (dataRectTransform != null)
                        {
                            dataRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height - 30f);
                        }
                    }
                }
                break;
        }

        if (_icon != null)
        {
            var image = ImageFrame.GetComponentInChildren<Image>();
            if (image != null)
            {
                image.sprite = _icon;
                image.preserveAspect = true;
            }
            ImageFrame.SetActive(true);
        }
        else
        {
            if (ImageFrame != null)
                ImageFrame.SetActive(false);
        }

        Height = _height;
        
        // 실제 UI 높이 설정
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();
            
        layoutElement.preferredHeight = _height;
    }
}
