using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public IHurtable owner;
    private TargetManager targetManager;
    private Player player;

    private Coroutine co_removeTargetHolder;

    [SerializeField] private GameObject indicator;

    Camera cam;

    private bool isInSight = false;

    private void Start()
    {
        owner = GetComponentInParent<IHurtable>();
        targetManager = Singleton.Get<TargetManager>();
        player = Singleton.Player;
        cam = Singleton.Get<CameraManager>().main;
    }

    private void LateUpdate()
    {
        if (cam != null)
        {
            transform.LookAt(cam.transform);
            transform.localScale = Vector3.one * 0.04f * Vector3.Distance(transform.position, cam.transform.position);
        }
    }

    private void OnDisable()
    {
        if (targetManager.CurrentTarget == this)
        {
            targetManager.UnSetTarget();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어로부터 특정 범위에 들어온 경우 타깃 리스트에 추가됨
        if (other == null) return;

        if (targetManager == null)
        {
            return;
        }

        StopAllCoroutines();
        targetManager.AddTarget(this, isInSight);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.tag != "TargetSearcher") return;

        if (co_removeTargetHolder != null) return;

        //일정 시간 대기 후 타겟 배열에서 삭제
        StopAllCoroutines();
        co_removeTargetHolder = StartCoroutine(RemoveTargetDelayCo());
    }

    private void OnDestroy()
    {
        targetManager.RemoveTarget(this);
    }

    private void OnBecameVisible()
    {
        isInSight = true;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (Physics.Raycast(transform.position, direction, distance, LayerMask.GetMask("Wall"))) return;
        targetManager.AddTargetVisible(this);
    }

    private void OnBecameInvisible()
    {
        //본인이 현재 타깃인 경우 시간 지연 후 제거
        //본인이 현재 타깃이 아닌 경우 즉시 제거
        //if (targetManager.CurrentTarget == this){}
        isInSight = false;
        targetManager.RemoveTargetVisible(this);

    }

    public void SetIndicatorVisibility(bool _true)
    {
        indicator.SetActive(_true);

        if (_true)
        {
            StopAllCoroutines();
            StartCoroutine(ShowCoroutine());
        }
    }

    private IEnumerator RemoveTargetDelayCo()
    {
        yield return new WaitForSeconds(0.5f);

        targetManager.RemoveTarget(this);
        co_removeTargetHolder = null;
        yield break;
    }

    public IEnumerator ShowCoroutine()
    {
        float time = 0f;

        Vector3 from = Vector3.one * 0.8f;
        Vector3 to = Vector3.one * 0.5f;

        while (true)
        {
            time += Time.deltaTime * 2f;

            indicator.transform.localScale = Vector3.Lerp(from, to, time);
            if (time >= 1f) break;
        }

        yield break;
    }
}
