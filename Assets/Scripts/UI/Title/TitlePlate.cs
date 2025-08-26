using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitlePlate : PlateBase
{
    [SerializeField] private Button enterButton;
    [SerializeField] private MeshRenderer logoMeshRenderer;
    [SerializeField] private float shineSpeed = 0.5f;
    [SerializeField] private float confirmSpeed = 2f;
    [SerializeField] [ColorUsage(true)] private Color enterColor;

    private Coroutine shine;
    private Coroutine confirm;
    private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private MaterialPropertyBlock propertyBlock;
    private Color currentOutlineColor = Color.black;

    protected override void Awake()
    {
        base.Awake();

        enterButton.gameObject.SetActive(false);
        enterButton.onClick.AddListener(EnterGame);
        if (logoMeshRenderer == null)
            logoMeshRenderer = GetComponentInChildren<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        var outlineMaterial = logoMeshRenderer ? logoMeshRenderer.sharedMaterial : null;
        if (outlineMaterial != null && outlineMaterial.HasProperty(outlineColorID))
            currentOutlineColor = outlineMaterial.GetColor(outlineColorID);
    }

    private void Start()
    {
        var outlineMaterial = logoMeshRenderer ? logoMeshRenderer.sharedMaterial : null;
        if (outlineMaterial != null && outlineMaterial.HasProperty(outlineColorID))
            shine = StartCoroutine(ShineCoroutine());
    }

    public void ActivateEnterButton(bool _logined)
    {
        enterButton.gameObject.SetActive(_logined);
    }

    private void EnterGame()
    {
        if (shine != null)
        {
            StopCoroutine(shine);
            shine = null;
        }

        if (confirm != null) return;

        confirm = StartCoroutine(ConfirmCoroutine());
        Singleton.Get<GameManager>().StartGame();
    }

    private IEnumerator ShineCoroutine()
    {
        if (logoMeshRenderer == null)
            yield break;

        var colorA = Color.black;
        var colorB = Color.white;

        while (true)
        {
            float time = Mathf.PingPong(Time.time * shineSpeed, 1f);
            Color pulsedOutlineColor = Color.Lerp(colorA, colorB, time);

            logoMeshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(outlineColorID, pulsedOutlineColor);
            logoMeshRenderer.SetPropertyBlock(propertyBlock);
            currentOutlineColor = pulsedOutlineColor;

            yield return null;
        }
    }

    private IEnumerator ConfirmCoroutine()
    {
        if (logoMeshRenderer == null)
            yield break;

        Color startColor = currentOutlineColor;
        Color endColor = enterColor;

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * confirmSpeed;
            Color interpolatedColor = Color.Lerp(startColor, endColor, Mathf.Clamp01(time));
            logoMeshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(outlineColorID, interpolatedColor);
            logoMeshRenderer.SetPropertyBlock(propertyBlock);
            currentOutlineColor = interpolatedColor;
            yield return null;
        }
        confirm = null;
    }
}
