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
    [SerializeField] int ballPerOneShot;    // 1회 발사당 샷건 발사 개수, 일반 총기는 1로 지정
    public int BallPerOneShot => ballPerOneShot;
    [Header("명중률")]
    [SerializeField] float maxSpread;   // 총기의 최대 스프레드
    public float MaxSpread => maxSpread;
    [SerializeField] float minSpread;   // 총기의 최대 스프레드
    public float MinSpread => minSpread;
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
    [SerializeField] float recoilMultiple = 1;  // idle 상태에서 랜덤스프래드 계수
    public float RecoilMultiple => recoilMultiple;
    [Header("화면반동")]
    [SerializeField] float recoilMultipleInIdle;  // idle 사격에서 화면 반동 계수
    public float RecoilMultipleInIdle => recoilMultipleInIdle;   
    [SerializeField] float recoilMultipleInZoom;  // zoom 사격에서 화면 반동 계수
    public float RecoilMultipleInZoom => recoilMultipleInZoom; 
    
    [Header("무기 재장전")]
    [SerializeField] int magCappacity; // 탄창 용량
    public int MagCappacity => magCappacity;
    [SerializeField] int initMagCount; // 초기 지급 탄창 수
    public int InitMagCount => initMagCount;
    [SerializeField] float reloadTime; // 재장전 시간
    public float ReloadTime => reloadTime;
    [SerializeField] bool useChamber;   // 재장전 시, 약실에 1발 남는 디테일을 구현하는지 여부
    public bool UseChamber => useChamber;
    [SerializeField] bool useMag = true;    // 탄창방식의 재장전을 사용하는지, 샷건의 경우 false
    public bool UseMag => useMag;

        // 총격 소리
        // 총격 이펙트
        // 재장전 소리

        // 탄피 프리팹
        // 탄피 배출 힘

}
