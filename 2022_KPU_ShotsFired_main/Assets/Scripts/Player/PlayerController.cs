using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 전역동작변수
    [HideInInspector]public int ID;     // 플레이어 ID
    [HideInInspector]public bool ableControlMove = true;    // 캐릭터의 움직임 조작가능 여부
    [HideInInspector]public bool ableControlAttack = true;   // 캐릭터의 공격 가능 여부
    [HideInInspector]public bool ableControlInterAction = true; // 캐릭터의 상호작용 가능 여부
    #endregion

    private void OnEnable() 
    {
        ID = GameManager.Instance.GetID(this.gameObject);
    }

    private void OnDisable() 
    {
        if(GameManager.Instance != null) GameManager.Instance.ReturnID(ID);  // 에디터 버전에서 게임 종료시 경고 알림 방지용
        ID = 0;
    }

    private void OnApplicationQuit() {
        if(GameManager.Instance != null) GameManager.Instance.ReturnID(ID);  // 에디터 버전에서 게임 종료시 경고 알림 방지용
        ID = 0;
    }
}
