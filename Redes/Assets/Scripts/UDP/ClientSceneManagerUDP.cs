using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

public class ClientSceneManagerUDP : MonoBehaviour
{
    // UI
    [SerializeField] GameObject chatGameObject;
    Text chatText;
    InputField chatInput;
    GameObject serverIpInput;
    GameObject userNameInput;
    [SerializeField] GameObject chatInputGameObject;
    [SerializeField] GameObject connectButton;
    [SerializeField] Text connectedPeople;

    [SerializeField] ServerUDP serverUDP;
    [SerializeField] ClientUDP clientScript;

    InputField serverIpInputField;
    InputField userNameInputField;

    // Players
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;
    Vector3 initialPlayerPos = Vector3.zero;
    Vector3 initialEnemyPos = Vector3.zero;


    // Fade
    Image fadeImage;
    bool fadingIn = false;
    bool fadingInCompleted = false;
    bool fadingOut = false;


    // Utility
    float blackScreenCounter = 5.0f;
    bool gameEnded = false;
    bool startingNewGame = false;
    public bool gameStarted = false;
    [HideInInspector] public bool clientJoined = false;

    // UI variables
    [SerializeField] GameObject[] UIToDeactivate;

    void Start()
    {
        serverIpInput = GameObject.Find("Server IP Input");
        userNameInput = GameObject.Find("Username Input");

        fadeImage = GameObject.Find("Fade").GetComponent<Image>();
        if (serverIpInput != null)
        {
            serverIpInputField = serverIpInput.GetComponentInChildren<InputField>();
            userNameInputField = userNameInput.GetComponentInChildren<InputField>();
        }

        chatText = GameObject.Find("Chat Text").GetComponent<Text>();
        chatInput = chatInputGameObject.GetComponent<InputField>();

        initialPlayerPos = player.transform.position;
        initialEnemyPos = enemy.transform.position;
    }

    void Update()
    {
        if (clientJoined)
        {
            StartClientConnection();
            HideUIChat(false);
            clientJoined = false;
        }

        if (fadingIn)
        {
            Color color = fadeImage.color;
            color.a += Time.deltaTime * 3.0f;
            fadeImage.color = color;

            if (color.a > 1.01f)
            {
                fadingIn = false;
                fadingInCompleted = true;
            }
        }
        else if (fadingInCompleted)
        {
            if (blackScreenCounter > 0.0f)
            {
                blackScreenCounter -= Time.deltaTime;
            }
            else
            {
                UpdateScene();
                fadingInCompleted = false;
                fadingOut = true;
                startingNewGame = true;
            }
        }
        else if (fadingOut)
        {
            Color color = fadeImage.color;
            color.a -= Time.fixedDeltaTime * 3.0f;
            fadeImage.color = color;

            if (color.a < -0.01f)
            {
                fadingOut = false;
                fadingInCompleted = false;
                fadingIn = false;
            }
        }

        if (gameEnded && startingNewGame)
        {
            ToggleGameUI(true);
            
            if (serverUDP != null)
            {
                for (int i = 0; i < UIToDeactivate.Length; ++i)
                {
                    UIToDeactivate[i].gameObject.SetActive(true);
                }
            }
            gameStarted = false;
            startingNewGame = false;
            gameEnded = false;
        }

        if (Input.GetKeyDown(KeyCode.Return) && chatInput.text.Length > 0)
        {
            string msg = "[Server]: " + chatInput.text;
            chatText.text += msg;
            chatInput.text = string.Empty;

            if (serverUDP != null)
            {
                byte[] bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, serverUDP.GetNetId(), msg);
                serverUDP.Send(bytes);
            }
        }


    }

    public void StartClient()
    {
        clientJoined = true;
        gameStarted = true;
    }

    public void StartClientConnection()
    {
        fadingOut = true;

        ToggleGameUI(true);

        clientScript.gameObject.SetActive(true);
        clientScript.serverIp = serverIpInputField.text;
        clientScript.userName = userNameInputField.text;

        player.SetActive(true);
        player.GetComponent<PlayerMovement>().isClient = true;
        enemy.SetActive(true);
    }

    void HideUIChat(bool value)
    {
        chatGameObject.SetActive(value);
        chatInputGameObject.SetActive(value);
    }

    public void StartServerConnection()
    {
        fadingOut = true;
        gameStarted = true;
        for (int i = 0; i < UIToDeactivate.Length; ++i)
        {
            UIToDeactivate[i].gameObject.SetActive(false);
        }

        byte[] bytes = Serializer.SerializeBoolWithHeader(MessageType.START_GAME, true);
        serverUDP.Send(bytes);
    }

    public void EndGame()
    {
        gameEnded = true;
        fadingIn = true;
        ToggleGameUI(false);        
    }

    void ToggleGameUI(bool value)
    {
        chatGameObject.SetActive(value);
        chatInputGameObject.SetActive(value);
        if (!gameEnded)
        {
            if (serverIpInput != null)
            {
                serverIpInput.SetActive(!value);
                userNameInput.SetActive(!value);
                connectButton.SetActive(!value);
            }
        }
    }

    void UpdateScene()
    {
        player.SetActive(true);
        player.transform.position = initialPlayerPos;
        player.GetComponent<PlayerMovement>().ResetStats();

        enemy.SetActive(true);
        enemy.transform.position = initialEnemyPos;
        enemy.GetComponent<EnemyController>().ResetStats();
    }


    public void OnNewChatMessage(string message)
    {
        chatText.text += message + "\n";
    }

    public void UpdateUsersList(string message)
    {
        if (connectedPeople == null)
            return;

        connectedPeople.text += message + "\n";
    }

}
