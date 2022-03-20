using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   #region 전역 동작 변수
   public int curLivingPlayer;  // 현재 게임의 살아있는 플레이어 수
   private int[] curPlayerCount = new int[4];   // 플레이어 ID 풀: 0-없음, 1-있음, 0번 ID-ID발급 거부
   #endregion

    //싱글톤
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    // 플레이어 ID부여
    public int GetID()
    {   
        for(int i=0; i < 4 ; i++)
        {
            if(curPlayerCount[i] == 0)
            {
                curPlayerCount[i] = 1;
                curLivingPlayer++;
                return i+1 ;
            }
        }

        return 0;   // 발급 불가능
    }

    // 플레이어 ID해제
    public void ReturnID(int _ID)
    {
        if(_ID == 0 ) return;
        curPlayerCount[_ID-1] = 0;
        curLivingPlayer--;
    }
}
