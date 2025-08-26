using System.Collections.Generic;
using TMPro;
using UnityEngine;

using GameStuff;

public class DamageIndicator : MonoBehaviour
{
    [Header("이동")]
    public float DropSpeed = 4f;
    public float RiseHeight = 0.3f;
    public float RiseDuration = 0.05f;
    public float FallDuration = 0.5f;

    [Header("텍스트")]
    public TMP_Text damageText;
    public float MaxTextScale = 1.2f;
    public float MinTextScale = 0.8f;

    private Transform cameraTransform;
    private Vector3 startPosition;
    private float currentTime = 0f;
    private bool isRising = true;
    private Dictionary<AttackType, UnityEngine.Color> damageColors = new Dictionary<AttackType, UnityEngine.Color>
    {
        { AttackType.Physical, new UnityEngine.Color(1f, 0.92f, 0.016f, 1f) },
        { AttackType.Magical, new UnityEngine.Color(0.4f, 0.8f, 1f, 1f) },
        { AttackType.Fixed, new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1f) }
    };


    private void OnEnable()
    {
        cameraTransform = Camera.main.transform;

        currentTime = 0f;
        isRising = true;
        damageText.color = new UnityEngine.Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
        damageText.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        Drop();
    }

    private void LateUpdate()
    {
        transform.LookAt(cameraTransform);
    }

    public void InitIndicator(AttackType _type, int _damage)
    {
        cameraTransform = Camera.main.transform;
        startPosition = transform.position;

        damageText.text = _damage.ToString();
        damageText.color = damageColors[_type];
    }

    public void Drop()
    {
        currentTime += Time.deltaTime;

        if (isRising)
        {
            float progress = currentTime / RiseDuration;
            if (progress >= 1f)
            {
                isRising = false;
                currentTime = 0f;
                return;
            }

            float height = Mathf.Lerp(0f, RiseHeight, progress);
            transform.position = startPosition + Vector3.up * height;

            float scale = Mathf.Lerp(1f, MaxTextScale, progress);
            damageText.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            float progress = currentTime / FallDuration;
            if (progress >= 1f)
            {
                gameObject.SetActive(false);
                return;
            }

            float height = Mathf.Lerp(RiseHeight, 0f, progress);
            transform.position = startPosition + Vector3.up * height;

            float scale = Mathf.Lerp(MaxTextScale, MinTextScale, progress);
            damageText.transform.localScale = new Vector3(scale, scale, scale);

            UnityEngine.Color currentColor = damageText.color;
            damageText.color = new UnityEngine.Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(1f, 0f, progress));
        }
    }
}
