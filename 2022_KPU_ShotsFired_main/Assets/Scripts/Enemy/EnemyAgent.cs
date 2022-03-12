using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AIState{ wait, move, engage}    // AI가 가질 수 있는 경계상태
class EnemyAgent : LivingEntity
{   
    [SerializeField] private Transform eyeTransform;    // 눈의 위치 정보
    #region 전역 변수
    // 시야
    [SerializeField] private float eyeDistance; // 시야 거리
    [SerializeField] private LayerMask attackTarget;    // 공격 및 타겟 대상 레이어
    [SerializeField] private float fieldOfView; // 시야각
    // 공격
    [SerializeField] private float engageDistance;  // 교전을 위한 이동 정지거리
    [SerializeField] private float melleeDistance; // 근접공격을 시작하는 거리
    [SerializeField] private float distanceTargetWeight;    // 공격 타겟 선정을 위한 거리별 가중치
    [SerializeField] private float attackerTargetWeight;    // 공격 타겟 선정을 위한 마지막 공격자 가중치
    #endregion
    #region 전역 동작변수
    private AIState curState;  // AI의 경계상태
    private int[] isPlayerOnSight = new int[4]; // 플레이어가 시야 내에 보이는지 여부
    private GameObject[] players = new GameObject[4];   // 플레이어들의 게임 오브젝트 정보
    private float[] playerDistance = {9999,9999,9999,9999};  // 각 플레이어와의 거리
    private int[] targetWeight = new int[4];    // 각 플레이어별 타겟팅(어그로) 가중치
    private int lastAttacker;   // 마지막으로 AI를 공격한 플레이어: 0은 판단불가 의미
    private Ray ray;    // 플레이어 탐색용 레이
    private int targetPlayer;   // 공격 대상: 0은 없음
    #endregion

    protected override void OnEnable() 
    {
        base.OnEnable();

        curState = AIState.wait;
    }

    private void FixedUpdate() 
    {
        SenseEntity();
        RenewPlayerInfo();

        if(curState != AIState.wait) 
        {
            AIStateBT();
            AIAttackBT();
        }
    }

    public override void TakeDamage(DamageMessage _damageMessage, HitParts _hitPart)
    {
        base.TakeDamage(_damageMessage, _hitPart);

        // 마지막 공격자를 저장
        var player = _damageMessage.attacker.GetComponent<PlayerController>();
        if( player != null) lastAttacker = player.ID;

        if(curState == AIState.wait)    // 데미지를 받으면 대기상태에서 해제
        {   
            var distance = (_damageMessage.attacker.transform.position - eyeTransform.position).magnitude;
            if(distance > engageDistance) curState = AIState.move;
            else curState = AIState.engage;
        }
    }

    // 시야내 모든 타겟 감지 
    private void SenseEntity()
    {   
        for(int i = 0; i < 4; i++)
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
                RaycastHit hit;
                var direction = collider.transform.position - eyeTransform.position;
                direction.y = eyeTransform.forward.y;
                
                // 시야각 내에 있음
                if (Vector3.Angle(direction, eyeTransform.forward) < fieldOfView * 0.5f)
                {   
                    ray = new Ray();
                    ray.origin = eyeTransform.position;
                    ray.direction = direction;

                    // 시야에 확인
                    if (Physics.Raycast(ray, out hit, eyeDistance))//, attackTarget))  
                    {   
                        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 5f);
                        if(hit.transform.root.gameObject == collider.gameObject) 
                        {
                            int ID = hit.transform.root.gameObject.GetComponent<PlayerController>().ID;
                            if(ID != 0)isPlayerOnSight[ ID -1 ] = 1;
                            
                            // 플레이어 정보 등록
                            if(players[ID-1] == null) players[ID-1] = hit.transform.root.gameObject;

                            // AI 상태 변경
                            if(curState == AIState.wait)
                            {
                                var distance = (hit.transform.position - eyeTransform.position).magnitude;
                                if(distance > engageDistance) curState = AIState.move;
                                else curState = AIState.engage;
                            }
                        }
                    }
                }
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var leftRayRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
        var leftRayDirection = leftRayRotation * transform.forward;
        Handles.color = new Color(1f, 1f, 1f, 0.2f);
        Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, eyeDistance);
    }
    #endif

    // 각 플레이어와의 거리, 타겟 가중치 계산
    private void RenewPlayerInfo()
    {
        for(int i=0; i < 4 ; i++)
        {
            if( players[i] == null) continue;
            
            playerDistance[i] = (players[i].transform.position - transform.position).magnitude; // 거리 계산

            targetWeight[i] = (int)(
                                Mathf.Clamp( ( -1 * playerDistance[i] +25 ), 0, 25 ) * distanceTargetWeight 
                                + ( (lastAttacker == i+1) ? attackerTargetWeight : 0 ) );    // 타겟 가중치 계산
        }
    }   

    // AI 행동트리_상태
    private void AIStateBT()
    {
        curState = AIState.move;
        foreach(var i in playerDistance)
        {
            if( i < engageDistance)
            {   
                curState = AIState.engage;
                break;
            }
        }
    }
    // AI 행동트리_공격
    private void AIAttackBT()
    {
        // 공격 타겟 선정
        int maxValue = targetWeight.Max();
        targetPlayer = targetWeight.ToList().IndexOf(maxValue);
        // 공격 방법 선정 & 공격
        if( playerDistance[targetPlayer] > melleeDistance ) MelleeAttack();    //근접공격
        else ShotAttack();  // 원거리 공격
    }

    private void MelleeAttack()
    {

    }

    private void ShotAttack()
    {

    }
}