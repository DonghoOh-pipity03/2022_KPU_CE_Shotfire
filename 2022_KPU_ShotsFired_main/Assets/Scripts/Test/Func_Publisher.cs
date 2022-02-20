using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Func_Publisher : MonoBehaviour
{
    public event Func<float, bool> publishTester;

    float damageMultiple = 1.5f;

    private void Update()
    {
        // �ۺ��� ����
        if (Input.GetKey(KeyCode.Space))
        {
            var temp = publishTester(damageMultiple);
            Debug.Log("publisher: " + temp);
        }
    }
}
