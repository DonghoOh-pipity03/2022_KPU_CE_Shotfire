using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerHealth : LivingEntity
{
    
    protected override void OnEnable() 
    {
        base.OnEnable();    

        UpdateUI();
    }

    public override void RestoreHealth(float _restoreAmount)
    {
        base.RestoreHealth(_restoreAmount);
        
        UpdateUI();
    }
    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);
        
        UpdateUI();
    }
    protected override void Die()
    {
        base.Die();
    }

    private void UpdateUI()
    {
        GameUIManager.Instance.UpdatePlayerHealth(curHealth, MaxHealth);
    }
}
