using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameObject chickenPlayer;
    public float offset = 0.0f;
    //public bool gameStarted = false;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = chickenPlayer.transform.position;
        this.transform.Translate(new Vector3(0.0f, 0.0f, offset));        
    }
}
