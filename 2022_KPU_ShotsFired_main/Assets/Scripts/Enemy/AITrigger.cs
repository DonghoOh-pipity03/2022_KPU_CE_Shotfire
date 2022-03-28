using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITrigger : MonoBehaviour
{
    [SerializeField] GameObject respawnPoint;
    [SerializeField] bool respawnActive;

    private void OnTriggerEnter(Collider other) 
    {

        if(other.transform.root.tag == "Player")
        {
            respawnPoint.SetActive(respawnActive);
        }   
    }
}
