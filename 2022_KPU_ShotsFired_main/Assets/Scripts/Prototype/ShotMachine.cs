using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMachine : MonoBehaviour
{
    public bool shot = false;
    
    // 총알 발사기 
    [SerializeField] Weapon weapon;

    private void Start() {
        weapon = GetComponent<Weapon>();
    }

    private void Update() {
        if(shot) weapon.Fire(0);
    }
}
