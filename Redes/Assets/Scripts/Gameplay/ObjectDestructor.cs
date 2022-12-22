using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestructor : MonoBehaviour
{
    private Rigidbody rb;
    public float impulseForce = 10.0f;
    private Vector3 impulse = Vector3.zero;

    // Online variables
    public ObjectData objectData = new ObjectData();
    UDPManager udpManager;
    public bool isClient = false;
    private bool isMoving = false;
    public int objectID = -1;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        udpManager = GameObject.Find("SceneManager").GetComponent<UDPManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            //objectData.position = this.gameObject.transform.position;
            //objectData.rotation = this.gameObject.transform.rotation;
            // Send data
            udpManager.SendObjectData(objectData, objectID, isClient);
            isMoving = false;
            //rb.AddForce(impulse, ForceMode.Impulse);
            //isMoving = false;
            //impulse = Vector3.zero;
        }

        //if (rb.velocity == Vector3.zero)
          //  isMoving = false;
        
    }

    public void SetImpulseForce()
    {
        rb.AddForce(objectData.impulse, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Rocket")
        {
            impulse = collision.gameObject.transform.forward * impulseForce;
            rb.AddForce(impulse, ForceMode.Impulse);
            objectData.impulse = impulse;
            isMoving = true;
        }
    }
}
