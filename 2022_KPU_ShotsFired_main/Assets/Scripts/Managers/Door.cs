/*
Door.cs
1. 문의 열림,닫힘 기능: StageManager.cs의 통제를 받아 문의 열거나 닫는 함수를 실행한다.
 */
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject doorOBJ;
    [SerializeField] bool Active;

    public void SetDoor()
    {
        doorOBJ.SetActive(Active);
    }

}
