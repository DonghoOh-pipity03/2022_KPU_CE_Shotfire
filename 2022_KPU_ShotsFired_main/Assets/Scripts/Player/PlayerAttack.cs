/*
최초작성자: 오동호
최종수정자: 오동호
수정날짜: 20220217
버전: 0.01.00
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 필요한 컴포넌트
[RequireComponent(typeof(PlayerInput))]
#endregion

public class PlayerAttack : MonoBehaviour
{
    private PlayerInput playerInput;

    #region 전역 변수
    [Header("카메라 세팅")]
    [SerializeField] Transform m_cameraHolder; // 카메라 거치대 자리
    [SerializeField] float m_screenXSpeed = 1f; // X축 화면 이동속도
    [SerializeField] float m_screenYSpeed = 1f; // Y축 화면 이동속도
    [SerializeField] float m_screenNormalSpeed = 1f;    // 평상시 화면 이동속도 계수
    [SerializeField] float m_screenZoomSpeed = 0.5f;    // 줌화면 이동속도 계수
    [SerializeField] float m_minScreenAngle = 80f;  // 화면 최대 아래 각도
    [SerializeField] float m_maxScreenAngle = 80f;  //화면 최개 위 각도
    [SerializeField] GameObject m_virtualMainCamera;    // 메인 카메라 자리
    
    [Header("카메라 거치대 위치")] 
    [SerializeField] private float m_idleCamHolderHeight = 1.8f;
    [SerializeField] private float m_zoomCamHolderHeught = 1.55f;
    #endregion

    #region 전역 동작 변수
    private float curScreenSpeed;
    private Vector3 curCamHolderLocalPosition = Vector3.zero;
    private bool isZoomMode = false;
    #endregion

    private void Start()
    {
        curScreenSpeed = m_screenNormalSpeed;
        curCamHolderLocalPosition.y = m_idleCamHolderHeight;
        m_cameraHolder.localPosition = curCamHolderLocalPosition;
        playerInput = GetComponent<PlayerInput>();
    }
    private void Update()
    {
        RotateScreen();

        isZoomMode = playerInput.zoom;
    }

    private void FixedUpdate() {
        SetScreenMode();
    }

    // 화면 회전
    private void RotateScreen()
    {
        m_cameraHolder.rotation *= Quaternion.AngleAxis(playerInput.look.x * m_screenXSpeed * curScreenSpeed, Vector3.up);
        m_cameraHolder.rotation *= Quaternion.AngleAxis(playerInput.look.y * m_screenYSpeed * curScreenSpeed, Vector3.left);

        var angles = m_cameraHolder.localEulerAngles;
        var angle = m_cameraHolder.localEulerAngles.x;

        if (180 < angle && angle < 360 - m_minScreenAngle)
        {
            angles.x = 360 - m_minScreenAngle;
        }
        else if (m_maxScreenAngle < angle && angle < 180)
        {
            angles.x = m_maxScreenAngle;
        }

        angles.z = 0;

        m_cameraHolder.localEulerAngles = angles;
    }

    // 줌 인/아웃 처리
    private void SetScreenMode()
    {
        m_virtualMainCamera.SetActive(!isZoomMode);

        if (!isZoomMode)    // idle 상태
        {
            curScreenSpeed = m_screenNormalSpeed;

            curCamHolderLocalPosition.y = m_idleCamHolderHeight;
            m_cameraHolder.localPosition = curCamHolderLocalPosition;
        }
        else    // zoom 상태
        {
           curScreenSpeed = m_screenZoomSpeed;

            curCamHolderLocalPosition.y = m_zoomCamHolderHeught;
            m_cameraHolder.localPosition = curCamHolderLocalPosition;
        }
    }

    
}