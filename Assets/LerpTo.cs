using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTo : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position +offset, Time.deltaTime * 12);
        //transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
    }
}
