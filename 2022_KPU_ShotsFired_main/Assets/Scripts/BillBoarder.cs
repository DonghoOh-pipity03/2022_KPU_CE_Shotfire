/*
월드 스페이스 UI를 카메라 방향으로 맞추기 위한 스크립트
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoarder : MonoBehaviour
{
    public  Camera cameraToLookAt;
    void Start () 
    {
        cameraToLookAt = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
    }

    void Update () 
    {
        
        transform.LookAt (transform.position + cameraToLookAt.transform.rotation * Vector3.forward,
                            cameraToLookAt.transform.rotation * Vector3.up);
    }
}
