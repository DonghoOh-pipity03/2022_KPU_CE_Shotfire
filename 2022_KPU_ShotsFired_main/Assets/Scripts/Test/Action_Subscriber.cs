using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Subscriber : MonoBehaviour
{
    public Action_Publisher publisher;

    float healthMultiple;

    // 구독
    private void Start()
    {
        publisher.publishTester += SetHealthMultiple;
    }

    // 등록한 함수
    void SetHealthMultiple(float _healthMultiple)
    {
        healthMultiple = _healthMultiple;
    }

    // 작동확인
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("Subscribe: "+ healthMultiple);
    }
}
