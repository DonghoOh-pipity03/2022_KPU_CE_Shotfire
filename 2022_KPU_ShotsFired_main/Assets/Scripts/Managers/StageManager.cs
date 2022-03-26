using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StageManager : MonoBehaviour
{
    
    #region 전역 변수
    [Tooltip("사용을 원하지 않으면 공백으로 두기")]
    [SerializeField] private string nextSceneName;  // 다음 스테이지 씬 이름
    [SerializeField] private string prevSceneName;  // 이전 스테이지 씬 이름
    [Header("마지막 스테이지 전용 세팅")]

    [SerializeField] bool useForLastStage;  // 게임이 최종적으로 끝나는 스테이지에서 사용할 것인지
    #endregion
    #region 전역 동작 변수
    private int curPlayerCountInRoom;   // 현재 세이프룸 인원
    private bool startedNextScene = false;  // 다음 씬의 로드가 시작되었는지 여부
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") curPlayerCountInRoom++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") curPlayerCountInRoom--;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!startedNextScene && other.tag == "Player") SetStage();
    }

    // 세이프룸 안의 플레이어수가 살아있는 플레이어 수와 같은지 검사하고, 
    // 다음 스테이지를 비동기 로딩 및 이전 스테이지를 비동기 언로딩한다.
    private void SetStage()
    {
        if (curPlayerCountInRoom == GameManager.Instance.curLivingPlayer)
        {
            if (!useForLastStage)    // 일반 스테이지일 때
            {
                if (nextSceneName != "") StartCoroutine(GameSceneManager.Instance.AddSceneWithAsync(nextSceneName));
                if (prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName));
                startedNextScene = true;
            }
            else    // 마지막 스테이지일 때
            {
                GameUIManager.Instance.SetActiveReport(true);
                GameUIManager.Instance.SetActivePlayer(false);
                GameUIManager.Instance.SetMouseLock(false);

                // 캐릭터 삭제 코드 작성!!! 네트워크에서 각자의 캐릭터를 삭제
                PhotonInit.PhotonInit.Instance.end_game();
                if (prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName));
                
                startedNextScene = true;
            }
        }
    }
}
