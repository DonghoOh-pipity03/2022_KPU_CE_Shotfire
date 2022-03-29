using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    PlayerAttack playerAttack;
    [SerializeField] WeaponData weaponData; // 총기 SO
    [SerializeField] Bullet bulletPrefab;  // 총알 프리팹
    [SerializeField] Transform muzzlePosition; // 총구 위치

    #region 전역 변수
    [SerializeField] bool useUI = false;    // 무기 관련 UI를 사용하는지 여부
    // 총기 정보
    bool autoReload = true;    // 자동 재장전 정책 허용여부
    [SerializeField] bool useRecoilInIdle = true;    // idle 사격에서도 화면 반동을 사용하는지
    // 총기 스펙
    FireMode[] havingFireMode;   // 해당 무기가 가지는 발사모드
    float fireRPM;  // 발사 속도(1rpm = 1round per 1minute)(실제스펙)
    float fireInterval => (60 / fireRPM); // 발사 속도에 따른 발사 간격
    bool useChamber;    // 재장전 시, 약실에 1발 남는 디테일을 구현하는지 여부
    int ballPerOneShot; // 1회 발사당 나가는 총알 구슬 수(샷건용)(일반 총은 1)(1이 아니면 줌상태에서도 퍼져나간다.)
    // 총알
    int magCappacity = 5; // 탄창 용량
    [Header("명중률")]
    [SerializeField] float maxSpread; // 최대 스프레드: 기본 스프레드는 1
    [SerializeField] float recoilPerShot;   // 발사당 스프레드 증가량
    [SerializeField] float recoilRecover;   // 초당 스프레드 회복량
    [SerializeField] Vector2 recoilHorizon; // 기본 가로 반동
    [SerializeField] Vector2 recoilVertical;    // 기본 세로 반동
    [SerializeField] Vector2 recoilZ;   // 기본 Z 반동
    [SerializeField] float recoilMultipleInIdle = 1;    // idle 사격에서 화면 반동의 계수
    #endregion

    #region 전역 동작 변수
    ObjectPool<Bullet> bulletPool;  // 총알 오브젝트 풀    
    // 총기 상태
    GameObject weaponUser; // 무기 사용자 오브젝트
    public enum State{ready, empty, reloading, shooting}
    public State state;    // 현재 상태
    int curFireMode = 0;    // 현재 발사모드
    bool isTriggered = false;   // 트리거가 눌려져 있는지
    bool isHipFire = true;  // 기본 사격 상태인지
    float lastFireTime = -1; // 마지막 발사 시간
    Vector3 fireDirection; // 의도하는 사격 방향
    // 명중률
    float curSpread = 1;    // 현재 스프레드
    float curRecoilX;    // 현재 x 반동 값
    float curRecoilY;    // 현재 y 반동 값
    float curRecoilZ;  // 현재 z 반동 값
    // 총알
    public int curRemainAmmo;    // 현재 탄창에 남은 총알 수
    int curRemainMag = 5;   // 현재 남은 탄창 수
    float reloadTime = 3;  // 재장전 시간
    bool useMag;    // 탄창식 재장전 방법을 사용하는지 여부, 샷건의 경우 false
    Coroutine reloadCoroutine;    // 재장전시 사용되는 코루틴 변수
    bool isRunningReloadCoroutine;  // 위의 코루틴이 동작중인지 여부
    Coroutine shotCoroutine;    // 사격시 사용되는 코루틴 변수
    #endregion

#region 콜백 함수
    public override void OnEnable() {
        base.OnEnable();
        isRunningReloadCoroutine = false;
        // 재장전 중에 무기를 집어넣었다가 다시 꺼낼 때, 재장전 중이였다면 자동 재장전을 시도한다.
        if( state == State.reloading) Reload(); 
        
        // 발사 중 무기를 집어넣었다가 다시 꺼낼 때, 현재 무기 상태를 다시 점검한다.
        if((Time.time >= lastFireTime + fireInterval) && (curRemainAmmo > 0)) state = State.ready;
    }
    private void Start()
    {
        SettingData();
        weaponUser = transform.root.gameObject;
        if(weaponUser.tag == "Player") playerAttack = weaponUser.GetComponent<PlayerAttack>();
        curRemainAmmo = magCappacity;
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
            maxSize: (magCappacity * ballPerOneShot)
        );
        #endregion
    }

    private void Update() 
    {   
            if(!useUI || !photonView.IsMine) return;    // 네트워크 통제 영역
            
            if(isHipFire) GameUIManager.Instance.UpdateAim( Mathf.Clamp( (curSpread -1.5f)/1.5f , 0, 1));
    }

    private void FixedUpdate() 
    {
        if(curSpread > 1)
        {
            curSpread -= recoilRecover * Time.deltaTime;
            if(curSpread < 1) curSpread = 1;
        }
    }
#endregion
#region 함수
    #region 사격
    // 1티어 사격 메소드
    // 발사횟수를 입력받는다: 0_플레이어용, 총기세팅값을 따른다. , 0 이상: AI용
    public void Fire(int _fireCount)
    {    
        // 발사가 가능한지 검사
        if(useMag && state != State.ready) return;  // 탄창식 장전방식을 사용하고, 발사준비가 안되었을 때 -> 리턴
        else if( !useMag )  // 샷건식 장전방식 사용할 경우
        {   // 잔탄이 없거나, 사격중이거나, 재장전중이면서 잔탄이 없는 경우
            if((state == State.empty || state == State.shooting) || (state == State.reloading && curRemainAmmo ==0 ))
            return;
        }

        // 샷건식 장전방식에, 재장전 중이라면, 재장전 코루틴 중지
        if(!useMag && state == State.reloading)
        {
            if( reloadCoroutine != null ) StopCoroutine(reloadCoroutine);
            isRunningReloadCoroutine = false;
        }

        if(_fireCount == 0){
        switch( havingFireMode[curFireMode] ){
            case FireMode.automatic: 
            state = State.shooting;
            shotCoroutine = StartCoroutine( Shots(1) );
            break;

            case FireMode.burst: 
            if(isTriggered == true) return;
            state = State.shooting;
            shotCoroutine = StartCoroutine( Shots(3) );
            break;

            case FireMode.twice: 
            if(isTriggered == true) return;
            state = State.shooting;
            shotCoroutine = StartCoroutine( Shots(2) );
            break;
            
            case FireMode.single: 
            if(isTriggered == true) return;
            state = State.shooting;
            shotCoroutine = StartCoroutine( Shots(1) );
            break;
        }
        isTriggered = true;
        }
        else
        {
            state = State.shooting;
            shotCoroutine = StartCoroutine( Shots(_fireCount) );
        }
    }
    
    // 2티어 사격 메소드: 사격 통제 장치
    IEnumerator Shots(int _remainShotCount)
    {  
        // 재장전 중일 경우
        if(isRunningReloadCoroutine == true )
        {
            yield break;
        }

        // 자동사격이 끝났거나, 총알이 없는 경우
        if( _remainShotCount <= 0 || curRemainAmmo <= 0) 
        {
            AmmoCheck();
            yield break;
        }

        // 재장전이 진행중인 경우
        //if(reloadCoroutine != null && shotCoroutine != null) StopCoroutine(shotCoroutine);

        if(Time.time >= lastFireTime + fireInterval)
        {
            Shot();

            yield return new WaitForSeconds(fireInterval);
            shotCoroutine = StartCoroutine( Shots(--_remainShotCount) );
        }
    }

    // 3티어 사격 메소드: 최종 1회 사격
    private void Shot(){
        curRemainAmmo--;
        UpdateUI();

        for(int i = 0 ; i < ballPerOneShot; i++)    // 산탄총 대응용
        {
            fireDirection = transform.eulerAngles;
            curRecoilX = Random.Range(recoilHorizon.x, recoilHorizon.y);
            curRecoilY = Random.Range(recoilVertical.x, recoilVertical.y);
            curRecoilZ = Random.Range(recoilZ.x, recoilZ.y);

            if(isHipFire || ballPerOneShot != 1)   // idle 조준상태 또는 샷건일 경우, 화면 반동과 랜더스프레드 적용
            {   
                // 화면 반동
                if(playerAttack != null && useRecoilInIdle){
                    playerAttack.FireRecoil( new Vector3(-1 * Mathf.Abs(curRecoilY), curRecoilX, curRecoilZ) * recoilMultipleInIdle);
                }

                // 랜덤 스프레드
                fireDirection += new Vector3(curRecoilY, curRecoilX, 0) * curSpread;
            }
            else    // zoom 조준 상태일 경우_플레이어만 가능한 사격 방법, 화면 반동 적용
            {
                playerAttack.FireRecoil( new Vector3(-1 * Mathf.Abs(curRecoilY), curRecoilX, curRecoilZ) );
            }

            curSpread += recoilPerShot;
            if(curSpread > maxSpread) curSpread = maxSpread;

            var bullet = bulletPool.Get();
        }

        lastFireTime = Time.time;
    }
    

    public void Detached() =>  isTriggered = false;
    #endregion
    #region 총알
    // 총알 수 확인 및 총기상태 변경, 총기 발사가 끝난 후 호출된다.
    private void AmmoCheck()
    {   
        if(curRemainAmmo > 0) state = State.ready;
        else if(state == State.shooting)
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
        if(isRunningReloadCoroutine == true) return;    // 이미 재장전 시쿼스라면 리턴
        //if(state == State.reloading) return;    // 이미 재장전 시퀀스라면 리턴

        if(curRemainMag <= 0) return;   // 남은 탄창 수 검사
        if(curRemainAmmo >= (useChamber ? magCappacity + 1 : magCappacity )) return;    // 재장전이 필요없는지 검사 (최대 탄이 재장전된 상태)

        state = State.reloading;
        reloadCoroutine = StartCoroutine( Reloadging() );
    }

    // 실제 재장전
    IEnumerator Reloadging()
    {
        isRunningReloadCoroutine = true;
        // 탄창분리 시퀀스
        if(useMag && useChamber && curRemainAmmo >= 1) curRemainAmmo = 1;
        else if(useMag) curRemainAmmo = 0;
        UpdateUI();

        //대기
        yield return new WaitForSeconds( reloadTime );

        if(useMag) curRemainAmmo += magCappacity;
        else curRemainAmmo++;
        
        curRemainMag--;
        state = State.ready;

        UpdateUI();
        isRunningReloadCoroutine = false;

        // 샷건식 재장전의 경우, 계속 재장전 시도
        if( !useMag ) Reload();
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
        useChamber = weaponData.UseChamber;
        useMag = weaponData.UseMag;
        ballPerOneShot = weaponData.BallPerOneShot;
        // 명중률
        maxSpread = weaponData.MaxSpread;
        recoilPerShot = weaponData.RecoilPerShot;
        recoilRecover = weaponData.RecoilRecover;
        recoilHorizon = weaponData.RecoilHorizontal;
        recoilVertical = weaponData.RecoilVertical;
        recoilZ = weaponData.RecoilZ;
        // 반동
        recoilMultipleInIdle = weaponData.RecoilMultipleInIdle;
    }

    // 발사모드 변경: 다음 발사모드로 순환하며 바꾼다.
    public void ChangeFireMode()
    {
        curFireMode++;
        if( curFireMode >= havingFireMode.Length) curFireMode = 0;
    }

    // 사격상태 변경: true를 받으면 기본사격상태(HipFire), 아니면 조준사격상태
    public void SetFireState(bool _hipFire) =>  isHipFire = _hipFire;

    public void UpdateUI()
    {
        if(!useUI || !photonView.IsMine) return;    // 네트워크 통제 영역
        GameUIManager.Instance.UpdateAmmo(curRemainAmmo);
        GameUIManager.Instance.Updatemag(curRemainMag);
    }
#endregion
}