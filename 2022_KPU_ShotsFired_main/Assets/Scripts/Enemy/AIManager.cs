using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class AIManager : MonoBehaviourPunCallbacks
{
    [Tooltip("배열에 등록할 프리팹은 반드시 Asset/Resource 폴더에 있어야 합니다.")]
    [SerializeField] GameObject[] enemies;  // 소한될 적 프리팹 배열
    [SerializeField] int[] respawnAmount;   // 리스폰 양 배열
    [SerializeField] float respawnTime; // 리스폰 주기
    float lastRespawnTime;  // 마지막 리스폰 시간
    
    public override void OnEnable() 
    {
        RespawnAI();
    }

    private void Update() 
    {
        if(Time.time > lastRespawnTime + respawnTime) RespawnAI();
    }

    private void RespawnAI()
    {
        lastRespawnTime = Time.time;
        // 적 스폰
        for(int i = 0 ; i < enemies.Length ; i++)
        {
            for(int j = 0 ; j < respawnAmount[i] ; j++)
            {
                var obj = PhotonNetwork.Instantiate(enemies[i].name , transform.position, transform.rotation);
                SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
            }
        }
    }
}
