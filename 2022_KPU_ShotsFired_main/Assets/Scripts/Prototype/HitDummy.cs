using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

class HitDummy : LivingEntity
{
    [SerializeField] private TextMeshProUGUI damageUI;
    [SerializeField] private TextMeshProUGUI suppressUI;

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override void Update() {
        base.Update();
        
        suppressUI.text = "suppress: " + base.curSuppress;
    }

    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);
        
        UpdateUI( _hitPart.ToString(), _damageMessage.damageAmount * hitMultiple[(int)_hitPart] );
    }

    private void UpdateUI(string _hitPoint, float _damage) 
    {
        damageUI.text = "hit: " + _hitPoint + '\n' 
                        + "damage: " + _damage + '\n';
    }
}
