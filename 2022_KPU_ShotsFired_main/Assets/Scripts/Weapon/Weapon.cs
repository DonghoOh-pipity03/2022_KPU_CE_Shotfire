using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;
using Knife.Effects;
using RayFire;
using Andtech.ProTracer;

[RequireComponent(typeof(AudioSource))]

public class Weapon : MonoBehaviourPunCallbacks
{
    PlayerAttack playerAttack;
    [SerializeField] WeaponData weaponData; // 총기 SO
    [SerializeField] Bullet bulletPrefab;  // 총알 프리팹
    public Transform muzzlePosition; // 총구 위치
    [Header("사운드")]
    AudioSource gunAudioPlayer;
    [SerializeField] AudioClip[] clip_shot;  // 사격음
    [SerializeField] float clipLifeTime = 1.1f;    // 사격음 재생시간
    [SerializeField] float clipWaitTime = 0.02f;    // 사격음 연속재생 간격시간
    [SerializeField] float volume_shot = 1.0f;  // 사격음 볼륨
    [SerializeField] AudioClip[] clip_reload;  // 재장전음
    [SerializeField] AudioClip[] clip_Suppress; // 제압소리
    [SerializeField] int SuppressSoundPlayPercent = 10; // 제압 소리 재생 확률
    [SerializeField] AudioClip clip_hit;    // 히트 소리
    [Header("파티클")]
    [SerializeField] ParticleGroupEmitter[]  shotParticle;    // 사격시 재생할 파티클
    [SerializeField] ParticleGroupPlayer[] shotParticle2;   // 사격시 재생할 파티클
    [SerializeField] GameObject shotParticle3;  // 사격시 재생할 파티클 프리팹
    [SerializeField] Bullet2 raytracerPrefab;   // 예광탄 프리팹
    [Header("기타")]
    [SerializeField] LayerMask gunLayerMask;    // 사격판정에 사용할 레이어마스크_플레이어, 적, 레벨디자인
    [SerializeField] LayerMask suppressLayerMask;   // 제압레이어 마스크
    [SerializeField] RayfireGun rayfireGun; // 장애물 파괴용 컴포넌트_총
    [SerializeField] Transform rayfireTarget;
    [SerializeField] float fittingForward;    // AI전용_총기 앞방향 조정용 (현재 앞뒤로만 조정가능)

    #region 전역 변수
    [Header("이하 디버그용")]
    [SerializeField] bool useUI = false;    // 무기 관련 UI를 사용하는지 여부
    [SerializeField] bool autoReload = true;    // 자동 재장전 정책 허용여부
    [SerializeField] bool useRecoilInIdle = true;    // idle 사격에서도 화면 반동을 사용하는지
    float MaxSpread = 5f;   // 에임UI의 최대 스프레드 값
    #endregion

    #region 전역 동작 변수
    ObjectPool<Bullet> bulletPool;  // 총알 오브젝트 풀    
    // 총기 상태
    GameObject weaponUser; // 무기 사용자 오브젝트
    bool useParticleInLocal = false; // 무기 이펙트의 로컬생성을 허용여부, AI는 레그돌 변환시 자식의 갯수를 맞춰야 하기에 flase로 사용
    public enum State { ready, empty, reloading, shooting }
    [HideInInspector] public State state;    // 현재 상태
    int curFireMode = 0;    // 현재 발사모드
    bool isTriggered = false;   // 트리거가 눌려져 있는지
    bool isHipFire = true;  // 기본 사격 상태인지
    float lastFireTime = -1; // 마지막 발사 시간
    Vector3 fireDirection; // 의도하는 사격 방향
    Vector3 hitPoint;   // 피탄 위치
    float fireInterval => (60 / weaponData.FireRPM); // 발사 속도에 따른 발사 간격
    Ray ray;
    RaycastHit hit;
    // 명중률
    float curSpread = 1;    // 현재 스프레드
    float curRecoilX;    // 현재 x 반동 값
    float curRecoilX2;  // 현재 x 반동 값 2
    float curRecoilY;    // 현재 y 반동 값
    float curRecoilZ;  // 현재 z 반동 값
    // 총알
    public int curRemainAmmo;    // 현재 탄창에 남은 총알 수
    int curRemainMag = 5;   // 현재 남은 탄창 수
    Coroutine reloadCoroutine;    // 재장전시 사용되는 코루틴 변수
    bool isRunningReloadCoroutine;  // 위의 코루틴이 동작중인지 여부
    Coroutine shotCoroutine;    // 사격시 사용되는 코루틴 변수
    #endregion

    #region 콜백 함수
    public override void OnEnable()
    {
        base.OnEnable();
        isRunningReloadCoroutine = false;
        // 재장전 중에 무기를 집어넣었다가 다시 꺼낼 때, 재장전 중이였다면 자동 재장전을 시도한다.
        if (state == State.reloading) Reload();

        // 발사 중 무기를 집어넣었다가 다시 꺼낼 때, 현재 무기 상태를 다시 점검한다.
        if ((Time.time >= lastFireTime + fireInterval) && (curRemainAmmo > 0)) state = State.ready;
    }
    private void Start()
    {
        curRemainMag = weaponData.InitMagCount;
        weaponUser = transform.root.gameObject;
        if (weaponUser.tag == "Player") {
            playerAttack = weaponUser.GetComponent<PlayerAttack>();
            useParticleInLocal = true;
        }
        curRemainAmmo = weaponData.MagCappacity;
        UpdateUI();
        gunAudioPlayer = GetComponent<AudioSource>();
        
        // RayFire 컴포넌트 세팅
        rayfireGun = GetComponent<RayfireGun>();
        rayfireTarget = transform.Find("RayFire Target");
        rayfireGun.target = rayfireTarget;
        rayfireGun.maxDistance = bulletPrefab.bulletData.MaxDistance;

        #region 오브젝트 풀링
        bulletPool = new ObjectPool<Bullet>
        (
            createFunc: () =>    // 오브젝트를 생성할 때(풀이 부족할 때)
            {
                var newBullet = Instantiate(bulletPrefab);
                newBullet.poolToReturn = bulletPool;
                return newBullet;
            },
            actionOnGet: (bullet) =>   // 풀에서 오브젝트를 꺼낼 때
            {
                bullet.gameObject.SetActive(true);
                bullet.Reset(weaponUser, muzzlePosition.position, fireDirection);
            },
            actionOnRelease: (bullet) =>   // 오브젝트를 다시 풀에 넣을 때
            {
                bullet.gameObject.SetActive(false);
            },
            actionOnDestroy: (bullet) =>    // 풀이 가득차서 오브젝트가 들어갈 공간이 없을 때
            {
                Destroy(bullet.gameObject);
            },
            maxSize: (weaponData.MagCappacity * weaponData.BallPerOneShot)
        );
        #endregion
    }

    private void Update()
    {
        if (photonView == null || !photonView.IsMine) return;    // 네트워크 통제 영역

        if (useUI && isHipFire) GameUIManager.Instance.UpdateAim(Mathf.Clamp((curSpread - weaponData.MinSpread) / MaxSpread, 0, 1));
    }

    private void FixedUpdate()
    {
        if (curSpread > weaponData.MinSpread)
        {
            curSpread -= weaponData.RecoilRecover * Time.deltaTime;
            if (curSpread < weaponData.MinSpread) curSpread = weaponData.MinSpread;
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
        if (weaponData.UseMag && state != State.ready) return;  // 탄창식 장전방식을 사용하고, 발사준비가 안되었을 때 -> 리턴
        else if (!weaponData.UseMag)  // 샷건식 장전방식을 사용하고, 발사준비가 안되었을 때 -> 리턴
        {   // 잔탄이 없거나, 사격중이거나, 재장전중이면서 잔탄이 없는 경우
            if ((state == State.empty || state == State.shooting) || (state == State.reloading && curRemainAmmo == 0))
                return;
        }

        // 샷건식 장전방식에, 재장전 중이라면, 재장전 코루틴 중지
        if (!weaponData.UseMag && state == State.reloading)
        {
            if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
            isRunningReloadCoroutine = false;
        }

        if (_fireCount == 0)
        {
            switch (weaponData.HavingFireMode[curFireMode])
            {
                case FireMode.automatic:
                    state = State.shooting;
                    shotCoroutine = StartCoroutine(Shots(1));
                    break;

                case FireMode.burst:
                    if (isTriggered == true) return;
                    state = State.shooting;
                    shotCoroutine = StartCoroutine(Shots(3));
                    break;

                case FireMode.twice:
                    if (isTriggered == true) return;
                    state = State.shooting;
                    shotCoroutine = StartCoroutine(Shots(2));
                    break;

                case FireMode.single:
                    if (isTriggered == true) return;
                    state = State.shooting;
                    shotCoroutine = StartCoroutine(Shots(1));
                    break;
            }
            isTriggered = true;
        }
        else
        {
            state = State.shooting;
            shotCoroutine = StartCoroutine(Shots(_fireCount));
        }
    }

    // 2티어 사격 메소드: 사격 통제 장치
    IEnumerator Shots(int _remainShotCount)
    {
        // 재장전 중일 경우
        if (isRunningReloadCoroutine == true)
        {
            yield break;
        }

        // 자동사격이 끝났거나, 총알이 없는 경우
        if (_remainShotCount <= 0 || curRemainAmmo <= 0)
        {
            AmmoCheck();
            yield break;
        }

        // 사격
        if (Time.time >= lastFireTime + fireInterval)
        {
            Shot();

            yield return new WaitForSeconds(fireInterval);
            shotCoroutine = StartCoroutine(Shots(--_remainShotCount));
        }
    }

    // 3티어 사격 메소드: 최종 1회 사격
    private void Shot()
    {
        curRemainAmmo--;
        UpdateUI();

        for (int i = 0; i < weaponData.BallPerOneShot; i++)    // 산탄 방식을 위해, 여러 발 처리
        {
            // 1. 투사체 방식 (폐기)
            // fireDirection = muzzlePosition.eulerAngles;
            // 2. 레이캐스트 방식
            if(playerAttack != null ) fireDirection = (playerAttack.aimTarget.position - muzzlePosition.position).normalized;
            else fireDirection = transform.forward * fittingForward;

            curRecoilX = Random.Range(weaponData.RecoilHorizontal.x, weaponData.RecoilHorizontal.y);
            curRecoilX2 = Random.Range(weaponData.RecoilHorizontal.x, weaponData.RecoilHorizontal.y);
            curRecoilY = Random.Range(weaponData.RecoilVertical.x, weaponData.RecoilVertical.y);
            curRecoilZ = Random.Range(weaponData.RecoilZ.x, weaponData.RecoilZ.y);

            // idle 조준상태 또는 샷건일 경우 -> 화면 반동과 랜더스프레드 적용
            if (isHipFire || weaponData.BallPerOneShot != 1)
            {
                // 화면 반동
                if (playerAttack != null && useRecoilInIdle)
                {
                    playerAttack.FireRecoil(new Vector3(-1 * Mathf.Abs(curRecoilY), curRecoilX, curRecoilZ) * weaponData.RecoilMultipleInIdle);
                }

                // 랜덤 스프레드
                fireDirection += new Vector3(curRecoilX, curRecoilY, curRecoilX2) * curSpread * weaponData.RecoilMultiple;
                fireDirection = fireDirection.normalized;
            }
            // zoom 조준 상태일 경우 -> 화면 반동 적용 (플레이어만 가능한 사격 방법)
            else    
            {
                playerAttack.FireRecoil(new Vector3(-1 * Mathf.Abs(curRecoilY), curRecoilX, curRecoilZ) * weaponData.RecoilMultipleInZoom);
            }

            curSpread += weaponData.RecoilPerShot;
            if (curSpread > weaponData.MaxSpread) curSpread = weaponData.MaxSpread;

            // 방식1 (폐기)_투사체 발사 처리
            //var bullet = bulletPool.Get();    

            // 방식2_레이캐스트_총구에서 fireDirection방향으로
            // 2.1. 공격처리
            ray.origin = muzzlePosition.position;
            ray.direction = fireDirection;
            if(Physics.Raycast(ray, out hit, bulletPrefab.bulletData.MaxDistance, gunLayerMask)){
                // (1) Damageable 물체 또는 미확인물체
                //Debug.Log("hit");
                var target = hit.transform.GetComponent<IDamageable>();
                if( target != null)
                {
                    hitPoint = hit.point;
                    if(hit.transform.root.tag != weaponUser.tag)
                    {
                        DamageMessage damageMessage;

                        damageMessage.attacker = weaponUser;
                        damageMessage.ID = Random.Range(0, 2147483647); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
                        damageMessage.damageKind = DamageKind.bullet;
                        damageMessage.damageAmount = bulletPrefab.bulletData.Damage;
                        damageMessage.suppressAmount = bulletPrefab.bulletData.Suppress;
                        damageMessage.hitPoint = hit.point;
                        damageMessage.hitNormal =  hit.normal;

                        target.ApplyDamage(damageMessage);

                        // 히트마커 표시, 히트소리 재생
                        if (!useUI || !photonView.IsMine || hit.transform.root.tag != "Enemy"){}
                        else{ 
                            GameUIManager.Instance.UpdateHitMark();
                            //if(clip_hit != null) 
                            gunAudioPlayer.PlayOneShot(clip_hit);
                        }
                    }
                }
                else hitPoint = hit.point;
            }
            // (2) 제한된 거리에 도달할 경우
            else{
                hitPoint = muzzlePosition.position + fireDirection * bulletPrefab.bulletData.MaxDistance;
            }
            #if UNITY_EDITOR
            Debug.DrawRay(muzzlePosition.position, hitPoint - muzzlePosition.position, Color.green, 1f);
            #endif
            // (3) 장애물
            rayfireTarget.position = hitPoint;
            rayfireGun.Shoot();

            // 2.2. 제압처리_끝점까지 제압콜라이더 감지 및 처리
            var hits = Physics.RaycastAll(muzzlePosition.position, fireDirection, hit.distance, suppressLayerMask);
            foreach( var j in hits){
                var target1 = j.transform.GetComponent<SuppressPoint>();
                if( target1 != null) 
                {   
                    // 제압 수치 부여
                    if(j.transform.root.tag != weaponUser.tag) target1.ApplySuppress(bulletPrefab.bulletData.Suppress);

                    // 제압 소리 재생
                    if(clip_Suppress != null){
                    int randomValue = Random.Range(0, 101);
                    if(randomValue <= SuppressSoundPlayPercent) {
                        SoundManager.Instance.PlaySFX( clip_Suppress[Random.Range(0,clip_Suppress.Length-1)], j.point, name );
                    }
                    }

                }
            }

            // 2.3. 예광탄
            Bullet2 bullet = Instantiate(raytracerPrefab);
            bullet.Completed += OnCompleted;
            bullet.DrawLine(muzzlePosition.position, hitPoint, bulletPrefab.bulletData.Speed, 0);
        }

        // 총소리
        if(clip_shot != null ){
            SoundManager.Instance.PlayLimitSFX(clip_shot[Random.Range(0, clip_shot.Length-1)], volume_shot, clipLifeTime, clipWaitTime, transform.position, gameObject.GetInstanceID().ToString());
        }

        foreach( var i in shotParticle){ i.Emit(1); }
        foreach( var i in shotParticle2){ i.Play(); }
        var impactEffectIstance = useParticleInLocal?
            Instantiate(shotParticle3, muzzlePosition.position, muzzlePosition.rotation, this.transform) as GameObject
            : Instantiate(shotParticle3, muzzlePosition.position, muzzlePosition.rotation) as GameObject;
        Destroy(impactEffectIstance, 4);
        
        lastFireTime = Time.time;
    }

    // 외부 코드_TracerDemo
    private void OnCompleted(object sender, System.EventArgs e)
	{
		// Handle complete event here
		if (sender is TracerObject tracerObject)
		{
			Destroy(tracerObject.gameObject);
		}
	}

    public void Detached() => isTriggered = false;
    #endregion
    #region 총알
    // 총알 수 확인 및 총기상태 변경, 총기 발사가 끝난 후 호출된다.
    private void AmmoCheck()
    {
        if (curRemainAmmo > 0) state = State.ready;
        else if (state == State.shooting)
        {
            state = State.empty;
            if (autoReload) Reload();
        }
    }

    // 탄창 회복
    public void GetMag(int _count) => curRemainMag += _count;

    // 재장전 시도
    public void Reload()
    {
        if (isRunningReloadCoroutine == true) return;    // 이미 재장전 시쿼스라면 리턴

        if (curRemainMag <= 0) return;   // 남은 탄창 수 검사
        if (curRemainAmmo >= (weaponData.UseChamber ? weaponData.MagCappacity + 1 : weaponData.MagCappacity)) return;    // 재장전이 필요없는지 검사 (최대 탄이 재장전된 상태)

        state = State.reloading;
        reloadCoroutine = StartCoroutine(Reloadging());
    }

    // 실제 재장전
    IEnumerator Reloadging()
    {
        isRunningReloadCoroutine = true;
        // 탄창분리 시퀀스
        if (weaponData.UseMag && weaponData.UseChamber && curRemainAmmo >= 1) curRemainAmmo = 1;
        else if (weaponData.UseMag) curRemainAmmo = 0;
        UpdateUI();
        if(clip_reload[0] != null) gunAudioPlayer.PlayOneShot(clip_reload[0]);

        //대기
        yield return new WaitForSeconds(weaponData.ReloadTime);

        if (weaponData.UseMag) curRemainAmmo += weaponData.MagCappacity;
        else curRemainAmmo++;

        curRemainMag--;
        state = State.ready;

        UpdateUI();
        isRunningReloadCoroutine = false;
        if(clip_reload[1] != null) SoundManager.Instance.PlaySFX(clip_reload[1], transform.position, name);

        // 샷건식 재장전의 경우, 계속 재장전 시도
        if (!weaponData.UseMag) Reload();
    }
    #endregion

    // 발사모드 변경: 다음 발사모드로 순환하며 바꾼다.
    public void ChangeFireMode()
    {
        curFireMode++;
        if (curFireMode >= weaponData.HavingFireMode.Length) curFireMode = 0;
    }

    // 사격상태 변경: true를 받으면 기본사격상태(HipFire), 아니면 조준사격상태
    public void SetFireState(bool _hipFire) => isHipFire = _hipFire;

    public void UpdateUI()
    {
        if (!useUI || !photonView.IsMine) return;    // 네트워크 통제 영역
        GameUIManager.Instance.UpdateAmmo(curRemainAmmo);
        GameUIManager.Instance.Updatemag(curRemainMag);
    }
    #endregion
}