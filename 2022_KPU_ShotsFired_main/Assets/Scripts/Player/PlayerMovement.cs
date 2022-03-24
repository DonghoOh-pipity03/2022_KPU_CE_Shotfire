using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
#region 필요한 컴포넌트
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
#endregion

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    private CharacterController charController;
    private PlayerInput playerInput;

    #region 전역 변수
    [Header("기본 이동")]
    [SerializeField] float m_walkFrontVelo = 1f;    // 앞 이동속도
    [SerializeField] float m_runVelo = 2.5f;    // 질주 이동속도
    [SerializeField] float m_walkSideRat = 0.7f;    // 앞과 옆뒤 이동속도 비율
    [SerializeField] float m_speedSmoothTime = 0.05f;   // 이동속도 스무스 시간

    [Header("카메라와 하체정렬")]
    [SerializeField] Transform m_camCradle; // 카메라 거치대 자리
    [SerializeField] float m_minMovementLowerBodyArrange = 0.1f;    // 상체하체정렬을 위해 필요한 최소한의 움직임

    [Header("점프")]
    [SerializeField] float m_jumpPower = 5; // 점프 속도
    [SerializeField] float m_gravityMultiple = 1;   // 점프시 중력 계수

    [Header("회피")]
    [SerializeField] float m_maxDodgeCount = 3; // 최대 회피 개수
    [SerializeField] float m_dodgeRecoverPerSecond = 0.66f; // 초당 회피 개수 회복량
    [SerializeField] float m_dodgeSpeed = 10;   // 회피 중 이동 속도
    [SerializeField] float m_dodgeTime = 0.1f;  // 회피 지속시간

    [Header("앉기")]
    [SerializeField] float m_CrouchSpeed;   // 앉기 중 이동 속도
    [SerializeField] float m_CrouchHeightRatio;  // 앉기 중 키 비율
    #endregion

    #region 전역 동작 변수
    // 기본 이동
    private Vector3 tarDirect;  // 목표이동방향
    private float tarVelo;  // 목표이동속도
    private bool isRun; // 질주 여부
    private float currentVelocityY; // 목표 y축 속도
    private float speedSmoothVelocity;  // 이동속도 변환의 부드러운 정도
    private float curSpeed =>
        new Vector2(charController.velocity.x, charController.velocity.z).magnitude;    // 현재 캐릭터 속도
    // 회피
    private float m_dodgeCount = 3;    // 현재 남은 회피 개수
    private bool isDodge = false;   // 회피 상태 여부
    private float lastDodgeTime = 0;    //마지막 회피 입력시간
    // 앉기
    private bool isCrouch = false;  // 앉기 상태 여부
    #endregion
#region 콜백함수
    private void Start()
    {
        charController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void FixedUpdate()
    {
        if(!photonView.IsMine) return ; // 네트워크 통제 구역

        if (DetectRotationGap() || charController.velocity.magnitude > m_minMovementLowerBodyArrange) ArrangeLowerBody();
        if (playerInput.fire == true || playerInput.zoom == true) ArrangeLowerBody();

        if (playerInput.jump) Jump();
        Dodge();
        Crouch();
        Move();
    }

#endregion
#region 함수
    private void Move()
    {
        // 목표 이동속도 결정
        if (isDodge) tarVelo = m_dodgeSpeed;
        else if (isCrouch) tarVelo = m_CrouchSpeed;
        else
        {
            isRun = playerInput.sprint;
            var angle = Mathf.Abs(Vector2.Angle(Vector2.up, playerInput.move));
            if (angle <= 45)
                tarVelo = isRun ? m_runVelo * playerInput.move.magnitude : m_walkFrontVelo * playerInput.move.magnitude;
            else tarVelo = m_walkFrontVelo * m_walkSideRat * playerInput.move.magnitude;
        }
        // 목표 이동방향 결정
        tarDirect = Vector3.Normalize(transform.forward * playerInput.move.y + transform.right * playerInput.move.x);

        tarVelo = Mathf.SmoothDamp(curSpeed, tarVelo, ref speedSmoothVelocity, m_speedSmoothTime);
        currentVelocityY += Time.deltaTime * Physics.gravity.y * m_gravityMultiple;

        var velocity = tarDirect * tarVelo + Vector3.up * currentVelocityY;

        charController.Move(velocity * Time.deltaTime);

        if (charController.isGrounded) currentVelocityY = 0;
    }
    private bool DetectRotationGap()
    {
        var angle = Mathf.Abs(m_camCradle.localEulerAngles.y);
        if (angle > 180) angle = angle * -1 + 360;
        if (angle > 90) return true;
        else return false;
    }

    private void ArrangeLowerBody()
    {
        this.transform.rotation = Quaternion.Euler(0, m_camCradle.eulerAngles.y, 0);

        var angles = m_camCradle.localEulerAngles;
        m_camCradle.localEulerAngles = new Vector3(angles.x, 0, 0);
    }

    private void Jump()
    {   
        playerInput.jump = false;
        if (!charController.isGrounded) return;
        currentVelocityY = m_jumpPower;
    }

    private void Dodge()
    {   
        // 회피의 (비)활성 처리
        if (playerInput.dodge)
        {
            playerInput.dodge = false;
            
            if (m_dodgeCount >= 1 && charController.velocity.magnitude > 0 && charController.isGrounded)
            {
                m_dodgeCount--;
                isDodge = true;
                lastDodgeTime = Time.time;
            }
        }
        else if (isDodge == true)
        {
            if (Time.time > lastDodgeTime + m_dodgeTime)
            {
                isDodge = false;
            }
        }
        // 회피 개수 회복 처리
        if (m_dodgeCount < m_maxDodgeCount)
        {
            m_dodgeCount += m_dodgeRecoverPerSecond * Time.deltaTime;
            if (m_dodgeCount > m_maxDodgeCount)
            {
                m_dodgeCount = m_maxDodgeCount;
            }
        }
    }

    private void Crouch() {
        if (playerInput.crouch && !isCrouch)
        {
            isCrouch = true;
            charController.height *= m_CrouchHeightRatio;
            charController.center *= m_CrouchHeightRatio;
        }
        else if(!playerInput.crouch &&isCrouch)
        {
            Ray crouchRay = new Ray(transform.position + Vector3.up * charController.radius, Vector3.up);
            float crouchRayLength = (charController.height / m_CrouchHeightRatio) - charController.radius;
            if (Physics.SphereCast(crouchRay, charController.radius, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                return; // 설 수 없는 곳 감지시 true유지
            }
            else
            {
                charController.height /= m_CrouchHeightRatio;
                charController.center /= m_CrouchHeightRatio;
                isCrouch = false;
            }
        }
    }
#endregion
}
