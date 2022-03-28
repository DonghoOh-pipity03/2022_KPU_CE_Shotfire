using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class AIManager : MonoBehaviourPunCallbacks
{   
    [SerializeField] StageManager stageManager;
    [Tooltip("배열에 등록할 프리팹은 반드시 Asset/Resource 폴더에 있어야 합니다.")]
    [SerializeField] GameObject[] enemies;  // 소한될 적 프리팹 배열
    [SerializeField] int[] respawnAmount;   // 리스폰 양 배열
    [SerializeField] float respawnTime; // 리스폰 주기
    float lastRespawnTime;  // 마지막 리스폰 시간
    bool isUsed;    // 활성화 되었었는지 여부
    
    public override void OnEnable() 
    {
        if(isUsed) return;
        isUsed = true;
        stageManager = transform.root.Find("SafeRoom Trigger").GetComponent<StageManager>();    
        RespawnAI();
    }

    private void Update() 
    {
        if(isUsed) return;
        if(Time.time > lastRespawnTime + respawnTime) RespawnAI();
    }

    private void RespawnAI()
    {
        if( !PhotonNetwork.IsMasterClient ) return; // 이하 네트워크_방장 권한 구역

        lastRespawnTime = Time.time;
        // 적 스폰
        for(int i = 0 ; i < enemies.Length ; i++)
        {
            for(int j = 0 ; j < respawnAmount[i] ; j++)
            {
                var obj = PhotonNetwork.Instantiate(enemies[i].name , transform.position, transform.rotation);
                SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
                stageManager.AddAI(obj);
            }
        }
    }
}
