using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ClientSceneManager : MonoBehaviour
{
    [SerializeField] GameObject userInputs;
    [SerializeField] GameObject chat;
    GameObject serverIpInput;
    GameObject userNameInput;
    GameObject inputBox;
    [SerializeField] GameObject connectButton;
    
    [SerializeField] Client clientScript;

    InputField serverIpInputField;
    InputField userNameInputField;

    // Start is called before the first frame update
    void Start()
    {
        serverIpInput = GameObject.Find("Server IP Input");
        userNameInput = GameObject.Find("Username Input");

        inputBox = GameObject.Find("Input");

        inputBox.SetActive(false);
        serverIpInputField = serverIpInput.GetComponentInChildren<InputField>();
        userNameInputField = userNameInput.GetComponentInChildren<InputField>();
    }

    public void StartConnection()
    {
        inputBox.SetActive(true);

        chat.SetActive(true);
        clientScript.gameObject.SetActive(true);
        clientScript.serverIp = serverIpInputField.text;
        clientScript.userName = userNameInputField.text;
        
        serverIpInput.SetActive(false);
        userNameInput.SetActive(false);
        connectButton.SetActive(false);
    }

}
