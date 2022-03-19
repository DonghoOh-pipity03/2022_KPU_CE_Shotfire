﻿using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class PlayerInput : MonoBehaviourPunCallbacks
{
    private PlayerController playerController;
    #region 입력 변수
    // 메모: get set 패턴이 아닌 변수들은, 사용하는 클래스에서 사용후 true, false로 조작해 줘야한다.
    public Vector2 move { get; private set; }
    public Vector2 look { get; private set; }
    public bool jump;
    public bool sprint { get; private set; }
    public bool dodge;
    public bool crouch { get; private set; }
    public bool fire { get; private set; }
    public bool zoom { get; private set; }
    public float mouseWheel { get; private set; }
    public bool interaction { get; private set; }
    public bool reload;
    public bool melee;
    public bool frag;
    public bool fireMode;
    public bool weapon1;
    public bool weapon2;
    public bool weapon3;
    public bool weapon4;
    #endregion

    [SerializeField] private float m_sprintInvokeTime = 0.25f;  // 질주 감지 시간

    #region 전역 동작 변수
    private float m_lastSprintInputTime = 0f;   // 마지막 질주 키 입력 시간
    private float m_curSprintInputTime = 0f;    // 현재 질주 키 입력 시간: 나중에 '마지막 질주 키 입력시간'으로 복사됨
    #endregion

    private void Start() 
    {
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if(!photonView.IsMine)
        {
            return ;
        }
        DetectInput();
        //SelfDebug();
    }

    private void DetectInput() 
    {
        // 이동 관련
        if(playerController.ableControlMove)
        {
        move = (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))).normalized;
        if( Input.GetButtonDown("Jump")) jump = true;
        sprint = DetectSprint();
        if( Input.GetButtonDown("Dodge")) dodge = true;
        crouch = Input.GetButton("Crunch");
        }
        else
        {
            move = Vector2.zero;
            jump = false;
            sprint = false;
            dodge = false;
            crouch = false;
        }

        // 공격 관련
        if(playerController.ableControlAttack)
        {
        fire = Input.GetButton("Fire1");
        zoom = Input.GetButton("Fire2");
        if( Input.GetButtonDown("Reload")) reload = true;
        if( Input.GetButtonDown("Melee")) melee = true;
        if( Input.GetButtonDown("Frag")) frag = true;
        if( Input.GetButtonDown("Firemode")) fireMode = true;
        if( Input.GetButtonDown("Weapon1")) weapon1 = true;
        if( Input.GetButtonDown("Weapon2")) weapon2 = true;
        if( Input.GetButtonDown("Weapon3")) weapon3 = true;
        if( Input.GetButtonDown("Weapon4")) weapon4 = true;
        }
        else
        {
            fire = false;
            zoom = false;
            reload = false;
            melee = false;
            frag = false;
            fireMode = false;
            weapon1 = false;
            weapon2 = false;
            weapon3 = false;
            weapon4 = false;
        }

        // 화면 관련
        look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        //기타
        mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        interaction = Input.GetButton("Interaction");
        }

    private bool DetectSprint() // UNITY_STANDALONE 전용_다른 장치는 별도 구현 필요
    {
        if (!Input.GetKey(KeyCode.W)) return false;
        if (sprint == true) return true;

        // 달리기 조건 감지
        if(m_curSprintInputTime!=Mathf.Infinity) m_lastSprintInputTime = m_curSprintInputTime;
        m_curSprintInputTime = Mathf.Infinity;
        if (Input.GetKeyDown(KeyCode.W)) m_curSprintInputTime = Time.time;
        if (m_curSprintInputTime <= m_lastSprintInputTime + m_sprintInvokeTime) return true;
        else return false;
    }

    void SelfDebug() 
    {
        if (jump) Debug.Log("jump");
        if (dodge) Debug.Log("dodge");
        if (crouch) Debug.Log("crunch");
        if (fire) Debug.Log("fire");
        if (zoom) Debug.Log("zoom");
        if (reload) Debug.Log("reload");
        if (interaction) Debug.Log("interaction");
        if (melee) Debug.Log("melee");
        if (frag) Debug.Log("frag");
        if (fireMode) Debug.Log("firemode");
        if (weapon1) Debug.Log("weapon1");
        if (weapon2) Debug.Log("weapon2");
        if (weapon3) Debug.Log("weapon3");
        if (weapon4) Debug.Log("weapon4");
        Debug.Log(mouseWheel);
    }
}
