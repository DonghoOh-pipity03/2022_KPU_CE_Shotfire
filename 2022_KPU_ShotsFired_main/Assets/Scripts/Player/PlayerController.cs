using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 전역동작변수
    public int ID;     // 플레이어 ID
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
