using UnityEngine;

public struct DamageMessage
{
    public enum DamageKind{ bullet, explosion, fire }

    public GameObject attacker;
    public float amount;
    public DamageKind damageKind;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}
