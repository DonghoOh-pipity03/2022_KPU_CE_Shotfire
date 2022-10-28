/*
 AIManager.cs
1. AI 리스폰: AiTrigger.cs의해 활성/비활성화되며, 일정 간경을 주기로 AI를 생성한다. 
이때 StageManager.cs의 최대 AI 수를 참조하여, AI의 최대 수를 넘지 않는다. 
 */
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class AISpawner : MonoBehaviourPunCallbacks
{
    #region 전역변수
    [SerializeField] StageManager stageManager;
    [Tooltip("배열에 등록할 프리팹은 반드시 Asset/Resource 폴더에 있어야 합니다.")]
    [SerializeField] GameObject[] enemies;  // 소한될 적 프리팹 배열
    [SerializeField] int[] respawnAmount;   // 리스폰 양 배열
    [SerializeField] float respawnTime; // 리스폰 주기
    [SerializeField] int maxEnemyCount; // 이 숫자 이상으로 적이 스테이지 내에 생존해 있다면, 리스폰시키지 않는다.
    [SerializeField] bool developMode = false;    // 개발모드여부
    #endregion

    #region 전역동작변수
    float lastRespawnTime;  // 마지막 리스폰 시간
    bool isUsed;    // 활성화 되었었는지 여부
    #endregion

    #region 콜백함수
    public override void OnEnable()
    {
        if (isUsed) return;
        if (!developMode) stageManager = transform.root.Find("SafeRoom Trigger").GetComponent<StageManager>();
        RespawnAI();
    }

    public override void OnDisable()
    {
        isUsed = true;
    }

    private void Update()
    {
        if (isUsed) return;
        if (Time.time > lastRespawnTime + respawnTime) RespawnAI();

    }
    #endregion

    #region 함수
    private void RespawnAI()
    {
        if (stageManager.GetAliveEnemyCount() >= maxEnemyCount) return;

        // 개발모드
        if (developMode)
        {
            lastRespawnTime = Time.time;
            for (int i = 0; i < enemies.Length; i++)
            {
                for (int j = 0; j < respawnAmount[i]; j++)
                {
                    var obj = Instantiate(enemies[i], transform.position, transform.rotation);
                    Debug.Log(gameObject.scene);
                    SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
                    obj.GetComponent<EnemyAgent>().SetAlert();
                    obj.GetComponent<EnemyAgent>().stageManager = stageManager;
                    stageManager.AddAI(obj);
                }
            }
        }

        // 일반모드
        else
        {
            if (!PhotonNetwork.IsMasterClient) return; // 이하 네트워크_방장 권한 구역

            lastRespawnTime = Time.time;
            // 적 스폰
            for (int i = 0; i < enemies.Length; i++)
            {
                for (int j = 0; j < respawnAmount[i]; j++)
                {
                    var obj = PhotonNetwork.Instantiate(enemies[i].name, transform.position, transform.rotation);
                    SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
                    obj.GetComponent<EnemyAgent>().SetAlert();
                    obj.GetComponent<EnemyAgent>().stageManager = stageManager;
                    stageManager.AddAI(obj);
                }
            }
        }
    }
    #endregion
}
