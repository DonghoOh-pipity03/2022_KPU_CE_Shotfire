using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InterActionDebuger : MonoBehaviour
{
    [SerializeField] InterActionAgent interActionAgent;
    [SerializeField] TextMeshProUGUI sign; 


    private void Update() 
    {
        CheckInterAction();
    }

    void CheckInterAction()
    {
        if(interActionAgent.interActionComplete == true)
        {
            // 기능
            sign.text = "COMPLETE!";
            // 후처리
        }
    }
}
