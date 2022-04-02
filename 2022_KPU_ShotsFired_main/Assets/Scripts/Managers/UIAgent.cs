using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAgent : MonoBehaviour
{
    [Tooltip("기능을 사용하지 않으면 비워두기")]
    [SerializeField] private Button ESC; // ESC 버튼을 ESC키로 대체한다.
    [SerializeField] private Button Enter; // Enter 버튼을 Enter키로 대체한다.
    [SerializeField] private Button AnyKey; // 아무 버튼을 아무 키로 대체한다.
   private void Update() 
   {    
       if(ESC != null && Input.GetKeyDown(KeyCode.Escape)) ESC.onClick.Invoke();
       if(Enter != null && Input.GetKeyDown(KeyCode.Return)) Enter.onClick.Invoke();
       if(AnyKey != null && Input.anyKey) AnyKey.onClick.Invoke(); 
   }

}
