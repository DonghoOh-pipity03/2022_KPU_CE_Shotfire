using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;


#region 필요한 컴포넌트
[RequireComponent(typeof(PlayerInput))]
#endregion

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    PlayerInput playerInput;

    #region 전역 변수
    [Header("화면 이동 속도")]
    [SerializeField] float m_screenXSpeed = 1f; // X축 화면 이동속도
    [SerializeField] float m_screenYSpeed = 1f; // Y축 화면 이동속도
    [SerializeField] float m_screenNormalSpeed = 1f;    // 평상시 화면 이동속도 계수
    [SerializeField] float m_screenZoomSpeed = 0.5f;    // 줌화면 이동속도 계수
    [Header("화면 최대 각도")]
    [SerializeField] float m_minScreenAngle = 80f;  // 화면 최대 아래 각도
    [SerializeField] float m_maxScreenAngle = 80f;  //화면 최대 위 각도
    [Header("무기 반동")]
    [SerializeField] float snappiness; //  -> 클수록 반동이 과격해짐 
    [SerializeField] float returnIdleSpeed; // 기본 반동 회복 속도 -> 클수록 빨리 반동에서 회복됨
    [SerializeField] float returnFireSpeed; // 사격중 반동 회복 속도 -> 클수록 빨리 반동에서 회복됨
    [Header("카메라")] 
    Vector3 curCamHolderLocalPosition = Vector3.zero;   // 현재 카메라 홀더의 로컬 위치
    [SerializeField] float m_idleCamHolderHeight = 1.8f;    // 평소 상태 카메라 높이
    [SerializeField] float m_zoomCamHolderHeught = 1.55f;   // 줌 상태 카메라 높이
    [Header("무기")] 
    [SerializeField] Weapon[] weaponArray = new Weapon[2];   // 1~4번 슬릇에 사용할 무기 배열
    #endregion

    #region 전역 동작 변수
    // 카메라
    GameObject idleVirtualCam;
    GameObject zoomVirtualCam;
    Transform cameraHolder; // 카메라 거치대 자리(1티어), 입력에 따른 화면 회전 담당
    Transform recoil;  // 카메라 거치대의 자식(2티어), 사격에 의한 반동 담당
    // 화면
    float curScreenSpeed;   // 현재 화면 회전 속도
    bool isZoomMode = false;    // 줌 상태 여부
    // 무기 반동
    Vector3 currectRotation;    
    Vector3 targetRotation;
    // 무기
    int curWeapon = 0;   // 현재 들고 있는 무기의 배열 번호
    #endregion
#region 콜백함수
    private void Start()
    {
        cameraHolder = transform.Find("Camera Holder");
        recoil = cameraHolder.GetChild(0);
        idleVirtualCam = GameObject.Find("Idle Virtual Camera");
        idleVirtualCam.GetComponent<CinemachineVirtualCamera>().Follow = recoil.Find("idle pos").transform;
        zoomVirtualCam = GameObject.Find("Zoom Virtual Camera");
        zoomVirtualCam.GetComponent<CinemachineVirtualCamera>().Follow = recoil.Find("zoom pos").transform;

        // 하이어라키 상의 무기를 배열에 세팅
        foreach(Weapon weapon in weaponArray)
        {
            weapon.gameObject.SetActive(false);
        }
        weaponArray[curWeapon].gameObject.SetActive(true);

        if(!photonView.IsMine) return ; // 네트워크 통제 구역

        curScreenSpeed = m_screenNormalSpeed;
        curCamHolderLocalPosition.y = m_idleCamHolderHeight;
        cameraHolder.localPosition = curCamHolderLocalPosition;
        playerInput = GetComponent<PlayerInput>(); 
    }
    private void Update()
    {
        if(!photonView.IsMine) return ; // 이하 네트워크 통제 구역
        
        isZoomMode = playerInput.zoom;

        if(playerInput.fire) weaponArray[curWeapon].Fire();
        else weaponArray[curWeapon].Detached();
        
        if(playerInput.reload) 
        {
            weaponArray[curWeapon].Reload();
            playerInput.reload = false;
        }
        if(playerInput.fireMode) 
        {
            weaponArray[curWeapon].ChangeFireMode();
            playerInput.fireMode = false;
        }

        if(playerInput.zoom) weaponArray[curWeapon].SetFireState(false);
        else weaponArray[curWeapon].SetFireState(true);

        ChangeWeaponCommand();
    }
    private void FixedUpdate() 
    {
        if(!photonView.IsMine) return ; // 이하 네트워크 통제 구역
        RotateScreen();
        SetScreenMode(); 
        RecoilScreen();
    }
#endregion
#region 함수
    #region 화면
    // 화면 회전
    private void RotateScreen()
    {
        cameraHolder.rotation *= Quaternion.AngleAxis(playerInput.look.x * m_screenXSpeed * curScreenSpeed, Vector3.up);
        cameraHolder.rotation *= Quaternion.AngleAxis(playerInput.look.y * m_screenYSpeed * curScreenSpeed, Vector3.left);

        var angles = cameraHolder.localEulerAngles;
        var angle = cameraHolder.localEulerAngles.x;

        if (180 < angle && angle < 360 - m_minScreenAngle)
        {
            angles.x = 360 - m_minScreenAngle;
        }
        else if (m_maxScreenAngle < angle && angle < 180)
        {
            angles.x = m_maxScreenAngle;
        }

        angles.z = 0;

        cameraHolder.localEulerAngles = angles;
    }

    // 줌인/아웃 처리: 화면 모드에 따라 속도, 높이, 위치를 설정한다.
    private void SetScreenMode()
    {
        idleVirtualCam.SetActive(!isZoomMode);  // 가상카메라 On/Off

        // 화면 회전 속도, 카메라 홀더 높이 조절
        if (!isZoomMode)    // idle 상태
        {
            curScreenSpeed = m_screenNormalSpeed;
            curCamHolderLocalPosition.y = m_idleCamHolderHeight;
        }
        else    // zoom 상태
        {
           curScreenSpeed = m_screenZoomSpeed;
            curCamHolderLocalPosition.y = m_zoomCamHolderHeught;
        }
        cameraHolder.localPosition = curCamHolderLocalPosition;
        
    }
    // 무기 반동 처리
    // 출처: https://www.youtube.com/watch?v=geieixA4Mqc
    private void RecoilScreen()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, 
                        ( weaponArray[curWeapon].state == Weapon.State.shooting ? returnFireSpeed : returnIdleSpeed ) * Time.deltaTime);
        currectRotation = Vector3.Slerp(currectRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        recoil.transform.localRotation = Quaternion.Euler(currectRotation);
    }

    // 무기 반동 처리: 반동 방향을 입력받는다
    public void FireRecoil(Vector3 _recoilDir)
    {   
        targetRotation += _recoilDir;
    }
#endregion
    #region 무기
    private void ChangeWeaponCommand()
    {
        if(Input.GetButtonDown("Weapon1")) ChangeWeapon(0);
        else if(Input.GetButtonDown("Weapon2")) ChangeWeapon(1);
        else if(Input.GetButtonDown("Weapon3")) ChangeWeapon(2);
        else if(Input.GetButtonDown("Weapon4")) ChangeWeapon(3);
    }
    private void ChangeWeapon(int _newWeaponIndex)
    {
        weaponArray[curWeapon].gameObject.SetActive(false);
        curWeapon = _newWeaponIndex;
        weaponArray[curWeapon].gameObject.SetActive(true);

        weaponArray[curWeapon].UpdateUI();
    }
    #endregion
#endregion
}