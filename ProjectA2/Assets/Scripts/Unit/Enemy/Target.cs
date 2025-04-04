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
        //�÷��̾�κ��� Ư�� ������ ���� ��� Ÿ�� ����Ʈ�� �߰���
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

        //���� �ð� ��� �� Ÿ�� �迭���� ����
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
        //������ ���� Ÿ���� ��� �ð� ���� �� ����
        //������ ���� Ÿ���� �ƴ� ��� ��� ����
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
