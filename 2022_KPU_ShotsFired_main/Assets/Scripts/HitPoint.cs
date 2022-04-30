using UnityEngine;

public enum HitParts{ head, upBody, arm, leg}   // 피격 부위 종류
public class HitPoint : MonoBehaviour, IDamageable
{
    LivingEntity EntityHealth;  // 데미지를 전달할 상위 LivingEntity
    [SerializeField] HitParts hitPart;  // 피격 부위

    private void Start(){
        EntityHealth = transform.root.gameObject.GetComponent<LivingEntity>();
        this.gameObject.layer = transform.root.gameObject.layer;
        // 1 << LayerMask.NameToLayer("3dObjectLayer");
    }

    // 공격체와 닿으면, 공격체는 이 메소드를 사용하여 데미지 메시지를 준다.
    public void ApplyDamage(DamageMessage _damageMessage) => EntityHealth.TakeDamage(_damageMessage, hitPart);
}
