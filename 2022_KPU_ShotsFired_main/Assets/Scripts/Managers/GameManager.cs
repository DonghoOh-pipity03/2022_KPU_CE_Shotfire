using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   #region 전역 동작 변수
   public int curLivingPlayer;  // 현재 게임의 살아있는 플레이어 수
   public GameObject[] players = new GameObject[4];   // 플레이어 ID 풀, 플레이어의 gameobject로 관리
   bool ispPlaying;  // 게임이 진행 중인지 여부
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

    private void Update() 
    {
        ObserveDefeatGame();
    }

    // 플레이어 ID부여
    public int GetID(GameObject _player)
    {   
        for(int i=0; i < 4 ; i++)
        {
            if(players[i] == null)
            {
                players[i] = _player;
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
        players[_ID-1] = null;
        curLivingPlayer--;
    }

    // 게임 시작
    public void StartGame()
    {
        ispPlaying = true;
    }

    // 게임 종료
    public void EndGame(bool _isVictory)
    {
        // UI 처리
        GameUIManager.Instance.SetActiveReport(true);
        GameUIManager.Instance.SetActivePlayer(false);
        GameUIManager.Instance.SetMouseLock(false);

        // 오브젝트 처리
        PhotonInit.PhotonInit.Instance.end_game();

        ispPlaying = false;
    }

    // 게임 패배 감지
    private void ObserveDefeatGame()
    {
        if(curLivingPlayer <= 0 && ispPlaying) EndGame(false);
    }
}
