using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Tooltip("사용을 원하지 않으면 공백으로 두기")]
    [SerializeField] string nextSceneName;
    [SerializeField] string prevSceneName;
    private int curPlayerCountInRoom;   // 현재 세이프룸 인원
    private bool startedNextScene = false;  // 다음 씬의 로드가 시작되었는지 여부
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") curPlayerCountInRoom++;
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "Player") curPlayerCountInRoom--;
    }

    private void OnTriggerStay(Collider other) 
    {  
        if(!startedNextScene && other.tag == "Player") SetStage();
    }

    // 세이프룸 안의 플레이어수가 살아있는 플레이어 수와 같은지 검사하고, 
    // 다음 스테이지를 비동기 로딩 및 이전 스테이지를 비동기 언로딩한다.
    private void SetStage()
    {
        if(curPlayerCountInRoom == GameManager.Instance.curLivingPlayer) 
        {
            if(nextSceneName != "") StartCoroutine(GameSceneManager.Instance.AddSceneWithAsync(nextSceneName));
            if(prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName));
            startedNextScene = true;
        }
    }
}
