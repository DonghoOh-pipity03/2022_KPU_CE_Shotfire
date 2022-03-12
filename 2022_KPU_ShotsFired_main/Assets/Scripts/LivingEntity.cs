using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityState{ alive, down, dead }   // 생명체의 상태
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
    public EntityState state;   // 생명체의 상태
    #endregion

    protected virtual void OnEnable() 
    {
        state = EntityState.alive;
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
        if(state == EntityState.dead) return;

        curHealth -= _damageMessage.damageAmount * HitMultiple[(int)_hitPart];

        if(curHealth <= 0) 
        {
            curHealth = 0;
            Die();
        }
    }

    public virtual void TakeSuppress(float _suppressAmount)
    {
        curSuppress += _suppressAmount;
    }

    public virtual void RestoreHealth(float _restoreAmount)
    {
        if(state == EntityState.dead) return;

        curHealth += _restoreAmount;
    }

    protected virtual void Die() {
        state = EntityState.dead;
    }
}
