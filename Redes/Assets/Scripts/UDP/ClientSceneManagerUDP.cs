using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ClientSceneManagerUDP : MonoBehaviour
{
    // UI
    [SerializeField] GameObject chatGameObject;
    [SerializeField] Text chatText;
    InputField chatInput;
    GameObject serverIpInput;
    GameObject userNameInput;
    [SerializeField] GameObject chatInputGameObject;
    [SerializeField] GameObject connectButton;
    [SerializeField] Text connectedPeople;

    [SerializeField] ServerUDP serverUDP;
    [SerializeField] ClientUDP clientUDP;

    InputField serverIpInputField;
    InputField userNameInputField;

    // Players
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;
    Vector3 initialPlayerPos = Vector3.zero;

    // connection manager
    [SerializeField] ConnectionsManager connectionManager;


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
    bool newChatMessage = false;
    string latestChatMessage = string.Empty;

    // UI variables
    [SerializeField] GameObject[] UIToDeactivate;

    void Start()
    {
        serverIpInput = GameObject.Find("Server IP Input");
        userNameInput = GameObject.Find("Username Input");
        connectionManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();

        fadeImage = GameObject.Find("Fade").GetComponent<Image>();
        if (serverIpInput != null)
        {
            serverIpInputField = serverIpInput.GetComponentInChildren<InputField>();
            userNameInputField = userNameInput.GetComponentInChildren<InputField>();
        }

        chatInput = chatInputGameObject.GetComponent<InputField>();

        initialPlayerPos = player.transform.position;
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
            if (serverUDP != null)
            {
                string msg = "[Server]: " + chatInput.text;
                OnNewChatMessage(msg);

                byte[] bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, serverUDP.GetNetId(), msg);
                serverUDP.Send(bytes);
            }
            else
            {
                string msg = "[" + clientUDP.GetUserName() + "]" + ": " + chatInput.text;
                OnNewChatMessage(msg);

                byte[] bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, clientUDP.GetNetId(), msg);
                clientUDP.Send(bytes);
            }
        }

        if (newChatMessage)
        {
            chatText.text += latestChatMessage + "\n";
            latestChatMessage = string.Empty;
            chatInput.text = string.Empty;
            newChatMessage = false;
        }

    }

    public void StartClient()
    {
        if (clientUDP == null)
            return;

        clientJoined = true;
        gameStarted = true;
    }

    public void StartClientConnection()
    {
        fadingOut = true;

        ToggleGameUI(true);

        clientUDP.gameObject.SetActive(true);
        clientUDP.serverIp = serverIpInputField.text;
        clientUDP.userName = userNameInputField.text;

        player.SetActive(true);
        player.GetComponent<PlayerMovement>().isClient = true;
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

        byte[] bytes = Serializer.SerializeBoolWithHeader(MessageType.START_GAME, serverUDP.GetNetId(), true);
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
    }


    public void OnNewChatMessage(string message)
    {
        latestChatMessage = message;
        newChatMessage = true;
    }

    public void UpdateUsersList(string message)
    {
        if (connectedPeople == null)
            return;

        connectedPeople.text += message + "\n";
    }

    public void RemovePlayerFromList(string userName)
    {
        if (connectedPeople == null)
            return;

        connectedPeople.text = connectedPeople.text.Replace(userName, string.Empty);
    }

}
