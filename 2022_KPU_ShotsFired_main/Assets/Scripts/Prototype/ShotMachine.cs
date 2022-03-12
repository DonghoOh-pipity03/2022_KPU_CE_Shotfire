using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMachine : MonoBehaviour
{
    // 총알 발사기 

    [SerializeField] Weapon weapon;

    private void Start() {
        weapon = GetComponent<Weapon>();
    }

    private void Update() {
        weapon.Fire();
    }
}
