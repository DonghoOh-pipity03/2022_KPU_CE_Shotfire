// 월드 스페이스 UI를 카메라 방향으로 맞추기 위한 스크립트
// 메인 카메라는 "MainCamera" 태그를 가지고 있어야 한다.
using UnityEngine;

public class BillBoarder : MonoBehaviour
{
    public  Camera cameraToLookAt;
    void Start () => cameraToLookAt = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();

    void Update () =>
        transform.LookAt (transform.position + cameraToLookAt.transform.rotation * Vector3.forward,
                            cameraToLookAt.transform.rotation * Vector3.up);
}
