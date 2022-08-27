using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    [SerializeField] float damage = 1;
    [SerializeField] float damageGapTime = 0.5f;
    Dictionary<int, float> playerDictionary = new Dictionary<int, float>();    // 플레이어 오브젝트 고유번호와 마지막 타격시간을 저장

    private void OnTriggerStay(Collider other) {
        var playerHealth = other.GetComponent<PlayerHealth>();
        if(playerHealth != null){
            Debug.Log("1");
            // 이미 데미지를 준 캐릭터인 경우
            var playerID = other.GetInstanceID();
            if( playerDictionary.ContainsKey(playerID).Equals(true) ){
                // 데미지를 줄 수 있는 시간인 경우
                if( Time.time >= playerDictionary[playerID] + damageGapTime){
                    DamageMessage damageMessage;

                    damageMessage.attacker = this.gameObject;
                    damageMessage.ID = transform.GetInstanceID(); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
                    damageMessage.damageKind = DamageKind.gas;
                    damageMessage.damageAmount = damage;
                    damageMessage.suppressAmount = 0;
                    damageMessage.hitPoint = Vector3.zero;
                    damageMessage.hitNormal =  Vector3.zero;

                    playerHealth.TakeDamage(damageMessage, HitParts.upBody);

                    playerDictionary[playerID] = Time.time;
                }
            }
            // 새로 데미지를 줄 캐릭터인 경우
            else{
                    DamageMessage damageMessage;

                    damageMessage.attacker = this.gameObject;
                    damageMessage.ID = transform.GetInstanceID(); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
                    damageMessage.damageKind = DamageKind.gas;
                    damageMessage.damageAmount = damage;
                    damageMessage.suppressAmount = 0;
                    damageMessage.hitPoint = Vector3.zero;
                    damageMessage.hitNormal =  Vector3.zero;

                    playerHealth.TakeDamage(damageMessage, HitParts.upBody);

                    playerDictionary[playerID] = Time.time;
            }

        }    
    }
}
