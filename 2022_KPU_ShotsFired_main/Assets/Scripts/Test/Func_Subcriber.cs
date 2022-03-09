using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Func_Subcriber : MonoBehaviour
{ 
    public Func_Publisher publisher;

    private bool canMove = true;

    private void Start()
    {
        publisher.publishTester += SetMoveConf;
    }

    // 구독자 등록 함수
    public bool SetMoveConf(float temp)
    {
        Debug.Log("Subscriber: "+temp);
        return canMove;
    }
}
