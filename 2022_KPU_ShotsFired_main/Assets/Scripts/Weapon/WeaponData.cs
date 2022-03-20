using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { automatic, burst, twice, single }   // 발사모드

[CreateAssetMenu(fileName = "Weapon Data", menuName ="Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ScriptableObject
{
    [SerializeField] string weaponName;  // 무기 이름
    public string WeaponName => weaponName;

        // 무기 인게임 UI 아이콘

    [Header("무기 스펙 정보")]
    [SerializeField] FireMode[] havingFireMode;   // 해당 무기가 가지는 발사모드
    public FireMode[] HavingFireMode => havingFireMode;
        // 사격 반동
    
    [SerializeField] float fireRPM;   // 사격 RPM
    public float FireRPM => fireRPM;
    [SerializeField]  float moa; // 명중률 MOA
    public float MOA => moa;
    [SerializeField]  float recoilPerShot;   // 1회 사격 당 반동
    public float RecoilPerShot => recoilPerShot;
    [SerializeField]  float recoilRecoverTime;   // 반동 회복 속도
    public float RecoilRecoverTime => recoilRecoverTime;
    
    [Header("무기 재장전 정보")]
    [SerializeField] int magCappacity; // 탄창 용량
    public int MagCappacity => magCappacity;
    [SerializeField] int initMagCount; // 초기 지급 탄창 수
    public int InitMagCount => initMagCount;
    [SerializeField] float reloadTime; // 재장전 시간
    public float ReloadTime => reloadTime;

        // 총격 소리
        // 총격 이펙트
        // 재장전 소리

        // 탄피 프리팹
        // 탄피 배출 힘

}
