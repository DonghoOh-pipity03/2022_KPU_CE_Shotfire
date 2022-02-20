/*
최초작성자: 오동호
최종수정자: 오동호
수정날짜: 20220213
버전: 0.01.00
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
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

    [SerializeField] private GameObject m_miniMap;

    public void SetActiveMiniMap(bool _active) 
    {
        m_miniMap.SetActive(_active);
    }

    /*
    작업예정

    public void UpdateSkill0(int _maxGauge, int _curGauge)  // 궁극기
    { 
        // (내용채우기)
    }
    */
}
