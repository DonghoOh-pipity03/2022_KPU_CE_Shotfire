using UnityEngine;

public enum DamageKind{ bullet, explosion, fire, gas, GM }   // 데미지의 종류

public struct DamageMessage
{
    public GameObject attacker; // 공격자
    public int ID;    // 공격체 식별을 위한 난수 저장공간
    public float damageAmount;  // 데미지 양
    public float suppressAmount;    // 제압량
    public DamageKind damageKind;   // 데미지 종류
    public Vector3 hitPoint;    // 충돌 위치
    public Vector3 hitNormal;   // 충돌 노멀
}
