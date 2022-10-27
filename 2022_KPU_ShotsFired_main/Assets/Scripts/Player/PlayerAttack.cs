/*
PlayerAttack.cs
1. 카메라 이동, 회전: 캐릭터의 위치에 따라 카메라를 이동하고, 마우스 입력에 따라 카메라를 회전한다. 
2. 카메라 콜라이더: 벽 레이어와의 충돌을 감지하고, 카메라의 위치를 앞뒤로 조정한다.
3. 무기조작: (1) 무기(Weapon.cs)를 등록 및 조작한다. (2) 무기의 반동을 입력받아 카메라에 반동을 구현한다.
4. 레이저 그리기: 라인렌더러를 이용하여 레이저의 궤적을 그리고, LaserPointer 오브젝트의 위치를 조작하여 레이저의 끝부분을 그린다.
5. 상호작용: 상호작용 레이어의 트리거와 반응하여, 상호작용 기능을 사용한다.
*/
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInput))]

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    PlayerInput playerInput;
    PlayerMovement playerMovement;
    LineRenderer lineRenderer;
    #region 전역 변수
    [Header("화면")]
    [Tooltip("X축 화면 이동속도")]
    [SerializeField] float m_screenXSpeed = 1f;
    [Tooltip("Y축 화면 이동속도")]
    [SerializeField] float m_screenYSpeed = 1f;
    [Tooltip("평상시 화면 이동속도 계수")]
    [SerializeField] float m_screenNormalSpeed = 1f;
    [Tooltip("줌화면 이동속도 계수")]
    [SerializeField] float m_screenZoomSpeed = 0.5f;
    [Tooltip("카메라 콜라이더에 의한 앞뒤 움직임의 부드러움")]
    [SerializeField] float cameraPoleSpeed;
    [Tooltip("화면 최대 아래 각도")]
    [SerializeField] float m_minScreenAngle = 80f;
    [Tooltip("화면 최대 위 각도")]
    [SerializeField] float m_maxScreenAngle = 80f;

    [Header("카메라")]
    [Tooltip("평소 상태 카메라 높이")]
    [SerializeField] float m_idleCamHolderHeight = 1.8f;
    [Tooltip("줌 상태 카메라 높이")]
    [SerializeField] float m_zoomCamHolderHeught = 1.55f;
    [Tooltip("카메라 콜라이더가 감지할 레이어")]
    [SerializeField] LayerMask cameraCollider;
    [Tooltip("카메라의 높이를 변환할 때, 변환 속도")]
    [SerializeField] float cameraHeightSmooth = 0.1f;
    [Tooltip("캐릭터가 앉을 때, 평소 상태 카메라의 높이")]
    [SerializeField] float crouchIdleCameraHolderHeight = 1.35f;
    [Tooltip("캐릭터가 앉을 때, 줌 상태 카메라의 높이")]
    [SerializeField] float crouchZoomCameraHolderHeight = 1.1f;

    [Header("무기")]
    [Tooltip("키보드 1~4번에 사용할 무기를 배치하는 배열")]
    [SerializeField] Weapon[] weaponArray = new Weapon[2];
    [Tooltip("클수록 반동이 과격해짐")]
    [SerializeField] float snappiness;
    [Tooltip("기본 반동 회복 속도, 클수록 반동에서 빨리 회복됨")]
    [SerializeField] float returnIdleSpeed;
    [Tooltip("사격중 반동 회복 속도, 클수록 반동에서 빨리 회복됨")]
    [SerializeField] float returnFireSpeed;
    [Tooltip("조준점에 사용할 레이캐스트의 최대 거리")]
    [SerializeField] float maxGunRayDistance = 100;
    [Tooltip("조준점 레이캐스트의 최소 유효거리")]
    [SerializeField] float minAimDistance = 1.5f;
    [Tooltip("조준점 위치 변경시 변경에 걸리는 시간")]
    [SerializeField] float aimPointSmooth = 0.1f;
    [Tooltip("조준 감지에 사용할 레이어마스크")]
    [SerializeField] LayerMask aimLayer;
    [Tooltip("레이저 조준에 사용할 레이어마스크")]
    [SerializeField] LayerMask laserLayer;
    [Tooltip("레이저 포인터의 끝부분")]
    [SerializeField] Transform laserPointer;
    #endregion

    #region 전역 동작 변수
    // 카메라
    GameObject idleVirtualCam;  // 기본상태에서 사용할 버추얼 카메라 
    GameObject zoomVirtualCam;  // 줌상태에서 사용할 버추얼 카메라 
    Transform cameraHolder; // 카메라 거치대 자리(1티어), 입력에 따른 화면 회전 담당
    Transform recoil;  // 카메라 거치대의 자식(2티어), 사격에 의한 반동 담당
    Transform cameraPole;   // 카메라 거치대의 자식(3티어), 캐릭터와 카메라 간의 거리 담당
    Camera mainCamera;  // 메인 카메라
    RaycastHit hit; // 카메라 콜라이더용
    Vector3 tarCameraPolePosition;  // 3티어 카메라 거치대의 목표 로컬위치
    Vector3 tarCamHolderLocalPosition;   // 목표 카메라 홀더의 로컬 위치_y값만 사용하는 홀더

    // 화면
    float curScreenSpeed => !isZoomMode ? m_screenNormalSpeed : m_screenZoomSpeed;   // 현재 화면 회전 속도
    bool isZoomMode => playerInput.zoom;    // 줌 상태 여부

    // 무기 반동
    Vector3 currectRotation;    // 2티어 카메라 거치대의 현재 회전량
    Vector3 targetRotation; // // 2티어 카메라 거치대의 목표 회전량

    // 무기
    int curWeapon = 0;   // 현재 들고 있는 무기의 배열 번호
    bool isInterActionning; // 상호작용 중인지
    float lastInterActionBeginTime;  // 마지막 상호작용 시작 시간

    // 조준
    public Transform aimTarget;    // 캐릭터가 조준하는 실제위치
    Vector3 tarAimPoint;    // 캐릭터가 조준하는 목표위치
    RaycastHit aimHit; // 화면 가운데에 레이를 쏴서 검출되는 정보
    Ray ray;    // 화면 가운데에 쏠 레이
    Vector3 screenCenter => new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);   // 화면 가운데 지점 픽셀정보
    #endregion


    #region 콜백함수
    public override void OnEnable()
    {
        base.OnEnable();
        Application.onBeforeRender += DrawLaser;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        Application.onBeforeRender -= DrawLaser;
    }
    private void Start()
    {
        if (!photonView.IsMine) return; // 이하 네트워크 통제 구역

        playerMovement = GetComponent<PlayerMovement>();
        lineRenderer = GetComponent<LineRenderer>();

        aimTarget = transform.Find("AimPoint");

        cameraHolder = transform.Find("Camera Holder");
        recoil = cameraHolder.GetChild(0);
        cameraPole = recoil.GetChild(0);
        idleVirtualCam = GameObject.Find("Idle Virtual Camera");
        idleVirtualCam.GetComponent<CinemachineVirtualCamera>().Follow = cameraPole.Find("idle pos").transform;
        zoomVirtualCam = GameObject.Find("Zoom Virtual Camera");
        zoomVirtualCam.GetComponent<CinemachineVirtualCamera>().Follow = cameraPole.Find("zoom pos").transform;

        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        // 하이어라키 상의 무기를 배열에 세팅
        foreach (Weapon weapon in weaponArray)
        {
            weapon.gameObject.SetActive(false);
        }
        weaponArray[curWeapon].gameObject.SetActive(true);

        tarCamHolderLocalPosition.y = m_idleCamHolderHeight;
        cameraHolder.localPosition = tarCamHolderLocalPosition;
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return; // 이하 네트워크 통제 구역

        if (playerInput.fire) weaponArray[curWeapon].Fire(0);
        else weaponArray[curWeapon].Detached();

        if (playerInput.reload)
        {
            weaponArray[curWeapon].Reload();
            playerInput.reload = false;
        }
        if (playerInput.fireMode)
        {
            weaponArray[curWeapon].ChangeFireMode();
            playerInput.fireMode = false;
        }

        if (playerInput.zoom) weaponArray[curWeapon].SetFireState(false);
        else weaponArray[curWeapon].SetFireState(true);

        ChangeWeaponCommand();
    }

    private void LateUpdate()
    {
        DrawLaser();
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine) return; // 이하 네트워크 통제 구역
        RotateScreen();
        SetScreenHeight();
        RecoilScreen();
        CameraCollider();
        if (aimTarget != null) Aim();
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

    // 카메라 높이 처리: 화면 모드와 캐릭터 앉기여부에 따라 속도, 높이, 위치를 설정한다.
    private void SetScreenHeight()
    {
        idleVirtualCam.SetActive(!isZoomMode);  // 가상카메라 On/Off

        // 카메라 높이 세팅_앉기, 기본사격, 줌사격
        if (playerMovement.isCrouch) // 앉기 상태
        {
            if (!isZoomMode) tarCamHolderLocalPosition.y = crouchIdleCameraHolderHeight;
            else tarCamHolderLocalPosition.y = crouchZoomCameraHolderHeight;
        }
        else    // 서있는 상태
        {
            if (!isZoomMode) tarCamHolderLocalPosition.y = m_idleCamHolderHeight;
            else tarCamHolderLocalPosition.y = m_zoomCamHolderHeught;
        }

        tarCamHolderLocalPosition.y = Mathf.Lerp(cameraHolder.localPosition.y, tarCamHolderLocalPosition.y, cameraHeightSmooth);
        cameraHolder.localPosition = tarCamHolderLocalPosition;
    }

    // 무기 반동 처리
    // 출처: https://www.youtube.com/watch?v=geieixA4Mqc
    private void RecoilScreen()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero,
                        (weaponArray[curWeapon].state == Weapon.State.shooting ? returnFireSpeed : returnIdleSpeed) * Time.deltaTime);
        currectRotation = Vector3.Slerp(currectRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        recoil.transform.localRotation = Quaternion.Euler(currectRotation);
    }

    // 무기 반동 처리: 반동 방향을 입력받는다
    public void FireRecoil(Vector3 _recoilDir)
    {
        targetRotation += _recoilDir;
    }

    // 카메라 콜라이더
    private void CameraCollider()
    {
        Debug.DrawRay(recoil.position, cameraPole.forward * -1.5f, Color.red, 1f);
        if (Physics.SphereCast(recoil.position, 0.15f, cameraPole.forward * -1, out hit, 1.25f, cameraCollider, QueryTriggerInteraction.Ignore))
        {
            tarCameraPolePosition = new Vector3(0, 0, -1 * hit.distance + 0.25f);
        }
        else
        {
            tarCameraPolePosition = new Vector3(0, 0, -1.25f);
        }

        cameraPole.localPosition = Vector3.Lerp(cameraPole.localPosition, tarCameraPolePosition, cameraPoleSpeed * Time.deltaTime);
    }

    // 카메라 가운데 지점을 조준점으로 설정한다.
    void Aim()
    {
        ray = mainCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out hit, maxGunRayDistance, aimLayer))
        {
            if (hit.distance > minAimDistance) tarAimPoint = hit.point;
            else tarAimPoint = mainCamera.transform.position + mainCamera.transform.forward * maxGunRayDistance;
        }
        else tarAimPoint = mainCamera.transform.position + mainCamera.transform.forward * maxGunRayDistance;

        aimTarget.position = Vector3.Lerp(aimTarget.position, tarAimPoint, aimPointSmooth);
    }
    #endregion


    #region 무기
    private void ChangeWeaponCommand()
    {
        if (Input.GetButtonDown("Weapon1")) ChangeWeapon(0);
        else if (Input.GetButtonDown("Weapon2")) ChangeWeapon(1);
        else if (Input.GetButtonDown("Weapon3")) ChangeWeapon(2);
        //else if(Input.GetButtonDown("Weapon4")) ChangeWeapon(3);
    }

    private void ChangeWeapon(int _newWeaponIndex)
    {
        weaponArray[curWeapon].gameObject.SetActive(false);
        curWeapon = _newWeaponIndex;
        weaponArray[curWeapon].gameObject.SetActive(true);

        weaponArray[curWeapon].UpdateUI();
    }

    // 상호작용 처리
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "InterAction" && other.transform.root.gameObject != this.gameObject)
        {
            var interactionAgent = other.GetComponent<InterActionAgent>();
            if (interactionAgent.interActionComplete == false)
            {
                if (photonView.IsMine) GameUIManager.Instance.SetActiveInterAction(true);

                if (playerInput.interaction == true)
                {
                    // 시작
                    if (isInterActionning == false)
                    {
                        isInterActionning = true;
                        lastInterActionBeginTime = Time.time;
                    }
                    // 진행중
                    else
                    {
                        if (Time.time > lastInterActionBeginTime + interactionAgent.interActionTime)
                        {
                            interactionAgent.interActionComplete = true;
                            if (photonView.IsMine) GameUIManager.Instance.SetActiveInterAction(false);
                        }
                    }
                }
                else isInterActionning = false;
            }
            else if (photonView.IsMine) GameUIManager.Instance.SetActiveInterAction(false);
        }
    }

    // 상호작용 처리
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "InterAction")
        {
            isInterActionning = false;
            if (photonView.IsMine) GameUIManager.Instance.SetActiveInterAction(false);
        }
    }

    // 레이저 그리기
    private void DrawLaser()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, weaponArray[curWeapon].muzzlePosition.position);
            ray.origin = weaponArray[curWeapon].muzzlePosition.position;
            ray.direction = weaponArray[curWeapon].muzzlePosition.forward;
            if (Physics.Raycast(ray, out hit, maxGunRayDistance, laserLayer))
            {
                lineRenderer.SetPosition(1, hit.point);
                laserPointer.position = hit.point + weaponArray[curWeapon].muzzlePosition.forward * -0.01f;
            }
            else
            {
                lineRenderer.SetPosition(1, weaponArray[curWeapon].muzzlePosition.position + weaponArray[curWeapon].muzzlePosition.forward * maxGunRayDistance);
                laserPointer.position = weaponArray[curWeapon].muzzlePosition.position + weaponArray[curWeapon].muzzlePosition.forward * maxGunRayDistance
                                        + weaponArray[curWeapon].muzzlePosition.forward * -0.01f; ;
            }
        }
    }
    #endregion
    #endregion
}