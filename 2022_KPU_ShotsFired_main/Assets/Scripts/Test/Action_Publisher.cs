using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Action_Publisher : MonoBehaviour
{
    public Action<float> publishTester;

    public float HealthMultiple = 2f;

    // �ۺ��� ����
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) publishTester(HealthMultiple);
    }
}
