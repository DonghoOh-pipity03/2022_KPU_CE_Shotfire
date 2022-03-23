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

    [Header("무기 스펙")]
    [SerializeField] FireMode[] havingFireMode;   // 해당 무기가 가지는 발사모드
    public FireMode[] HavingFireMode => havingFireMode;
    [SerializeField] float fireRPM;   // 사격 RPM
    public float FireRPM => fireRPM;
    [Header("명중률")]
    [SerializeField] float maxSpread;   // 최대 스프레드
    public float MaxSpread => maxSpread;
    [SerializeField]  float recoilPerShot;   // 1회 사격 당 반동
    public float RecoilPerShot => recoilPerShot;
    [SerializeField]  float recoilRecover;   // 반동 회복 속도
    public float RecoilRecover => recoilRecover;
    [SerializeField] Vector2 recoilHorizontal;  // 기본 가로 반동
    public Vector2 RecoilHorizontal => recoilHorizontal;
    [SerializeField] Vector2 recoilVertical;  // 기본 세로 반동
    public Vector2 RecoilVertical => recoilVertical;
    [SerializeField] Vector2 recoilZ;  // 기본 Z 반동
    public Vector2 RecoilZ => recoilZ;
    [Header("반동")]
    [SerializeField] float recoilMultipleInIdle;  // idle 사격에서 화면 반동 계수
    public float RecoilMultipleInIdle => recoilMultipleInIdle;    
    
    [Header("무기 재장전")]
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
