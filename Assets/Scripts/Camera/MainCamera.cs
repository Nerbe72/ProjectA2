using System.Collections;
using UnityEngine;

public partial class MainCamera : MonoBehaviour
{
    bool CameraShake = false;

    Transform ShakeTr;

    public class ShakeInfo
    {
        public float StartDelay;
        public bool UseTotalTime;
        public float TotalTime;
        public Vector3 Dest;
        public Vector3 Shake;
        public Vector3 Dir;

        public float RemainDist;
        public float RemainCountDis;

        public bool UseCount;
        public int Count;

        public float Veclocity;

        public bool UseDamping;
        public float Damping;
        public float DampingTime;
    }

    ShakeInfo shakeInfo = new ShakeInfo();

    Vector3 OrgPos;

    float FovX = 0.2f;
    float FovY = 0.2f;

    float Left = -1f;
    float Right = 1f;

    private void Awake()
    {
        Singleton.Add<MainCamera>(this);

        Left = -1f;
        Right = 1f;

        OrgPos = transform.position;

        InitShake();
    }

    protected void InitShake()
    {
        ShakeTr = transform.parent;
        CameraShake = false;
    }

    protected void ResetShakeTr()
    {
        ShakeTr.localPosition = Vector3.zero;
        CameraShake = false;

        CameraLimit();
    }

    void CameraLimit(bool OrgPosY = false)
    {
        Vector3 camera = OrgPos;

        if (camera.x - FovX < Left)
            camera.x = Left + FovX;
        else if (camera.x + FovX > Right)
            camera.x = Right - FovX;

        if (OrgPosY)
            camera.y = OrgPos.y;
    }

    public void Shake(int CameraID)
    {
        shakeInfo.StartDelay = 0f;
        shakeInfo.TotalTime = 3f;
        shakeInfo.UseTotalTime = true;

        shakeInfo.Shake = new Vector3(0.2f, 0.2f, 0f);

        shakeInfo.Dest = shakeInfo.Shake;
        shakeInfo.Dir = shakeInfo.Shake;
        shakeInfo.Dir.Normalize();

        shakeInfo.RemainDist = shakeInfo.Shake.magnitude;
        shakeInfo.RemainCountDis = float.MaxValue;

        shakeInfo.Veclocity = 8;

        shakeInfo.Damping = 0.5f;
        shakeInfo.UseDamping = true;

        shakeInfo.DampingTime = shakeInfo.RemainDist / shakeInfo.Veclocity;

        shakeInfo.Count = 4;
        shakeInfo.UseCount = true;

        StopCoroutine("ShakeCoroutine");
        ResetShakeTr();
        StartCoroutine("ShakeCoroutine");
    }

    IEnumerator ShakeCoroutine()
    {
        CameraShake = true;

        float dt, dist;

        if (shakeInfo.StartDelay > 0)
            yield return new WaitForSeconds(shakeInfo.StartDelay);

        while (true)
        {
            dt = Time.fixedDeltaTime;
            dist = dt * shakeInfo.Veclocity;

            if ((shakeInfo.RemainDist -= dist) > 0)
            {
                ShakeTr.localPosition += shakeInfo.Dir * dist;

                float rc = transform.position.x - FovX - Left;

                if (rc < 0)
                    ShakeTr.localPosition += new Vector3(-rc, 0, 0);

                rc = Right - (transform.position.x + FovX);

                if (rc < 0)
                    ShakeTr.localPosition += new Vector3(rc, 0, 0);

                CameraLimit(true);

                if (shakeInfo.UseCount)
                {
                    if ((shakeInfo.RemainCountDis -= dist) < 0)
                    {
                        shakeInfo.RemainCountDis = float.MaxValue;

                        if (--shakeInfo.Count < 0)
                            break;
                    }
                }
            }
            else
            {
                if (shakeInfo.UseDamping)
                {
                    float distdamping = Mathf.Max(shakeInfo.Damping * shakeInfo.DampingTime,
                        shakeInfo.Damping * dt);

                    if (shakeInfo.Shake.magnitude > distdamping)
                        shakeInfo.Shake -= shakeInfo.Dir * distdamping;
                    else
                    {
                        shakeInfo.UseCount = true;
                        shakeInfo.Count = 1;
                    }
                }

                ShakeTr.localPosition = shakeInfo.Dest - shakeInfo.Dir * (-shakeInfo.RemainDist);

                float rc = transform.position.x - FovX - Left;

                if (rc < 0)
                    ShakeTr.localPosition += new Vector3(-rc, 0, 0);

                rc = Right - (transform.position.x + FovX);

                if (rc < 0)
                    ShakeTr.localPosition += new Vector3(rc, 0, 0);

                CameraLimit(true);

                shakeInfo.Shake = -shakeInfo.Shake;
                shakeInfo.Dest = shakeInfo.Shake;
                shakeInfo.Dir = -shakeInfo.Dir;

                float len = shakeInfo.Shake.magnitude;

                shakeInfo.RemainCountDis = len + shakeInfo.RemainDist;
                shakeInfo.RemainDist += len * 2f;

                shakeInfo.DampingTime = shakeInfo.RemainDist / shakeInfo.Veclocity;

                if (shakeInfo.RemainDist < dist)
                    break;
            }

            if (shakeInfo.UseTotalTime && (shakeInfo.TotalTime -= dt) < 0)
                break;

            yield return new WaitForFixedUpdate();
        }

        ResetShakeTr();

        yield break;
    }
}
