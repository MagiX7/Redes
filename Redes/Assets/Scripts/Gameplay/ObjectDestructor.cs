using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestructor : MonoBehaviour
{
    private Rigidbody rb;
    public float impulseForce = 10.0f;

    // Online variables
    public ObjectData objectData = new ObjectData();
    [SerializeField] UDPManager udpManager;
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
            objectData.position = this.gameObject.transform.position;
            objectData.rotation = this.gameObject.transform.rotation;
            // Send data
            udpManager.SendObjectData(objectData, objectID, isClient);
        }

        if (rb.velocity == Vector3.zero)
            isMoving = false;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Rocket")
        {
            rb.AddForce(collision.gameObject.transform.forward * impulseForce, ForceMode.Impulse);
            isMoving = true;
        }
    }
}
