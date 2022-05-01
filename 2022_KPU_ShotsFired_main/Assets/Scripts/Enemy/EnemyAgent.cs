using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AIState { dead, wait, move, engage }    // AI가 가질 수 있는 상태
class EnemyAgent : LivingEntity
{
    private NavMeshAgent agent; // 경로 AI 에이전트
    LineRenderer lineRenderer;
    Animator animator;
    #region 전역 변수
    [SerializeField] EnemyData enemyData;   // 적 AI SO
    [SerializeField] Transform eyeTransform;    // 눈의 위치 정보
    [SerializeField] Weapon weapon; // AI가 사용하는 무기
    [SerializeField] bool useRagDoll = false;   // 래그돌 사용여부
    [SerializeField] GameObject animModel;  // 애니메이션을 사용하는 모델
    [SerializeField] GameObject animMeshRoot;    // 애니메이션용 메쉬루트
    [SerializeField] GameObject ragdollModel;   // 래그돌을 사용하는 모델
    [SerializeField] GameObject ragdollMeshRoot;    // 래그돌용 메쉬루트
    [SerializeField] GameObject minimapHolder;  // 미니맵용 UI
    [Header("이하 디버그용")]
    [SerializeField] LayerMask attackTarget;   // 공격 대상의 레이어

    [SerializeField] TextMeshProUGUI sign;  // 디버그용 머리 위 텍스트
    [SerializeField] private bool useSign;  // 디버그용 텍스트 사용 여부
    [SerializeField] bool useAttack = true;   // 개발용, 공격을 하는지 여부
    #endregion
    #region 전역 동작변수
    private AIState aiState;  // AI의 경계상태
    Coroutine AICroutine;   // AI FSM의 행동을 실행하는 코루틴
    int[] isPlayerOnSight = new int[4]; // 플레이어가 시야 내에 보이는지 여부
    GameObject[] players = new GameObject[4];   // 플레이어들의 게임 오브젝트 정보
    LivingEntity[] playerState = new LivingEntity[4];   //플레이어들의 LivingEntity, 체력상태 확인용
    float[] playerDistance = { 9999, 9999, 9999, 9999 };  // 각 플레이어와의 거리
    int[] targetWeight = new int[4];    // 각 플레이어별 타겟팅(어그로) 가중치
    int target; // 최우선 공격 대상: 0~3 플레이어
    int lastAttacker;   // 마지막으로 AI를 공격한 플레이어: 0_없음, 1~4_플레이어
    Ray ray;    // 플레이어 탐색용 레이
    RaycastHit hit; // 레이의 충돌 정보
    int rayMask;    // 레이마스크
    float turnSmoothVelocity;   // 회전에 사용하는 변수
    float lastAttackTime;   // 마지막 공격 시간
    float engageDistance;   // 교전 시작거리
    #endregion
    #region 콜백함수
    private void Awake()
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
    }

    protected override void Update()
    {
        base.Update();

        if (useSign) sign.text = entityState.ToString() + "\n" + aiState.ToString();

        if(animator != null) animator.SetFloat("MoveVertical", agent.velocity.magnitude / agent.speed);

        if( entityState != EntityState.dead && lineRenderer != null){
            lineRenderer.SetPosition(0, weapon.muzzlePosition.position);
            ray.origin = weapon.muzzlePosition.position;
            ray.direction = weapon.muzzlePosition.forward;
            if( Physics.Raycast(ray, out hit, enemyData.EyeDistance, rayMask)){
                lineRenderer.SetPosition(1, hit.point);
            }
            else{
                lineRenderer.SetPosition(1, weapon.muzzlePosition.position + weapon.muzzlePosition.forward * enemyData.EyeDistance);
            }
        }
    }
    #endregion
    #region 함수
#region LivingEntity 상속
    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);

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
    private void ChangeState(AIState _state)
    {
        if(AICroutine != null) StopCoroutine(AICroutine);//aiState.ToString());
        aiState = _state;
        AICroutine = StartCoroutine(aiState.ToString());
    }

    // 상태변경타이밍을 감지한다
    private AIState Transition()
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
    IEnumerator dead()
    {
        // 시작시 코드
        agent.enabled = false;

        if(animator == null) yield break;
        animator.SetBool("DeathBack", true);

        if(lineRenderer != null) lineRenderer.enabled = false;
        if(minimapHolder != null) minimapHolder.SetActive(false);

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
    IEnumerator wait()
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
    IEnumerator move()
    {
        // 시작시 코드
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

    IEnumerator engage()
    {
        // 시작시 코드
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
    private bool SenseEntity()
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
                var direction = collider.transform.position - eyeTransform.position;
                direction.y = eyeTransform.forward.y;

                // 대기상태라면 시야각 내에 있는지 파악한다.
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
    private void ShotRay(Vector3 _dir)
    {
        ray.origin = eyeTransform.position;
        ray.direction = _dir;
        Physics.Raycast(ray, out hit, enemyData.EyeDistance, rayMask);
    }

#if UNITY_EDITOR
    // 시야 디버그용
    private void OnDrawGizmosSelected()
    {
        var leftRayRotation = Quaternion.AngleAxis(-enemyData.FieldOfView * 0.5f, Vector3.up);
        var leftRayDirection = leftRayRotation * transform.forward;
        Handles.color = new Color(1f, 1f, 1f, 0.2f);
        Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, enemyData.FieldOfView, enemyData.EyeDistance);
    }
#endif

    // 각 플레이어와의 거리, 타겟 가중치 계산 및 최우선 공격 대상 선정
    private void RenewPlayerInfo()
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
    private void Attack()
    {
        // 회전
        if (aiState == AIState.engage)
        {
            var lookRotation = Quaternion.LookRotation(players[target].transform.position - transform.position, Vector3.up);
            var targetAngleY = lookRotation.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, enemyData.TurnSpeed);
        }

        // 공격 방법 선정 & 공격
        if (playerDistance[target] < enemyData.MelleeDistance) MelleeAttack();    //근접공격
        else ShotAttack();  // 원거리 공격
    }

    private void MelleeAttack()
    {

    }

    private void ShotAttack()
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

    private void SettingData()
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
    private void CopyCharacterTransformToRagdoll(Transform from, Transform to){
        for(int i = 0 ;  i < from.childCount; i++){
            if(from.childCount != 0){
                CopyCharacterTransformToRagdoll(from.GetChild(i), to.GetChild(i));
            } 
            to.GetChild(i).localPosition = from.GetChild(i).localPosition;
            to.GetChild(i).localRotation = from.GetChild(i).localRotation;
        }
    }
#endregion
    #endregion
}