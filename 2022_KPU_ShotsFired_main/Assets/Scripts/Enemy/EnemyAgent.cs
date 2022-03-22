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

public enum AIState { wait, move, engage }    // AI가 가질 수 있는 경계상태
class EnemyAgent : LivingEntity
{
    private NavMeshAgent agent; // 경로 AI 에이전트
    #region 전역 변수
    [SerializeField] private EnemyData enemyData;   // 적 AI SO
    [SerializeField] private Transform eyeTransform;    // 눈의 위치 정보
    [SerializeField] private LayerMask attackTarget;   // 공격 대상의 레이어
    [SerializeField] private TextMeshProUGUI sign;  // 디버그용 머리 위 텍스트
    [SerializeField] private bool useSign;  // 디버그용 텍스트 사용 여부
    private string enemyName;   // AI의 이름
    // 시야 파라미터
    private float eyeDistance; // 시야 거리
    private float fieldOfView; // 시야각
    // 공격 파라미터
    private float engageDistance;  // 교전을 위한 이동 정지거리
    private float melleeDistance; // 근접공격을 시작하는 거리
    private float distanceTargetWeight;    // 공격 타겟 선정을 위한 거리별 가중치
    private float attackerTargetWeight;    // 공격 타겟 선정을 위한 마지막 공격자 가중치
    // 이동 파라미터
    private float moveSpeed;   // 이동 속도
    #endregion
    #region 전역 동작변수
    private AIState curState;  // AI의 경계상태
    private int[] isPlayerOnSight = new int[4]; // 플레이어가 시야 내에 보이는지 여부
    private GameObject[] players = new GameObject[4];   // 플레이어들의 게임 오브젝트 정보
    private float[] playerDistance = { 9999, 9999, 9999, 9999 };  // 각 플레이어와의 거리
    private int[] targetWeight = new int[4];    // 각 플레이어별 타겟팅(어그로) 가중치
    private int target; // 최우선 공격 대상: 0~3 플레이어
    private int lastAttacker;   // 마지막으로 AI를 공격한 플레이어: 0_없음, 1~4_플레이어
    private Ray ray;    // 플레이어 탐색용 레이
    RaycastHit hit; // 레이의 충돌 정보
    int rayMask;    // 레이마스크
    #endregion
    #region 콜백함수
    private void Awake()
    {
        SettingData();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = engageDistance;
        agent.speed = moveSpeed;
        rayMask = 1 << LayerMask.NameToLayer("Suppress");
        rayMask = ~rayMask;
    }
    public override void OnEnable()
    {
        base.OnEnable();

        curState = AIState.wait;
    }

    protected override void Update()
    {
        base.Update();

        if (useSign) sign.text = state.ToString() + "\n" + curState.ToString();
    }
    private void FixedUpdate()
    {
        if (state == EntityState.dead) return;

        SenseEntity();
        RenewPlayerInfo();

        if (curState != AIState.wait)
        {
            AIStateBT();
            AIAttackBT();
        }
    }
    #endregion
    #region 함수
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

        // 데미지를 받으면 대기상태에서 해제
        if (curState == AIState.wait)
        {
            var distance = (_damageMessage.attacker.transform.position - eyeTransform.position).magnitude;
            if (distance > engageDistance) curState = AIState.move;
            else curState = AIState.engage;
        }
    }

    protected override void Die()
    {
        base.Die();

        agent.enabled = false;
    }

    // 시야내 모든 타겟 감지: 플레이어별로 시야 확인 유무를 저장하고, 1명이라도 시야내에 있다면 true를 반환한다.
    private bool SenseEntity()
    {
        for (int i = 0; i < 4; i++)
        {
            isPlayerOnSight[i] = 0;
        }

        var colliders = Physics.OverlapSphere(eyeTransform.position, eyeDistance, attackTarget);
        foreach (var collider in colliders)
        {
            // 시야 거리 내에서 존재한 상황
            var livingEntity = collider.GetComponent<LivingEntity>();
            if (livingEntity != null && livingEntity.state != EntityState.dead)
            {
                var direction = collider.transform.position - eyeTransform.position;
                direction.y = eyeTransform.forward.y;

                // 대기상태라면 시야각 내에 있는지 파악한다.
                if ((curState == AIState.wait && Vector3.Angle(direction, eyeTransform.forward) < fieldOfView * 0.5f)
                    || (curState != AIState.wait))
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
                            if (players[ID - 1] == null) players[ID - 1] = hit.transform.root.gameObject;

                            // AI 상태 변경
                            if (curState == AIState.wait)
                            {
                                var distance = (hit.transform.position - eyeTransform.position).magnitude;
                                if (distance > engageDistance) curState = AIState.move;
                                else curState = AIState.engage;
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (isPlayerOnSight[i] == 1) return true;
        }
        return false;
    }

    // 방향을 입력 받아, 레이를 쏜다.
    private void ShotRay(Vector3 _dir)
    {
        //ray = new Ray();
        ray.origin = eyeTransform.position;
        ray.direction = _dir;
        Physics.Raycast(ray, out hit, eyeDistance, rayMask);
    }

#if UNITY_EDITOR
    // 시야 디버그용
    private void OnDrawGizmosSelected()
    {
        var leftRayRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
        var leftRayDirection = leftRayRotation * transform.forward;
        Handles.color = new Color(1f, 1f, 1f, 0.2f);
        Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, eyeDistance);
    }
#endif

    // 각 플레이어와의 거리, 타겟 가중치 계산 및 최우선 공격 대상 선정
    private void RenewPlayerInfo()
    {
        // 가중치 계산
        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null) continue;

            playerDistance[i] = (players[i].transform.position - transform.position).magnitude; // 거리 계산

            targetWeight[i] = (int)(
                                Mathf.Clamp((-1 * playerDistance[i] + 25), 0, 25) * distanceTargetWeight
                                + ((lastAttacker == i + 1) ? attackerTargetWeight : 0));    // 타겟 가중치 계산
        }

        // 최우선 타겟 선정
        int maxValue = targetWeight.Max();
        target = targetWeight.ToList().IndexOf(maxValue);
    }

    // AI 행동트리_상태
    private void AIStateBT()
    {
        curState = AIState.move;

        for (int i = 0; i < 4; i++)
        {
            // 시야 내에 있고 교전거리 내에 있을 때: 정지
            if (isPlayerOnSight[i] == 1 && playerDistance[i] < engageDistance)
            {
                curState = AIState.engage;
                agent.speed = 0;
                //agent.isStopped = true;
                return;
            }
        }

        // 이동
        //agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(players[target].transform.position);
    }

    // AI 행동트리_공격
    private void AIAttackBT()
    {   
        // 회전

        // 공격 방법 선정 & 공격
        if (playerDistance[target] > melleeDistance) MelleeAttack();    //근접공격
        else ShotAttack();  // 원거리 공격
    }

    private void MelleeAttack()
    {

    }

    private void ShotAttack()
    {

    }

    private void SettingData()
    {
        enemyName = enemyData.EnemyName;
        maxHealth = enemyData.MaxHealth;
        maxSuppress = enemyData.MaxSuppress;
        unsuppressAmount = enemyData.UnsuppressAmount;
        hitMultiple = (float[])(enemyData.HitMultiple.Clone());
        eyeDistance = enemyData.EyeDistance;
        fieldOfView = enemyData.FieldOfView;
        engageDistance = enemyData.EngageDistance;
        melleeDistance = enemyData.MelleeDistance;
        distanceTargetWeight = enemyData.DistanceTargetWeight;
        attackerTargetWeight = enemyData.AttackerTargetWeight;
        moveSpeed = enemyData.MoveSpeed;
    }
    #endregion
}