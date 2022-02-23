using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
   //private IWeapon[] weaponArray = new IWeapon[3];
   [SerializeField] Camera playerCamera;
   [SerializeField] Weapon weapon;

   #region 전역 변수
   [SerializeField] Weapon[] weaponArray = new Weapon[2];   // 1~4번 슬릇에 사용할 무기 배열
   #endregion

   #region 전역 동작 변수
   private int curWeapon = 0;
   #endregion
   
    private void Start() {
        foreach(Weapon weapon in weaponArray)
        {
            weapon.gameObject.SetActive(false);
        }
        weaponArray[curWeapon].gameObject.SetActive(true);
    }

    // 개발용
   private void Update() {  
        if(Input.GetButton("Fire1")) weaponArray[curWeapon].Fire();
        else weaponArray[curWeapon].Detached();
        
        if(Input.GetButtonDown("Reload")) weaponArray[curWeapon].Reload();
        if(Input.GetButtonDown("Firemode")) weaponArray[curWeapon].ChangeFireMode();
        if(Input.GetButtonDown("Fire2")) weaponArray[curWeapon].SetFireState(false);
        else if(Input.GetButtonUp("Fire2")) weaponArray[curWeapon].SetFireState(true);

        ChangeWeaponCommand();
    }

    private void ChangeWeaponCommand()
    {
        if(Input.GetButtonDown("Weapon1")) ChangeWeapon(0);
        else if(Input.GetButtonDown("Weapon2")) ChangeWeapon(1);
        else if(Input.GetButtonDown("Weapon3")) ChangeWeapon(2);
        else if(Input.GetButtonDown("Weapon4")) ChangeWeapon(3);
    }
    private void ChangeWeapon(int _newWeaponIndex)
    {
        weaponArray[curWeapon].gameObject.SetActive(false);
        curWeapon = _newWeaponIndex;
        weaponArray[curWeapon].gameObject.SetActive(true);

        weaponArray[curWeapon].UpdateUI();
    }
}
