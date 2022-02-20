/*
최초작성자: 오동호
최종수정자: 오동호
수정날짜: 20220213
버전: 0.01.00
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class GameSceneManager : MonoBehaviour
{
    //싱글톤
    private static GameSceneManager instance;
    public static GameSceneManager Instance
    {
        get 
        {
            if (instance == null) instance = FindObjectOfType<GameSceneManager>();
            return instance;
        }
    }

    [SerializeField] private PostProcessVolume m_postProcesser;

    public void AddScene(string _sceneName) 
    {
        SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
    }

    public void CloseScene(string _sceneName) 
    {
        var scene = GameObject.Find(_sceneName);
        if (scene != null) Destroy(scene);
    }

    public void SetActiveWallPaper(bool _active) 
    {
        m_postProcesser.enabled = _active;
    }

    public void SetActiveUI(GameObject _UI, bool _active)
    {
        _UI.SetActive(_active);
    }

    public void RescaleGameTime(float _time)
    {
        Time.timeScale = _time;
    }
}
