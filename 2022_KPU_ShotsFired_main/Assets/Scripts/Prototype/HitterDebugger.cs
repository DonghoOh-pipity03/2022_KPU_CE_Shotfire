using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

class HitterDebugger : LivingEntity
{
    private enum DebugKind{ Hit, Suppress}
    [SerializeField] private DebugKind debugKind;
    [SerializeField] private TextMeshProUGUI Display;   // hit UI
    public int hitCount;    // 피격 횟수
    public float lastHitTime;   // 마지막 피격 시간
    public int supCount;    // 제압 횟수
    public float lastSupTime;   // 마지막 제압 시간

    protected override void Update() {
        base.Update();
        UpdateUI();
    }

    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);
        
        hitCount++;
        lastHitTime = Time.time;
    }

    public override void TakeSuppress(float _suppressAmount)
    {
        base.TakeSuppress(_suppressAmount);

        supCount++;
        lastSupTime= Time.time;
    }
    

    public void UpdateUI()
    {
        if(debugKind == DebugKind.Hit) Display.text = "Hit Count: " + hitCount + '\n' + "Last Hit Time: " + lastHitTime;
        else Display.text = "Sup Count: " + supCount + '\n' + "Last Sup Time: " + lastSupTime;
    }

}
