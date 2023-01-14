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

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        udpManager = GameObject.Find("SceneManager").GetComponent<UDPManager>();
    }

    void Update()
    {
        if (isMoving)
        {
            objectData.position = this.transform.position;
            objectData.rotation = this.transform.rotation;
            udpManager.SendObjectData(objectData, objectID, isClient);
        }    
        
        if (rb.velocity == Vector3.zero) 
        {
            isMoving = false;
        }
    }

    public void ApplyImpulseForce()
    {
        //rb.AddForce(objectData.impulse, ForceMode.Impulse);
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

        if (collision.gameObject.tag == "Player")
        {
            impulse = collision.gameObject.transform.forward * 2.0f;
            rb.AddForce(impulse, ForceMode.Force);
            objectData.impulse = impulse;
            isMoving = true;
        }
    }
}
