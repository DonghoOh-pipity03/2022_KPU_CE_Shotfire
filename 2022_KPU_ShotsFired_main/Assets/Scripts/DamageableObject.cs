using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableObject : MonoBehaviour, IDamageable
{
    [SerializeField] EffectKind effectKind;
    [SerializeField] bool setParent = false;    // 움직이는 물체인 경우, 데칼의 부모를 설정해주어야 함
    [SerializeField] AudioClip[] clip_hit;    // 피격음

    public void ApplyDamage(DamageMessage _damageMessage){
        if(_damageMessage.damageKind == DamageKind.bullet){
            EffectManager.Instance.PlayEffect(_damageMessage.hitPoint, _damageMessage.hitNormal, effectKind, setParent?transform:null);
            if(clip_hit.Length != 0) SoundManager.Instance.PlayLimitSFX( clip_hit[Random.Range(0, clip_hit.Length-1)], _damageMessage.hitPoint, name );
        }
    }
}
