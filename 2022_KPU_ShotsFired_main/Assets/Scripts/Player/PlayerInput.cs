using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    #region 입력 변수
    public Vector2 move { get; private set; }
    public Vector2 look { get; private set; }
    public bool jump { get; private set; }
    public bool sprint { get; private set; }
    public bool dodge { get; private set; }
    public bool crouch { get; private set; }
    public bool fire { get; private set; }
    public bool zoom { get; private set; }
    public float mouseWheel { get; private set; }
    public bool reload { get; private set; }
    public bool interaction { get; private set; }
    public bool melee { get; private set; }
    public bool frag { get; private set; }
    public bool fireMode { get; private set; }
    public bool weapon1 { get; private set; }
    public bool weapon2 { get; private set; }
    public bool weapon3 { get; private set; }
    public bool weapon4 { get; private set; }
    #endregion

    [SerializeField] private float m_sprintInvokeTime = 0.25f;  // 질주 감지 시간

    #region 전역 동작 변수
    private float m_lastSprintInputTime = 0f;   // 마지막 질주 키 입력 시간
    private float m_curSprintInputTime = 0f;    // 현재 질주 키 입력 시간: 나중에 '마지막 질주 키 입력시간'으로 복사됨
    #endregion

    private void Update()
    {
        DetectInput();
        //SelfDebug();
    }

    private void DetectInput() 
    {
        move = (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))).normalized;
        look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        jump = Input.GetButtonDown("Jump");
        sprint = DetectSprint();
        dodge = Input.GetButtonDown("Dodge");
        crouch = Input.GetButton("Crunch");
        fire = Input.GetButton("Fire1");
        zoom = Input.GetButton("Fire2");
        mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        reload = Input.GetButtonDown("Reload");
        interaction = Input.GetButton("Interaction");
        melee = Input.GetButtonDown("Melee");
        frag = Input.GetButtonDown("Frag");
        fireMode = Input.GetButtonDown("Firemode");
        weapon1 = Input.GetButtonDown("Weapon1");
        weapon2 = Input.GetButtonDown("Weapon2");
        weapon3 = Input.GetButtonDown("Weapon3");
        weapon4 = Input.GetButtonDown("Weapon4");
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
