using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PressToLoadScene : MonoBehaviour
{   
    [SerializeField] Image progressBar; // 로딩 바 이미지
    [SerializeField] private string nextSceneName;  // 다음 씬 이름

    private void Start() {
        StartCoroutine(AsyncLoadScene());
    }

    // 출처: https://wergia.tistory.com/183
    IEnumerator AsyncLoadScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime / 3;
            if (op.progress < 0.9f)            
            {               
                //progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, 0.5f);
                if (progressBar.fillAmount >= op.progress) timer = 0f;
            }            
            else            
            {                
                //progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, 0.5f);
                if (progressBar.fillAmount == 1.0f && Input.anyKey)                
                {              
                    op.allowSceneActivation = true;                 
                    yield break;                
                }            
            }
        }
    }
}
