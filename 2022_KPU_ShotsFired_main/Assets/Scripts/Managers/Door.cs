using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorKind{ active }
    public DoorKind doorKind;
    [SerializeField] GameObject doorOBJ;
    [SerializeField] bool Active;

    public void SetDoor(){
        switch(doorKind){
            case DoorKind.active:
                doorOBJ.SetActive(Active);
                break;
        }
    }
}
