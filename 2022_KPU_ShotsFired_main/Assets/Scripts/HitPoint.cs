using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitParts{ head, upBody, arm, leg}
public class HitPoint : MonoBehaviour, IDamageable
{
    private LivingEntity EntityHealth;

    [SerializeField] HitParts hitPart;

    private void Start() {
        EntityHealth = transform.root.gameObject.GetComponent<LivingEntity>();
    }

    // 공격체와 닿으면, 공격체는 이 메소드를 사용하여 데미지 메시지를 준다.
    public void ApplyDamage(DamageMessage _damageMessage)
    {
        EntityHealth.TakeDamage(_damageMessage, hitPart);
    }

}
