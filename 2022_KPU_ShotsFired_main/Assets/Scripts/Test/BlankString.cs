using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankString : MonoBehaviour
{   
    public string text;

    private void Update() 
    {
        if(text == null) Debug.Log("null");
        else if(text == "" ) Debug.Log("\"");
        else Debug.Log("else");
    }
}
