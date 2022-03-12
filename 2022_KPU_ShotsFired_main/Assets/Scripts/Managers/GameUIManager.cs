using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
#region 전역 변수
    [SerializeField] private GameObject m_miniMap;  // 플레이어_미니맵 
    // 체력 계열
    [SerializeField] private RectTransform playerHealthBackground;  // 플레이어_체력 GUI 배경
    [SerializeField] private RectTransform playerMaxHealthBar;  // 플레이어_최대 체력 GUI
    [SerializeField] private RectTransform playerCurHealthBar;  // 플레이어_현재 체력 GUI
    [SerializeField] private TextMeshProUGUI playerHealth;  // 플레이어_체력 텍스트
    // 무기 계열
    [SerializeField] private TextMeshProUGUI remainAmmo;   // 플레이어_남은 탄환
    [SerializeField] private TextMeshProUGUI remainMag;    // 플레이어_남은 탄창
#endregion
#region 전역 동작 변수
    private float playerHealthGUIRatio => playerMaxHealthBar.sizeDelta.x / 100f; // 플레이어_체력 포인트와 체력 GUI 바의 비율 
    private float playerHealthBackgorundHorizonMargine;    // 플레이어_체력 배경 GUI 가로 마진
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

    private void Awake() 
    {   
        playerHealthBackgorundHorizonMargine = (playerHealthBackground.sizeDelta.x - playerMaxHealthBar.sizeDelta.x) / 2f; 
    }

    public void SetActiveMiniMap(bool _active) 
    {
        m_miniMap.SetActive(_active);
    }
    #region 체력계열
    public void UpdatePlayerHealth(float _playerHealth, float _maxHealth)
    {
        playerHealth.text = _playerHealth + "/" + _maxHealth;
        playerCurHealthBar.sizeDelta = new Vector2(_playerHealth * playerHealthGUIRatio, 
                                                playerCurHealthBar.sizeDelta.y);
    }
    #endregion
    #region 무기 계열
    public void UpdateAmmo(int _remainAmmo)
    {
        remainAmmo.text = _remainAmmo.ToString();
    }

    public void Updatemag(int _remainMag)
    {   
        remainMag.text = _remainMag.ToString();
    }
    #endregion
    /*
    작업예정

    public void UpdateSkill0(int _maxGauge, int _curGauge)  // 궁극기
    { 
        // (내용채우기)
    }
    */
}
