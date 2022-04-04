using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 레벨디자인의 목표임무, 플레이어의 체력회복에 사용할 상호작용을 대리하는 클래스
// 부착 물체의 태그는 InterAction이어야 한다.

public class InterActionAgent : MonoBehaviour
{
    // 전역 변수
    public float interActionTime; // 상호작용을 완료하는데 걸리는 시간
    // 전역 동작 변수
    public bool interActionComplete;   // 상호작용을 완료했는지 여부
}
