using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    #region 전역 변수
    [Header("UI 씬")]
    [SerializeField] GameObject player_UI;
    [SerializeField] GameObject report_UI;
    [SerializeField] GameObject boss_UI;
    [SerializeField] private GameObject m_miniMap;  // 플레이어_ 미니맵UI 오브젝트 자리

    [Header("주인공 캐릭터 체력 계열")]
    [SerializeField] private RectTransform playerHealthBackground;  // 플레이어_체력 GUI 배경
    [SerializeField] private RectTransform playerMaxHealthBar;  // 플레이어_최대 체력 GUI
    [SerializeField] private RectTransform playerCurHealthBar;  // 플레이어_현재 체력 GUI
    [SerializeField] private TextMeshProUGUI playerHealth;  // 플레이어_체력 텍스트
    private float playerHealthGUIRatio => playerMaxHealthBar.sizeDelta.x / 100f; // 플레이어_체력 포인트와 체력 GUI 바의 비율 
    private float playerHealthBackgorundHorizonMargine;    // 플레이어_체력 배경 GUI 가로 마진
                                                           // 피격 필터
    [SerializeField] Image playerDamaged;    // 플레이어_피격시 핏빛 이미지
    [SerializeField] float playerDamageRestoreTime;   // 플레이어_피격 이미지가 옅어지는 시간
    IEnumerator playerDamagedFadeOut;    // 플레이어_피격 이미지를 옅게 할 함수명
    [Header("플레이어 무기 계열")]
    [SerializeField] private Animator aimAnimator;  // 에임 애니메이터
    [SerializeField] private Image[] hitMarker; // 히트 마커
    [SerializeField] private float hitMarkTime; // 히트 마커 지속시간
    IEnumerator hitMarkerIEnumerator;   // 히트 마커 표시함수
    [SerializeField] private TextMeshProUGUI remainAmmo;   // 플레이어_남은 탄환
    [SerializeField] private TextMeshProUGUI remainMag;    // 플레이어_남은 탄창
    [SerializeField] GameObject InterAction;    // 플레이어_상호작용 오브젝트
    [Header("보스 계열")]
    [SerializeField] private TextMeshProUGUI bossName;  // 보스_이름
    [SerializeField] private RectTransform bossHealthBackground;    // 보스_체력바 배경
    [SerializeField] private RectTransform bossHealthImage; // 보스_체력바
    [SerializeField] private TextMeshProUGUI bossHealthText;  // 보스_체력 텍스트

    #endregion
    //싱글톤
    private static GameUIManager instance;
    public static GameUIManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameUIManager>();
            return instance;
        }
    }
    #region 콜백함수
    private void Awake()
    {
        if (playerHealthBackground != null && playerMaxHealthBar != null)
            playerHealthBackgorundHorizonMargine = (playerHealthBackground.sizeDelta.x - playerMaxHealthBar.sizeDelta.x) / 2f;
    }
    #endregion
    #region 함수
    #region UI 씬
    public void SetActivePlayer(bool _active)
    {
        player_UI.SetActive(_active);
    }
    public void SetActiveReport(bool _active)
    {
        report_UI.SetActive(_active);
    }
    public void SetActiveMiniMap(bool _active)
    {
        m_miniMap.SetActive(_active);
    }
    public void SetActiveBoss(bool _active)
    {
        boss_UI.SetActive(_active);
    }
    #endregion
    #region 주인공 캐릭터 체력
    public void UpdatePlayerHealth(float _playerHealth, float _maxHealth)
    {
        playerHealth.text = _playerHealth + "/" + _maxHealth;
        playerCurHealthBar.sizeDelta = new Vector2(_playerHealth * playerHealthGUIRatio,
                                                playerCurHealthBar.sizeDelta.y);
    }

    public void SetActviePlayerDamaged(bool _active)
    {
        if (_active)
        {
            if (playerDamagedFadeOut != null) StopCoroutine(playerDamagedFadeOut);
            Color c = playerDamaged.color;
            c.a = 1;
            playerDamaged.color = c;
        }
        else
        {
            playerDamagedFadeOut = FadeOut(playerDamaged, playerDamageRestoreTime);
            StartCoroutine(playerDamagedFadeOut);
        }
    }

    #endregion
    #region 플레이어 무기
    public void UpdateAim(float _spread)
    {
        aimAnimator.SetFloat("Spread", _spread);
    }
    public void UpdateHitMark()
    {
        if (hitMarkerIEnumerator != null) StopCoroutine(hitMarkerIEnumerator);
        hitMarkerIEnumerator = HitMarker();
        StartCoroutine(hitMarkerIEnumerator);
    }
    IEnumerator HitMarker()
    {
        float i = 1;
        foreach (var j in hitMarker)
        {
            Color c = j.color;
            c.a = 1;
            j.color = c;
        }

        while (i > 0)
        {
            i -= 1 / (hitMarkTime * 50);
            if (i < 0) i = 0;

            foreach (var k in hitMarker)
            {
                Color c = k.color;
                c.a = i;
                k.color = c;
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    public void UpdateAmmo(int _remainAmmo)
    {
        remainAmmo.text = _remainAmmo.ToString();
    }

    public void UpdateMag(int _remainMag)
    {
        remainMag.text = _remainMag.ToString();
    }

    public void SetActiveInterAction(bool _active) => InterAction.SetActive(_active);

    #endregion
    #region  보스
    public void UpdateBossUI(string _bossName) { bossName.text = _bossName; }
    public void UpdateBossHealth(int _maxHealth, int _curHealth)
    {
        bossHealthText.text = _curHealth + " / " + _maxHealth;
        bossHealthImage.sizeDelta =
            new Vector2(bossHealthBackground.sizeDelta.x / _maxHealth * _curHealth,
                bossHealthImage.sizeDelta.y);
    }

    #endregion
    /*
    작업예정

    public void UpdateSkill0(int _maxGauge, int _curGauge)  // 궁극기
    { 
        // (내용채우기)
    }
    */
    #endregion
    public void SetMouseLock(bool _active)
    {
        if (_active) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
        Cursor.visible = !_active;
    }

    IEnumerator FadeOut(Image _image, float _time)
    {
        float i = 1;
        Color c = _image.color;
        while (i > 0)
        {
            i -= 1 / (_time * 50);
            if (i < 0) i = 0;

            c.a = i;
            _image.color = c;

            yield return new WaitForSeconds(0.02f);
        }
    }

    public void QuitProgram()
    {
        Application.Quit();
    }
}
