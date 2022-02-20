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

    // ������ ��� �Լ�
    public bool SetMoveConf(float temp)
    {
        Debug.Log("Subscriber: "+temp);
        return canMove;
    }
}
