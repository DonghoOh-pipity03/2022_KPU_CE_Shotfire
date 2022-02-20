using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotatoin : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public Vector3 transform_eulerAngles;

    private void FixedUpdate() {
        //Debug.Log("Fixed Update");
        this.transform.rotation = Quaternion.Euler(x,y,z);
        
        transform_eulerAngles = transform.eulerAngles;
    }

    private void OnDrawGizmos() {
            Gizmos.color = new Color(1f,0f,0f,0.5f);
            Gizmos.DrawLine(transform.position, transform.position+transform.forward);
    }
}
