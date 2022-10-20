using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ClientSceneManager : MonoBehaviour
{
    [SerializeField] GameObject userInputs;
    [SerializeField] GameObject chat;
    GameObject clientIpInput;
    GameObject serverIpInput;
    GameObject userNameInput;
    GameObject inputBox;
    [SerializeField] GameObject connectButton;
    
    [SerializeField] Client clientScript;

    InputField clientIpInputField;
    InputField serverIpInputField;
    InputField userNameInputField;

    // Start is called before the first frame update
    void Start()
    {
        clientIpInput = GameObject.Find("User IP Input");
        serverIpInput = GameObject.Find("Server IP Input");
        userNameInput = GameObject.Find("Username Input");

        inputBox = GameObject.Find("Input");

        inputBox.SetActive(false);
        clientIpInputField = clientIpInput.GetComponentInChildren<InputField>();
        serverIpInputField = serverIpInput.GetComponentInChildren<InputField>();
        userNameInputField = userNameInput.GetComponentInChildren<InputField>();
    }

    public void StartConnection()
    {
        Debug.Log("Your IP is " + clientIpInputField.text);
        inputBox.SetActive(true);

        chat.SetActive(true);
        clientScript.gameObject.SetActive(true);
        clientScript.clientIp = clientIpInputField.text;
        clientScript.serverIp = serverIpInputField.text;
        clientScript.userName = userNameInputField.text;
        
        clientIpInput.SetActive(false);
        serverIpInput.SetActive(false);
        userNameInput.SetActive(false);
        connectButton.SetActive(false);
    }

}
