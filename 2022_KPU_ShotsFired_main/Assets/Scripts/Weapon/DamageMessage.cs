using UnityEngine;

public struct DamageMessage
{
    public enum DamageKind{ bullet, explosion, fire }

    public GameObject attacker; // 공격자
    public int ID;    // 공격체 식별을 위한 난수 저장공간
    public float amount;
    public DamageKind damageKind;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}
