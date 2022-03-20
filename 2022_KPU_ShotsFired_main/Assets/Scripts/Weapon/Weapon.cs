using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    [SerializeField] WeaponData weaponData; // 총기 SO
    [SerializeField] Bullet bulletPrefab;  // 총알 프리팹
    [SerializeField] Transform muzzlePosition; // 총구 위치

    #region 전역 변수
    [SerializeField] bool useUI = false;    // 무기 관련 UI를 사용하는지 여부
    [SerializeField] float moaMultiple = 4f;    // 실제 총기 명중률의 계수
    // 총기 정보
    GameObject weaponUser; // 무기 사용자 오브젝트
    bool autoReload = true;    // 자동 재장전 정책 허용여부
    // 총기 스펙
    FireMode[] havingFireMode;   // 해당 무기가 가지는 발사모드
    float fireRPM;  // 발사 속도(1rpm = 1round per 1minute)(실제스펙)
    float fireInterval => (60 / fireRPM); // 발사 속도에 따른 발사 간격
    float MOA; // 총기 명중률 (1moa = 1inch per 100yard)(실제스펙)
    
    [Header("명중률")]  // 개발 중인 영역
    [SerializeField] private float maxSpreadStandard; // 최대 스프레드
    [SerializeField] private float recoilPerShot;   // 발사 당 증가 스프레드
    [SerializeField] private float recoilRecoverTime;   // 반동 회복 속도

    // 총알
    int magCappacity = 5; // 탄창 용량
    #endregion

    #region 전역 동작 변수
    ObjectPool<Bullet> bulletPool;  // 총알 오브젝트 풀    
    // 총기 상태
    enum State{ready, empty, reloading, shooting}
    State state;    // 현재 상태
    int curFireMode = 0;    // 현재 발사모드
    bool isTriggered = false;   // 트리거가 눌려져 있는지
    bool isHipFire = true;  // 기본 사격 상태인지
    float lastFireTime = -1; // 마지막 발사 시간
    // 사격
    Vector3 fireDirection; // 의도하는 사격 방향
    float curSpread;    // 현재 스프레드
    float curSpreadVelocity; 
    float xVar; // x스프레드 값
    float yVar; // y스프레드 값
    // 총알
    int curRemainAmmo;    // 현재 탄창에 남은 총알 수
    int curRemainMag = 5;   // 현재 남은 탄창 수
    float reloadTime = 3;  // 재장전 시간
    #endregion

#region 콜백 함수
    public override void OnEnable() {
        base.OnEnable();
        // 재장전 중에 무기를 집어넣었다가 다시 꺼낼 때, 재장전 중이였다면 자동 재장전을 시도한다.
        if( state == State.reloading) Reload(); 
        
        // 발사 중 무기를 집어넣었다가 다시 꺼낼 때, 현재 무기 상태를 다시 점검한다.
        if((Time.time >= lastFireTime + fireInterval) && (curRemainAmmo > 0)) state = State.ready;
    }
    private void Start()
    {
        SettingData();
        weaponUser = transform.root.gameObject;
        curRemainAmmo = magCappacity;
        maxSpreadStandard = MOA * moaMultiple;

        UpdateUI();

        #region 오브젝트 풀링
        bulletPool = new ObjectPool<Bullet>
        (
            createFunc: ()=>    // 오브젝트를 생성할 때(풀이 부족할 때)
            {
                var newBullet = Instantiate(bulletPrefab);
                newBullet.poolToReturn = bulletPool;
                return newBullet;
            },
            actionOnGet: (bullet)=>   // 풀에서 오브젝트를 꺼낼 때
            {
                bullet.gameObject.SetActive(true);
                bullet.Reset(weaponUser, transform.position, fireDirection);
            },
            actionOnRelease: (bullet)=>   // 오브젝트를 다시 풀에 넣을 때
            {
                bullet.gameObject.SetActive(false);
            },
            actionOnDestroy: (bullet)=>    // 풀이 가득차서 오브젝트가 들어갈 공간이 없을 때
            {
                Destroy(bullet.gameObject);
            },
            maxSize: magCappacity
        );
        #endregion
    }

    private void Update() {
        curSpread = Mathf.SmoothDamp(curSpread, 0, ref curSpreadVelocity, recoilRecoverTime);
        curSpread = Mathf.Clamp(curSpread, 0, maxSpreadStandard);
    }
#endregion
    #region 사격
    // 1티어 사격 메소드
    public void Fire()
    {      
        if(state != State.ready) return;

        switch( havingFireMode[curFireMode] ){
            case FireMode.automatic: 
            state = State.shooting;
            StartCoroutine( Shots(1) );
            break;

            case FireMode.burst: 
            if(isTriggered == true) return;
            state = State.shooting;
            StartCoroutine( Shots(3) );
            break;

            case FireMode.twice: 
            if(isTriggered == true) return;
            state = State.shooting;
            StartCoroutine( Shots(2) );
            break;
            
            case FireMode.single: 
            if(isTriggered == true) return;
            state = State.shooting;
            StartCoroutine( Shots(1) );
            break;
        }

        isTriggered = true;
    }
    
    // 2티어 사격 메소드: 사격 통제 장치
    IEnumerator Shots(int _remainShotCount)
    {   
        // 자동사격이 끝났거나, 총알이 없는 경우
        if( _remainShotCount <= 0 || curRemainAmmo <= 0) 
        {
            AmmoCheck();
            yield break;
        }
        
        if(Time.time >= lastFireTime + fireInterval)
        {
            Shot();

            yield return new WaitForSeconds(fireInterval);
            StartCoroutine( Shots(--_remainShotCount) );
        }
    }

    // 3티어 사격 메소드: 최종 1회 사격
    private void Shot(){
        curRemainAmmo--;
        UpdateUI();

        fireDirection = transform.eulerAngles;
        
        // idle 조준 상태일 경우 랜더스프레드 적용
        if(isHipFire)
        {
            xVar = GetRandomNormalDistribution(0, curSpread);
            yVar = GetRandomNormalDistribution(0, curSpread);

            fireDirection = Quaternion.AngleAxis(xVar, Vector3.right) * fireDirection;
            fireDirection = Quaternion.AngleAxis(yVar, Vector3.up) * fireDirection;
        }

        curSpread += recoilPerShot;

        var bullet = bulletPool.Get();

        lastFireTime = Time.time;
    }
    

    public void Detached() =>  isTriggered = false;
    #endregion
    #region 총알
    // 총알 수 확인 및 총기상태 변경
    private void AmmoCheck()
    {
        if(curRemainAmmo > 0) state = State.ready;
        else 
        {
            state = State.empty;
            if(autoReload) Reload();
        }
    }

    // 탄창 회복
    public void GetMag(int _count)  => curRemainMag += _count;

    // 재장전 시도
    public void Reload()
    {
        if(curRemainMag <= 0) return;

        StartCoroutine( Reloadging() );
    }

    // 실제 재장전
    IEnumerator Reloadging()
    {
        state = State.reloading;
        curRemainAmmo = 0;

        yield return new WaitForSeconds( reloadTime );

        curRemainAmmo = magCappacity;
        curRemainMag--;
        state = State.ready;

        UpdateUI();
    }
    #endregion

    // 총기 정보를 SO에서 초기화
    private void SettingData()
    {   
        havingFireMode = weaponData.HavingFireMode;
        fireRPM = weaponData.FireRPM;
        magCappacity = weaponData.MagCappacity;
        curRemainMag = weaponData.InitMagCount;
        reloadTime = weaponData.ReloadTime;
        MOA = weaponData.MOA;
        recoilPerShot = weaponData.RecoilPerShot;
        recoilRecoverTime = weaponData.RecoilRecoverTime;
    }

    // 발사모드 변경: 다음 발사모드로 순환하며 바꾼다.
    public void ChangeFireMode()
    {
        curFireMode++;
        if( curFireMode >= havingFireMode.Length) curFireMode = 0;
    }

    // 사격상태 변경: true를 받으면 기본사격상태(HipFire), 아니면 조준사격상태
    public void SetFireState(bool _hipFire) =>  isHipFire = _hipFire;

    // 정규분포 난수 생성기
    // 출처: https://github.com/IJEMIN/Unity-TPS-Sample
    private float GetRandomNormalDistribution(float mean, float standard) // standard: 정규분포
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2));
    }

    public void UpdateUI()
    {
        if(!useUI || !photonView.IsMine) return;    // 네트워크 통제 영역
        GameUIManager.Instance.UpdateAmmo(curRemainAmmo);
        GameUIManager.Instance.Updatemag(curRemainMag);
    }
}