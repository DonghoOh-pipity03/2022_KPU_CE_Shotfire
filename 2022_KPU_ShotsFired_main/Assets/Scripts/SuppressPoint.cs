using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppressPoint : MonoBehaviour
{
    private LivingEntity EntityHealth;

    private void Start() {
        EntityHealth = transform.root.gameObject.GetComponent<LivingEntity>();
    }

    // 공격체와 닿으면, 공격체는 이 메소드를 사용하여 데미지 메시지를 준다.
    public void ApplySuppress(float _suppressAmount)
    {
        EntityHealth.TakeSuppress(_suppressAmount);
    }
}
