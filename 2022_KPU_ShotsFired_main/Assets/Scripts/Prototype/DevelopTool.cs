using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopTool : MonoBehaviour
{
    public bool useMouseLock = false;   // 마우스 잠금 사용여부
    public float timeScale = 1f;    // 게임시간

    private void Update() {
        // 마우스 잠금
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

        // 시간조절
        Time.timeScale = timeScale;
    }
}
