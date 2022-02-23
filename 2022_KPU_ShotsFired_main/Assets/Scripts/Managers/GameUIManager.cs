using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
#region 전역 변수
    [SerializeField] private GameObject m_miniMap;
    // 체력 계열
    [SerializeField] private TextMeshProUGUI playerHealth;  // 플레이어_체력
    // 무기 계열
    [SerializeField] private TextMeshProUGUI remainAmmo;   // 플레이어_남은 탄환
    [SerializeField] private TextMeshProUGUI remainMag;    // 플레이어_남은 탄창
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

    public void SetActiveMiniMap(bool _active) 
    {
        m_miniMap.SetActive(_active);
    }
    #region 체력계열
    public void UpdatePlayerHealth(float _playerHealth, float _maxHealth)
    {
        playerHealth.text = _playerHealth + "/" + _maxHealth;
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
