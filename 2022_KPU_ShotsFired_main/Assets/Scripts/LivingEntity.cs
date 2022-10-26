using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum EntityState{ alive, down, dead }   // 생명체의 상태
abstract class LivingEntity : MonoBehaviourPunCallbacks
{
    #region 전역 변수
    [Header("체력 파라미터")]
    [SerializeField] protected float maxHealth = 100; // 시작 및 최대 체력
    [SerializeField] protected float[] hitMultiple = {2f, 1f, 0.8f, 0.8f};  // 부위별 데미지 계수 / 머리,몸통,팔,다리
    [SerializeField] public Transform shotPoint; // AI가 조준을 하기 위한 위치를 저장한다. 플레이어만 사용한다.
    [Header("제압 파라미터")]
    [SerializeField] protected float maxSuppress = 100; // 최대 제압수치
    [SerializeField] protected float unsuppressAmount   = 7; // 초당 제압해제수치
    #endregion
    #region 전역 동작 변수
    protected float curHealth;    // 현재 체력
    protected float curSuppress;    // 현재 제압량
    [HideInInspector] public EntityState entityState;   // 생명체의 상태
    #endregion
    #region 콜백함수
    public override void OnEnable() 
    {   
        base.OnEnable();
        entityState = EntityState.alive;
        curHealth = maxHealth;   
    }

    protected virtual void Update() 
    {
        if(entityState != EntityState.dead) UnSuppress();
    }
    #endregion
    #region 함수
    public virtual void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        if(entityState == EntityState.dead) return;

        
        curHealth -= _damageMessage.damageAmount * hitMultiple[(int)_hitPart];

        // Debug.Log(photonView);
        

        if(curHealth <= 0) 
        {
            curHealth = 0;
            Die();
        }
    }
    public virtual void RestoreHealth(float _restoreAmount)
    {
        if(entityState == EntityState.dead) return;

        curHealth += _restoreAmount;
        if(curHealth > maxHealth) curHealth = maxHealth;
    }

    protected virtual void Die() {
        entityState = EntityState.dead;
    }

    public virtual void TakeSuppress(float _suppressAmount)
    {
        curSuppress += _suppressAmount;
        if(curSuppress > maxSuppress) curSuppress = maxSuppress;
    }
    // 제압 자동해제
    protected virtual void UnSuppress()
    {
        if(curSuppress > 0) curSuppress -= unsuppressAmount * Time.deltaTime;
        else if(curSuppress < 0) curSuppress =0;
    }
    #endregion

}
