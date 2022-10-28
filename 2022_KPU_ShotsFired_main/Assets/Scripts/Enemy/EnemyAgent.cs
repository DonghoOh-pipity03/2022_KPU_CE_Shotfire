using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
/*
 
 */
using TMPro;
using Photon.Pun;
using UnityEngine.Animations.Rigging;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AIState { dead, wait, move, engage }    // AI가 가질 수 있는 상태
class EnemyAgent : LivingEntity
{
    protected NavMeshAgent agent; // 경로 AI 에이전트
    protected LineRenderer lineRenderer;
    protected Animator animator;
    public StageManager stageManager;
    #region 전역 변수
    [SerializeField] protected EnemyData enemyData;   // 적 AI SO
    [SerializeField] protected Transform eyeTransform;    // 눈의 위치 정보
    [SerializeField] protected Transform aim;   // 총기 조준점
    [SerializeField] protected Weapon weapon; // AI가 사용하는 무기
    [SerializeField] protected bool useRagDoll = false;   // 래그돌 사용여부
    [SerializeField] protected GameObject animModel;  // 애니메이션을 사용하는 모델
    [SerializeField] protected GameObject animMeshRoot;    // 애니메이션용 메쉬루트
    [SerializeField] protected GameObject ragdollModel;   // 래그돌을 사용하는 모델
    [SerializeField] protected GameObject ragdollMeshRoot;    // 래그돌용 메쉬루트
    [SerializeField] protected GameObject minimapHolder;  // 미니맵용 UI
    [SerializeField] protected LayerMask sightLayer;  // 시야용 레이어
    [SerializeField] protected LayerMask laserLayer;  // 레이저용 레이어
    [SerializeField] protected float fittingAttackY;  // 공격시 Y축 회전 조정값
    [Header("SFX")]
    [SerializeField] protected AudioClip[] clip_walk;  // 발소리
    [SerializeField] protected float footSoundVolume; // 발소리 크기
    [SerializeField] protected float walk_steptime;   // 발소리 간격 시간
    [Header("이하 디버그용")]
    [SerializeField] protected LayerMask attackTarget;   // 공격 대상의 레이어

    [SerializeField] protected TextMeshProUGUI sign;  // 디버그용 머리 위 텍스트
    [SerializeField] protected bool useSign;  // 디버그용 텍스트 사용 여부
    [SerializeField] protected bool useAttack = true;   // 개발용, 공격을 하는지 여부
    #endregion
    #region 전역 동작변수
    [HideInInspector] public AIState aiState;  // AI의 경계상태
    protected Coroutine AICroutine;   // AI FSM의 행동을 실행하는 코루틴
    protected int[] isPlayerOnSight = new int[4]; // 플레이어가 시야 내에 보이는지 여부
    protected GameObject[] players = new GameObject[4];   // 플레이어들의 게임 오브젝트 정보
    protected LivingEntity[] playerState = new LivingEntity[4];   //플레이어들의 LivingEntity, 체력상태 확인용
    protected Vector3? shootingPoint;    // AI가 조준 및 사격해야 할 position, 플레이어 캐릭터의 상체를 가리킨다.
    protected  float[] playerDistance = { 9999, 9999, 9999, 9999 };  // 각 플레이어와의 거리
    protected int[] targetWeight = new int[4];    // 각 플레이어별 타겟팅(어그로) 가중치
    protected int target; // 최우선 공격 대상: 0~3 플레이어
    protected int lastAttacker;   // 마지막으로 AI를 공격한 플레이어: 0_없음, 1~4_플레이어
    protected Ray ray;    // 플레이어 탐색용 레이
    protected RaycastHit hit; // 레이의 충돌 정보
    protected int rayMask;    // 레이마스크
    protected float turnSmoothVelocity;   // 회전에 사용하는 변수
    protected float lastAttackTime;   // 마지막 공격 시간
    protected float engageDistance;   // 교전 시작거리
    protected bool dead_sync = false; //AI 사망 판정
    [SerializeField] protected Rig rig;
     // 사운드
    protected float lastFootSoundTime;    // 마지막 발소리 출력 시간
    #endregion
    #region 콜백함수
    protected virtual void Awake()
    {
        SettingData();
        agent = GetComponent<NavMeshAgent>();
        agent.baseOffset = -0.075f;
        agent.stoppingDistance = enemyData.MelleeDistance;
        agent.speed = enemyData.MoveSpeed;
        
        engageDistance = UnityEngine.Random.Range(enemyData.MinEngageDistance, enemyData.MaxEngageDistance);

        rayMask = 1 << LayerMask.NameToLayer("Suppress");
        rayMask = ~rayMask;
    }
    public override void OnEnable()
    {
        base.OnEnable();
        lineRenderer = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        ChangeState(AIState.wait);
        Application.onBeforeRender += DrawLaser;
    }
    public override void OnDisable() {
        base.OnDisable();
        Application.onBeforeRender -= DrawLaser;
    }
    protected override void Update()
    {
        base.Update();

        // Debug.Log(photonView);
        
        // Debug.Log(this.curHealth);
        
        if (this.dead_sync == false)
        {
            if (this.entityState == EntityState.dead)
            {                
                base.photonView.RPC("EnemyDead",RpcTarget.All);
                PhotonNetwork.IsMessageQueueRunning = false;
                PhotonNetwork.IsMessageQueueRunning = true;
            }
        }
        if (useSign) sign.text = entityState.ToString() + "\n" + aiState.ToString();

        if(animator != null) animator.SetFloat("MoveVertical", agent.velocity.magnitude / agent.speed);
    }
    protected virtual void LateUpdate() {
        DrawLaser();
    }
    private void FixedUpdate() {
        aim.position = (shootingPoint!= null)?(Vector3)shootingPoint:(transform.position + transform.forward* 2f + transform.up * 1.5f);
    }
    #endregion
    #region 함수
#region LivingEntity 상속
    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);

        base.photonView.RPC("GetDamge",RpcTarget.All,this.curHealth);
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.IsMessageQueueRunning = true;
        // 마지막 공격자를 저장
        var player = _damageMessage.attacker.GetComponent<PlayerController>();
        if (player != null)
        {
            lastAttacker = player.ID;

            // 플레이어 정보가 없으면 플레이어 저장
            if (players[lastAttacker - 1] == null) players[lastAttacker - 1] = _damageMessage.attacker;
        }
    }
#endregion
#region FSM
    // 상태를 변경한다.
    protected void ChangeState(AIState _state)
    {
        if(AICroutine != null) StopCoroutine(AICroutine);//aiState.ToString());
        aiState = _state;
        AICroutine = StartCoroutine(aiState.ToString());
    }

    // 상태변경타이밍을 감지한다
    protected AIState Transition()
    {
        // 죽음
        if (entityState == EntityState.dead) return AIState.dead;
        // 대기 -> 이동, 전투
        else if (aiState == AIState.wait)
        {
            // 적 감지: 이동
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    if(animator != null) animator.SetBool("Engage", true);
                    if(lineRenderer != null) lineRenderer.enabled = true;
                    return AIState.move;
                }
            }
            // 그렇지 않다면
            return AIState.wait;
        }
        // 이동 <-> 전투
        else
        {   
            // 시야 내에 있고 교전거리 내에 있을 때: 전투
            for (int i = 0; i < 4; i++)
            {
                if (isPlayerOnSight[i] == 1 && playerDistance[i] < engageDistance)
                {
                    return AIState.engage;
                }
            }
            // 그렇지 않다면
            return AIState.move;
        }
    }

    // 아래의 FSM_행동은 입력-판단-출력의 시퀀스를 가진다.
    protected IEnumerator dead()
    {
        // 시작시 코드
        agent.enabled = false;
        if(stageManager != null && stageManager.isActiveAndEnabled) stageManager.GetAliveEnemyCount();

        if(animator == null) yield break;
        animator.SetBool("DeathBack", true);

        if(lineRenderer != null) lineRenderer.enabled = false;
        if(minimapHolder != null) minimapHolder.SetActive(false);

        rig.weight=0;
        if(!useRagDoll) yield break;    // 이하 레그돌 영역
        var ragdollTime = UnityEngine.Random.Range(0f,.75f);
        yield return new WaitForSeconds(ragdollTime);
        CopyCharacterTransformToRagdoll(animModel.transform, ragdollModel.transform);
        Destroy(animMeshRoot);
        Destroy(animModel);
        ragdollModel.SetActive(true);
        ragdollMeshRoot.SetActive(true);

        // 수행중 코드
        yield break;
    }
    protected IEnumerator wait()
    {
        // 시작시 코드
        // 수행중 코드
        while (true)
        {
            SenseEntity();
            RenewPlayerInfo();
            
            AIState newState = Transition();
            if(aiState != newState) ChangeState(newState);

            yield return new WaitForSeconds(0.1f);
        }
    }
    protected IEnumerator move()
    {
        // 시작시 코드
        rig.weight = 1;
        agent.speed = enemyData.MoveSpeed;
        // 수행중 코드
        while (true)
        {
            SenseEntity();
            RenewPlayerInfo();

            AIState newState = Transition();
            if(aiState != newState) ChangeState(newState);
            
            if(agent.enabled == true) 
            {
                agent.SetDestination(players[target].transform.position);
            }
            Attack();
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected IEnumerator engage()
    {
        // 시작시 코드
        rig.weight = 1;
        agent.speed = 0;
        // 수행중 코드
        while (true)
        {
            SenseEntity();
            RenewPlayerInfo();

            AIState newState = Transition();
            if(aiState != newState) ChangeState(newState);

            Attack();
            yield return new WaitForSeconds(0.01f);
        }
    }
#endregion
#region 유틸리티
    // 시야내 타겟 감지: 플레이어별로 시야 확인 유무를 저장하고, 1명이라도 시야 내에 있다면 true를 반환한다.
    protected bool SenseEntity()
    {   
        // 시야유무 정보 리셋
         for (int i = 0; i < 4; i++)
        { 
            isPlayerOnSight[i] = 0;
        }

        // 시야 확인
        var colliders = Physics.OverlapSphere(eyeTransform.position, enemyData.EyeDistance, attackTarget);
        foreach (var collider in colliders)
        {
            // 시야 거리 내에서 존재한 상황
            var livingEntity = collider.GetComponent<LivingEntity>();
            if (livingEntity != null && livingEntity.entityState != EntityState.dead)
            {
                Vector3 direction;
                shootingPoint = null;
                if( (shootingPoint = collider.GetComponent<LivingEntity>().shotPoint.position) != null ){
                    direction = (Vector3)shootingPoint - eyeTransform.position;
                }else{
                    direction = collider.transform.position - eyeTransform.position;
                    direction.y = eyeTransform.forward.y;
                }

                // (1) 대기상태 중 시야각 내에 있거나, (2) 대기 상태가 아니라면, 육안 관측이 가능한지 파악한다.
                if ((aiState == AIState.wait && Vector3.Angle(direction, eyeTransform.forward) < enemyData.FieldOfView * 0.5f)
                    || (aiState != AIState.wait))
                {
                    ShotRay(direction);
                    Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 5f);
                    if (hit.transform != null)
                    {
                        if (hit.transform.root.gameObject == collider.gameObject)
                        {
                            int ID = hit.transform.root.gameObject.GetComponent<PlayerController>().ID;
                            if (ID != 0) isPlayerOnSight[ID - 1] = 1;

                            // 플레이어 정보 등록
                            if (players[ID - 1] == null) 
                            {
                                players[ID - 1] = hit.transform.root.gameObject;
                                playerState[ID - 1] = players[ID - 1].GetComponent<LivingEntity>();
                            }
                        }
                    }
                }
            }
        }

        // 플레이어 확인시 true 리턴
        for (int i = 0; i < 4; i++)
        {
            if (isPlayerOnSight[i] == 1) return true;
        }
        return false;
    }

    // 방향을 입력 받아, 레이를 쏜다.
    protected void ShotRay(Vector3 _dir)
    {
        ray.origin = eyeTransform.position;
        ray.direction = _dir;
        Physics.Raycast(ray, out hit, enemyData.EyeDistance, sightLayer);
    }

#if UNITY_EDITOR
    // 시야 디버그용
    protected void OnDrawGizmosSelected()
    {
        var leftRayRotation = Quaternion.AngleAxis(-enemyData.FieldOfView * 0.5f, Vector3.up);
        var leftRayDirection = leftRayRotation * transform.forward;
        Handles.color = new Color(1f, 1f, 1f, 0.2f);
        Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, enemyData.FieldOfView, enemyData.EyeDistance);
    }
#endif

    // 각 플레이어와의 거리, 타겟 가중치 계산 및 최우선 공격 대상 선정
    protected void RenewPlayerInfo()
    {
        // 거리와 가중치 계산
        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null) continue;
            if( playerState[i] == null) playerState[i] = players[i].GetComponent<LivingEntity>();

            playerDistance[i] = (players[i].transform.position - transform.position).magnitude; // 거리 계산

            // 타겟 가중치 계산
            targetWeight[i] = playerState[i].entityState == EntityState.dead ? 0 :  // 죽음상태 가중치
                                (int)((Mathf.Clamp((-1 * playerDistance[i] + 25), 0, 25) * enemyData.DistanceTargetWeight  // 거리 가중치
                                + ((lastAttacker == i + 1) ? enemyData.AttackerTargetWeight : 0))   // 마지막 공격자 가중치
                                * (playerState[i].entityState == EntityState.alive ? 1 : enemyData.DownTargetWeight));   // 다운 상태 가중치
        }

        // 최우선 타겟 선정
        int maxValue = targetWeight.Max();
        target = targetWeight.ToList().IndexOf(maxValue);
    }

    // 공격
    protected void Attack()
    {
        // 회전
        if (aiState == AIState.engage)
        {
            var lookRotation = Quaternion.LookRotation(players[target].transform.position - transform.position, Vector3.up);
            var targetAngleY = lookRotation.eulerAngles.y + fittingAttackY;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, enemyData.TurnSpeed);
        }

        // 공격 방법 선정 & 공격
        if (playerDistance[target] < enemyData.MelleeDistance) MelleeAttack();    //근접공격
        else ShotAttack();  // 원거리 공격
    }

    protected void MelleeAttack()
    {

    }

    protected void ShotAttack()
    {
        // 시야내에 타겟이 있으며, 총기방향과 타겟방향의 차이가 허용 각도 내에 있으면, 조준을 타겟의 에임 포인트로 이동
        // 그렇지 않으면, 기본 위치로

        // 발사주기가 되었으면, 발사횟수를 랜덤으로 결정 후 발사
        if (Time.time > lastAttackTime + enemyData.AttackDelay)
        {
            int fireCount = UnityEngine.Random.Range(enemyData.MinAttackCount, enemyData.MaxAttackCount + 1);
            if (useAttack) weapon.Fire(fireCount);
            lastAttackTime = Time.time;
        }
        // 그렇지 않으면, 리턴
    }

    protected void SettingData()
    {
        maxHealth = enemyData.MaxHealth;
        maxSuppress = enemyData.MaxSuppress;
        unsuppressAmount = enemyData.UnsuppressAmount;
        hitMultiple = (float[])(enemyData.HitMultiple.Clone());
    }

    // 경계(이동 또는 전투)상태가 되며, 게임 매니저로 부터 플레이어의 정보를 가져온다.
    public void SetAlert()
    {
        players = GameManager.Instance.players;
        for(int i = 0 ; i < players.Length ; i++)
        {
            if(players[i] != null) playerState[i] = players[i].GetComponent<LivingEntity>();
        }
        ChangeState(Transition());

    }

    // 출처: https://www.youtube.com/watch?v=cTHceZpwGt4
    protected void CopyCharacterTransformToRagdoll(Transform from, Transform to){
        for(int i = 0 ;  i < from.childCount; i++){
            if(from.childCount != 0){
                CopyCharacterTransformToRagdoll(from.GetChild(i), to.GetChild(i));
            } 
            to.GetChild(i).localPosition = from.GetChild(i).localPosition;
            to.GetChild(i).localRotation = from.GetChild(i).localRotation;
        }
    }

    protected void DrawLaser(){
        if( entityState != EntityState.dead && lineRenderer != null){
            lineRenderer.SetPosition(0, weapon.muzzlePosition.position);
            ray.origin = weapon.muzzlePosition.position;
            ray.direction = weapon.muzzlePosition.forward;
            if( Physics.Raycast(ray, out hit, enemyData.EyeDistance, laserLayer)){
                lineRenderer.SetPosition(1, hit.point);
            }
            else{
                lineRenderer.SetPosition(1, weapon.muzzlePosition.position + weapon.muzzlePosition.forward * enemyData.EyeDistance);
            }
        }
    }

    protected void updateSound(){
        if(agent.speed > 0.5f){
            if(  Time.time > lastFootSoundTime + walk_steptime && clip_walk != null) { 
                SoundManager.Instance.PlaySFX( clip_walk[UnityEngine.Random.Range(0, clip_walk.Length -1)], footSoundVolume, transform.position, name ); 
                lastFootSoundTime = Time.time;
            }
        }
    }
#endregion
    #endregion

    [PunRPC]
    protected void EnemyDead()
    {
        this.entityState = EntityState.dead;
        this.dead_sync = true;
        // PhotonNetwork.Destroy(this.gameObject);
    }

    [PunRPC]
    protected void GetDamge(float hp)
    {
        this.curHealth = hp;
    }
}