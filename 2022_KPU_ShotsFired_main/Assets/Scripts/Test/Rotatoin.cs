using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotatoin : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public Vector3 transform_eulerAngles;
    public Vector3 transform_invert_eulerAngles;

    private void FixedUpdate() {
        //Debug.Log("Fixed Update");
        this.transform.rotation = Quaternion.Euler(x,y,z);
        
        transform_eulerAngles = transform.eulerAngles;

        transform_invert_eulerAngles = transform.forward * -1;

    }

    private void OnDrawGizmos() {
            Gizmos.color = new Color(1f,0f,0f,0.5f);
            Gizmos.DrawLine(transform.position, transform.position+ transform.forward.normalized);

            Gizmos.color = new Color(0f,0f,1f,0.5f);
            Gizmos.DrawLine(transform.position, transform.position + transform_invert_eulerAngles.normalized * 2);
    }
}
