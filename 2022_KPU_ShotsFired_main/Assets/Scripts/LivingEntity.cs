using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract class LivingEntity : MonoBehaviour
{
    #region 전역 변수
    [SerializeField] protected float MaxHealth = 100; // 시작 및 최대 체력
    [SerializeField] protected float MaxSuppress = 100; // 최대 제압수치
    [SerializeField] protected float UnsuppressAmount   = 7; // 초당 제압해제수치
    [SerializeField] protected float[] HitMultiple = {2f, 1f, 0.8f, 0.8f};  // 부위별 데미지 계수 / 머리,몸통,팔,다리
    #endregion
    #region 전역 동작 변수
    protected float curHealth;    // 현재 체력
    protected float curSuppress = 0;    // 현재 제압량
    public bool isDead;    // 사망여부
    #endregion

    protected virtual void OnEnable() 
    {
        isDead = false;
        curHealth = MaxHealth;   
    }

    protected virtual void Update() 
    {
        if(curSuppress > 0)
        {
            curSuppress -= UnsuppressAmount * Time.deltaTime;
        }
        else if(curSuppress < 0) curSuppress =0;
    }

    public virtual void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        if(isDead) return;

        curHealth -= _damageMessage.damageAmount * HitMultiple[(int)_hitPart];

        if(curHealth <= 0) Die();
    }

    public virtual void TakeSuppress(DamageMessage _damageMessage)
    {
        curSuppress += _damageMessage.suppressAmount;
    }

    public virtual void RestoreHealth(float _restoreAmount)
    {
        if(isDead) return;

        curHealth += _restoreAmount;
    }

    protected virtual void Die() {
        isDead = true;
    }
}
