using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Subscriber : MonoBehaviour
{
    public Action_Publisher publisher;

    float healthMultiple;

    // 액션 등록
    private void Start()
    {
        publisher.publishTester += SetHealthMultiple;
    }

    // 실행되어야할 함수
    void SetHealthMultiple(float _healthMultiple)
    {
        healthMultiple = _healthMultiple;
    }

    // 실행되는 함수
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("Subscribe: "+ healthMultiple);
    }
}
