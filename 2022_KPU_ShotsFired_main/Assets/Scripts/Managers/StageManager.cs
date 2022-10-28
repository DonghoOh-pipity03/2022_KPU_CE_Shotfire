/*
 StageManager.cs
1. 다음 씬의 비동기 로딩: '세이프룸'에 모든 플레이어가 있으면, 일반적으로 다음 씬을 로딩하고 이전 씬을 삭제한다.
2. 관리 대상 AI 추가: AI 오브젝트를 입력받아, 자료구조에 추가한다.
3. 관리 대상 AI 모두 삭제: 관리 대상인 AI를 모두 삭제한다.
4. 살아있는 AI 카운팅: 살아있는 AI의 수를 세어, 반환한다.
 */
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class StageManager : MonoBehaviour
{
    
    #region 전역 변수
    [Tooltip("다음 스테이지 씬 이름, 사용을 원하지 않으면 공백으로 두기")]
    [SerializeField] private string nextSceneName;
    [Tooltip("이전 스테이지 씬 이름, 사용을 원하지 않으면 공백으로 두기")]
    [SerializeField] private string prevSceneName;
    [Tooltip("이전 씬을 삭제할지 여부")]
    [SerializeField] bool deletePreScene = true;
    [Tooltip("이전 스테이지로의 방향을 막는 문 오브젝트")]
    [SerializeField] Door[] door;
    
    [Header("마지막 스테이지 전용 세팅")]
    [Tooltip("게임이 최종적으로 끝나는 스테이지에서 사용할 것인지 여부")]
    [SerializeField] bool useForLastStage;

    [Header("적 정보")]
    [Tooltip("해당 스테이지에 생성되어있는 적들의 게임오브젝트 리스트")]
    [SerializeField] List<GameObject> enemies;

    [Header("BGM")]
    [Tooltip("배경음")]
    [SerializeField] EnvSFX bgm;
    #endregion

    #region 전역 동작 변수
    private int curPlayerCountInRoom;   // 현재 세이프룸 인원
    private bool startedNextScene = false;  // 다음 씬의 로드가 시작되었는지 여부
    List<bool> aliveEnemy; // enemies 적들 중 생사여부
    [HideInInspector] public int aliveEnemyCount;    // 살아있는 적들 수
    #endregion

    #region 콜백함수
    private void OnEnable() {
        aliveEnemy = new List<bool>( new bool[enemies.Count]);
        for(int i = 0 ; i < aliveEnemy.Count; i++){
            aliveEnemy[i] = true;
        }
        aliveEnemyCount = aliveEnemy.Count;
        //var doorObject = this.transform.root.Find("Door");
        //if(doorObject != null) door = doorObject.GetComponent<Door>();
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
    #endregion

    #region 함수
    // 세이프룸 안의 플레이어수가 살아있는 플레이어 수와 같은지 검사하고, 
    // 다음 스테이지를 비동기 로딩 및 이전 스테이지를 비동기 언로딩한다.
    private void SetStage()
    {
        if (curPlayerCountInRoom == GameManager.Instance.curLivingPlayer)
        {
            if (!useForLastStage)    // 일반 스테이지일 때
            {
                if(door != null) {
                    foreach(var i in door) i.SetDoor();
                }
                if (nextSceneName != "") StartCoroutine(GameSceneManager.Instance.AddSceneWithAsync(nextSceneName));    // 다음 씬로딩
                if (prevSceneName != "") GameObject.Find(prevSceneName).transform.Find("SafeRoom Trigger").GetComponent<StageManager>().DeleteAllAI(); // 이전 씬의 AI 삭제
                if (prevSceneName != "" && deletePreScene) StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName)); // 이전 씬삭제
            }
            else    // 마지막 스테이지일 때
            {
               GameManager.Instance.EndGame(true);

                if (prevSceneName != "") StartCoroutine(GameSceneManager.Instance.UnloadSceneWithAsync(prevSceneName));
            }

            startedNextScene = true;
            SoundManager.Instance.SetEnv(bgm);
            
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
    #endregion
}
