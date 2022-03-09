using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class GameSceneManager : MonoBehaviour
{   
    [SerializeField] private PostProcessVolume m_postProcesser;
    #region 전역 함수
    
    #endregion
    #region 전역 동작 함수
    
    #endregion
    
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

    // 씬을 추가한다.
    public void AddScene(string _sceneName) 
    {
        SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
    }


    public IEnumerator AddSceneWithAsync(string _sceneName) 
    {   
        var targetScene = SceneManager.GetSceneByName(_sceneName);
        if(!targetScene.isLoaded)
        {
            var op = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);

            while(!op.isDone)
            {
                yield return null;
            }
        }
    }

    public IEnumerator UnloadSceneWithAsync(string _sceneName)
    {
        var targetScene = SceneManager.GetSceneByName(_sceneName);
        if(targetScene.isLoaded)
        {
            var op = SceneManager.UnloadSceneAsync(_sceneName);

            while(!op.isDone)
            {
                yield return null;
            }
        }
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

    public void ActiveGameObject(GameObject _UI)
    {
        _UI.SetActive(true);
    }
     public void DisableGameObject(GameObject _UI)
    {
        _UI.SetActive(false);
    }

    public void QiutProgram()
    {
        Application.Quit();
    }


    public void RescaleGameTime(float _time)
    {
        Time.timeScale = _time;
    }
    
}
