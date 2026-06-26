using System;
using UnityEngine;

public class Cursor:MonoBehaviour{
    public void Update(){
        //set position to mouse
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}