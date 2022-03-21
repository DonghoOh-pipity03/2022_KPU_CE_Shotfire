using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopTool : MonoBehaviour
{
    public bool useMouseLock = false;
    private void Start()
    {
        if(useMouseLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void Update() {
        if(useMouseLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
