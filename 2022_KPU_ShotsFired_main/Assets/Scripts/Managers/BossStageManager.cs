using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStageManager : MonoBehaviour
{
    [SerializeField] EnemyData bossInfo;
    [SerializeField] BossAgent boss;
    [SerializeField] GameObject phase2Event;
    [SerializeField] GameObject endStageManager;
    bool isUsed = false;
    bool isActivePhase2 = false;

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Player" && isUsed == false){
            GameUIManager.Instance.SetActiveBoss(true);
            GameUIManager.Instance.UpdateBossUI(bossInfo.name);
        }
    }

    private void OnEnable() {
        StartCoroutine(CheckBoss());
    }

    IEnumerator CheckBoss(){
        yield return null;
        while(true){
            // 보스 체력 50이하
            if( boss.GetHealth() <= bossInfo.MaxHealth / 2 && !isActivePhase2){
                isActivePhase2 = true;
                phase2Event.SetActive(true);
            }

            // 보스 사망
            else if( boss.aiState==AIState.dead ){
                Time.timeScale = 0.1f;
                foreach(var i in GameManager.Instance.players){ 
                    if(i == null) continue;

                    var cc = i.GetComponent<PlayerController>();
                    cc.ableControlAttack = false;
                    cc.ableControlMove = false;
                    }

                yield return new WaitForSecondsRealtime(5f);

                Time.timeScale = 1f;
                GameUIManager.Instance.SetActiveBoss(false);


                yield return new WaitForSecondsRealtime(3f);
                endStageManager.SetActive(true);

                break;
            }
            else{
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
