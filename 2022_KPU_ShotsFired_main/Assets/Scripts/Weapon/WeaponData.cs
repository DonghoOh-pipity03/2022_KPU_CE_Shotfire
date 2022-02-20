using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { automatic, burst, twice, single }   // 발사모드

[CreateAssetMenu(fileName = "Weapon Data", menuName ="Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ScriptableObject
{
    [SerializeField]private string weaponName;  // 무기 이름
    public string WeaponName{ get{return weaponName;} set{}}

        // 무기 인게임 UI 아이콘

    [Header("무기 스펙 정보")]
    [SerializeField]private FireMode[] havingFireMode;   // 해당 무기가 가지는 발사모드
    public FireMode[] HavingFireMode{ get{return havingFireMode;} set{}}
        // 사격 반동
    
    [SerializeField]private float fireRPM;   // 사격 RPM
    public float FireRPM{ get{ return fireRPM;} set{}}
    [SerializeField] private float moa; // 명중률 MOA
    public float MOA{ get{return moa;} set{}}
    [SerializeField] private float recoilPerShot;   // 1회 사격 당 반동
    public float RecoilPerShot{ get{return recoilPerShot;} set{}}
    [SerializeField] private float recoilRecoverTime;   // 반동 회복 속도
    public float RecoilRecoverTime{ get{ return recoilRecoverTime;} set{}}
 
    [Header("무기 재장전 정보")]
    [SerializeField]private int magCappacity; // 탄창 용량
    public int MagCappacity{ get{return magCappacity;} set{}}
    [SerializeField]private int initMagCount; // 초기 지급 탄창 수
    public int InitMagCount{ get{return initMagCount;} set{}}
    [SerializeField]private float reloadTime; // 재장전 시간
    public float ReloadTime{get{return reloadTime;} set{}}

        // 총격 소리
        // 총격 이펙트
        // 재장전 소리

        // 탄피 프리팹
        // 탄피 배출 힘

}
