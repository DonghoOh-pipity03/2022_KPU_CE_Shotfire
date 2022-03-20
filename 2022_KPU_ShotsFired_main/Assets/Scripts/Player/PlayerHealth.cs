using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class PlayerHealth : LivingEntity
{
    private PlayerController playerController;
#region 전역변수
    [SerializeField] private float restoreStartTime;    // 마지막 피격 후 자동 회복까지 걸리는 시간
    [SerializeField] private float autoRestoreAmount;   // 초당 체력 자동 회복량 
    [SerializeField] private float downMaxHealth;   // 다운 되었을 때 초기, 최대 체력
#endregion
#region 전역동작변수
    private float lastHitTime;  //마지막 피격 시간
#endregion

    private void Start() 
    {
      playerController = GetComponent<PlayerController>();  
    }
    public override void OnEnable() 
    {
        base.OnEnable();    

        UpdateUI();
    }

    private void FixedUpdate() 
    {
        AutoResore();
    }

    public override void RestoreHealth(float _restoreAmount)
    {
        base.RestoreHealth(_restoreAmount);
        
        UpdateUI();
    }
    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);
        
        lastHitTime = Time.time;

        if(state == EntityState.alive) UpdateUI();
    }
    protected override void Die()
    {
        if(state == EntityState.alive)   
        {
            base.state = EntityState.down;
            
            UpdateUI();
            curHealth = downMaxHealth;
            
            playerController.ableControlMove = false;
        }
        else if( state == EntityState.down) 
        {
            base.Die();

            playerController.ableControlAttack = false;
        }
    }

    private void AutoResore()
    {
        if( curHealth < MaxHealth && Time.time >= lastHitTime + restoreStartTime && state == EntityState.alive)
        {   
            curHealth += autoRestoreAmount * Time.deltaTime;
            if(curHealth > MaxHealth) curHealth = MaxHealth;

            UpdateUI();
        }
    }

    private void UpdateUI()
    {   
        if( !photonView.IsMine ) return;
        GameUIManager.Instance.UpdatePlayerHealth(( (int)curHealth), (int)MaxHealth);
    }
}
