using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 전역동작변수
    public int ID;     // 플레이어 ID
    public bool ableControlMove = true;    // 캐릭터의 움직임 조작가능 여부
    public bool ableControlAttack = true;   // 캐릭터의 공격 가능 여부
    #endregion

    private void OnEnable() 
    {
        ID = GameManager.Instance.GetID();
    }

    private void OnDisable() 
    {
        GameManager.Instance.ReturnID(ID);
        ID = 0;
    }
}
