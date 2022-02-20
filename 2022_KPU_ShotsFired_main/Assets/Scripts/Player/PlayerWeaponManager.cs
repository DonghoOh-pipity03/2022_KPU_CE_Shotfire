using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
   //private IWeapon[] weaponArray = new IWeapon[3];
   [SerializeField] Camera playerCamera;
   [SerializeField] Weapon weapon;

   #region 전역 변수
   #endregion

   #region 전역 동작 변수
   #endregion
   
   private void Update() {  
        if(Input.GetButton("Fire1")) weapon.Fire();
        else weapon.Detached();
        
        if(Input.GetButtonDown("Reload")) weapon.Reload();
        if(Input.GetButtonDown("Firemode")) weapon.ChangeFireMode();
        if(Input.GetButtonDown("Fire2")) weapon.SetFireState(false);
        else if(Input.GetButtonUp("Fire2")) weapon.SetFireState(true);
    }

}
