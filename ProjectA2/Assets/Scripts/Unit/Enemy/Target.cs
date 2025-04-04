using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Target : MonoBehaviour
{
    private TargetManager targetManager;
    private Player player;

    private Coroutine c_removeTargetHolder;

    [SerializeField] private GameObject indicator;

    Camera cam;

    private bool isInSight = false;

    private void Start()
    {
        targetManager = SingletonManager.targetManager;
        player = SingletonManager.player;
        cam = SingletonManager.cameraManager.main;
    }

    private void LateUpdate()
    {
        if (cam.transform != null)
        {
            transform.LookAt(cam.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어로부터 특정 범위에 들어온 경우 타깃 리스트에 추가됨
        if (other == null) return;
        if (other.tag != "TargetSearcher") return;

        StopAllCoroutines();
        targetManager.AddTarget(this, isInSight);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.tag != "TargetSearcher") return;

        if (c_removeTargetHolder != null) return;

        c_removeTargetHolder = StartCoroutine(RemoveTargetDelayCo());

        //일정 시간 대기 후 타겟 배열에서 삭제
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
    }

    private IEnumerator RemoveTargetDelayCo()
    {
        yield return new WaitForSeconds(1f);

        targetManager.RemoveTarget(this);
        c_removeTargetHolder = null;
        yield break;
    }
}
