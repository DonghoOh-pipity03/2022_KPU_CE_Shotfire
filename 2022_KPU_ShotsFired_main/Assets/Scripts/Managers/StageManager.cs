using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class StageManager : MonoBehaviour
{
    
    #region 전역 변수
    [Tooltip("사용을 원하지 않으면 공백으로 두기")]
    [SerializeField] private string nextSceneName;  // 다음 스테이지 씬 이름
    [SerializeField] private string prevSceneName;  // 이전 스테이지 씬 이름
    [SerializeField] Door door; // 이전 스테이지로의 방향을 막는 문 오브젝트
    [Header("마지막 스테이지 전용 세팅")]

    [SerializeField] bool useForLastStage;  // 게임이 최종적으로 끝나는 스테이지에서 사용할 것인지
    [Header("적 정보")]
    [SerializeField] List<GameObject> enemies;   // 해당 스테이지에 생성되어있는 적들의 게임오브젝트 리스트
    #endregion
    #region 전역 동작 변수
    private int curPlayerCountInRoom;   // 현재 세이프룸 인원
    private bool startedNextScene = false;  // 다음 씬의 로드가 시작되었는지 여부
    List<bool> aliveEnemy; // enemies 적들 중 생사여부
    public int aliveEnemyCount;    // 살아있는 적들 수
    #endregion

    private void OnEnable() {
        aliveEnemy = new List<bool>( new bool[enemies.Count]);
        for(int i = 0 ; i < aliveEnemy.Count; i++){
            aliveEnemy[i] = true;
        }
        aliveEnemyCount = aliveEnemy.Count;
        var doorObject = this.transform.root.Find("Door");
        if(doorObject != null) door = doorObject.GetComponent<Door>();
    }
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
                if(door != null) door.SetDoor();
                if (nextSceneName != "") StartCoroutine(GameSceneManager.Instance.AddSceneWithAsync(nextSceneName));    // 다음 씬로딩
                if (prevSceneName != "") GameObject.Find(prevSceneName).transform.Find("SafeRoom Trigger").GetComponent<StageManager>().DeleteAllAI(); // 이전 씬의 AI 삭제
                if (prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName)); // 이전 씬삭제
                startedNextScene = true;
            }
            else    // 마지막 스테이지일 때
            {
               GameManager.Instance.EndGame(true);

                if (prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName));
                
                startedNextScene = true;
            }
        }
    }

    public void AddAI(GameObject _AI)
    {
        enemies.Add(_AI);
        aliveEnemy.Add(true);
        aliveEnemyCount++;
    }

    public void DeleteAllAI()
    {
        foreach( var i in enemies)
        {
            if( i != null) PhotonNetwork.Destroy(i);
        }
    }

    public int GetAliveEnemyCount(){ 
        
        aliveEnemyCount=0;

        for(int i = 0; i < aliveEnemy.Count; i++ ){
            aliveEnemy[i] = enemies[i].GetComponent<EnemyAgent>().entityState == EntityState.dead ? false : true;
            if(aliveEnemy[i] == true) aliveEnemyCount++;
        }

        return aliveEnemyCount;
    }
}
