using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ClientSceneManager : MonoBehaviour
{
    [SerializeField] GameObject userInputs;
    GameObject ipInput;
    GameObject userNameInput;
    GameObject inputBox;
    
    [SerializeField] Client clientScript;

    InputField ipInputField;
    InputField userNameInputField;

    // Start is called before the first frame update
    void Start()
    {
        ipInput = GameObject.Find("IP Input");
        userNameInput = GameObject.Find("Username Input");

        inputBox = GameObject.Find("Input");

        inputBox.SetActive(false);
        ipInputField = ipInput.GetComponentInChildren<InputField>();
        userNameInputField = userNameInput.GetComponentInChildren<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!inputBox.activeSelf &&  Input.GetKeyUp(KeyCode.Return))
        //{
        //    Debug.Log("Your IP is " + ipInputField.text);
        //    inputBox.SetActive(true);
        //    ipInput.SetActive(false);
        //
        //    clientScript.gameObject.SetActive(true);
        //    clientScript.clientIp = ipInputField.text;
        //    clientScript.userName = userNameInputField.text;
        //    //clientScript.InitNetwork();
        //}
    }

    public void StartConnection()
    {
        Debug.Log("Your IP is " + ipInputField.text);
        inputBox.SetActive(true);

        clientScript.gameObject.SetActive(true);
        clientScript.clientIp = ipInputField.text;
        clientScript.userName = userNameInputField.text;
        
        ipInput.SetActive(false);
        userNameInput.SetActive(false);
    }

}
