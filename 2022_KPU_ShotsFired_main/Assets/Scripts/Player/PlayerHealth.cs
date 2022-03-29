using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class PlayerHealth : LivingEntity
{
    private PlayerController playerController;
    #region 전역변수
    [Header("체력 자동회복 파라미터")]
    [SerializeField] private float restoreStartTime;    // 마지막 피격 후 자동 회복까지 걸리는 시간
    [SerializeField] private float autoRestoreAmount;   // 초당 체력 자동 회복량 
    [Header("다운 파라미터")]
    [SerializeField] private float downMaxHealth;   // 다운 되었을 때 초기, 최대 체력   
    #endregion
    #region 전역동작변수
    private float lastHitTime;  //마지막 피격 시간
    bool isRestoring;   // 자동회복 중인지 여부
    #endregion
    #region 콜백함수
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
    #endregion
    #region 함수
    public override void RestoreHealth(float _restoreAmount)
    {
        base.RestoreHealth(_restoreAmount);
        
        UpdateUI();
    }

     public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);
        
        lastHitTime = Time.time;

        if( !photonView.IsMine ) return;    // 네트워크 통제 구역
        
        if(state == EntityState.alive) UpdateUI();
        GameUIManager.Instance.SetActviePlayerDamaged(true);
    }

    protected override void Die()
    {
        if(state == EntityState.alive)   
        {
            base.state = EntityState.down;
            
            UpdateUI();
            curHealth = downMaxHealth;
            
            playerController.ableControlMove = false;   // 이동 불능
            GameManager.Instance.curLivingPlayer--; // 생존 플레이어 수 감소
        }
        else if( state == EntityState.down) 
        {
            base.Die();

            playerController.ableControlAttack = false;
        }
    }

    // 체력 자동회복
    private void AutoResore()
    {   
        // 회복 가능 조건일 때
        if( curHealth < maxHealth && Time.time >= lastHitTime + restoreStartTime && state == EntityState.alive)
        {   
            if(!isRestoring)    // 회복 시작 타이밍일 때
            {
                isRestoring = true;

                if( !photonView.IsMine ) return;    // 네트워크 통제 구역
                GameUIManager.Instance.SetActviePlayerDamaged(false);
            }
            
            curHealth += autoRestoreAmount * Time.deltaTime;
            if(curHealth > maxHealth) curHealth = maxHealth;

            if( !photonView.IsMine ) return;    // 네트워크 통제 구역
            UpdateUI();
        }
        else isRestoring = false;
    }

    private void UpdateUI()
    {   
        if( !photonView.IsMine ) return;    // 네트워크 통제 구역
        GameUIManager.Instance.UpdatePlayerHealth(( (int)curHealth), (int)maxHealth);
    }
    #endregion 
}