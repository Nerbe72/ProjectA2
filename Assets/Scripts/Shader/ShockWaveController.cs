using System.Collections;
using UnityEngine;

public class ShockWaveController : MonoBehaviour
{
    private Material material;

    public float shockSpeed;
    public float shockTime;
    public bool reverse = false;
    public Vector2 thickMinMax;

    private readonly int centerID = Shader.PropertyToID("_Center");
    private readonly int customTimeID = Shader.PropertyToID("_CustomTime");
    private readonly int thicknessID = Shader.PropertyToID("_RingThickness");

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        material = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartShock(Singleton.Player.transform.position);
        }
    }

    public void StartShock(Vector3 position)
    {
        Vector2 screenPosition = Camera.main.WorldToViewportPoint(position);

        material.SetVector(centerID, screenPosition);
        material.SetFloat(customTimeID, -0.1f);

        StopAllCoroutines();
        // Start the shock wave effect
        StartCoroutine(ShockLerp());
    }

    private IEnumerator ShockLerp()
    {
        float time = -0.1f;

        while (true)
        {
            time += Time.deltaTime * shockSpeed;

            material.SetFloat(customTimeID, time);
            material.SetFloat(thicknessID, Mathf.Clamp((reverse ? (shockTime - time) : time), thickMinMax.x, thickMinMax.y));

            if (time >= shockTime) break;
            yield return null;
        }


        material.SetFloat(customTimeID, shockTime);
        yield break;
    }
}
