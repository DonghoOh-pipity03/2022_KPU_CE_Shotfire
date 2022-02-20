using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Subscriber : MonoBehaviour
{
    public Action_Publisher publisher;

    float healthMultiple;

    // ����
    private void Start()
    {
        publisher.publishTester += SetHealthMultiple;
    }

    // ����� �Լ�
    void SetHealthMultiple(float _healthMultiple)
    {
        healthMultiple = _healthMultiple;
    }

    // �۵�Ȯ��
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("Subscribe: "+ healthMultiple);
    }
}
