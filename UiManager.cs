using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class UiManager : MonoBehaviour
{

    public static UiManager instance;
    [Header("Room UI Refrencess ... ")]
    public GameObject roomPanel;
    public GameObject roomListPanel;
    public GameObject roomPrefab;
    [Header("Login UI Refrencess ... ")]
    public GameObject loginPanel;
    public Text playerName;
    public Text playerID;
    public Text roomName;
    public Text masterClientText;
    [Header("Messages UI Refrencess ... ")]
    public Transform messagePanel;
    public GameObject messageTextPrefab;
    [Header("PlayerList UI Refrencess ... ")]
    
    public Transform sendToTargetPanel;
    public Transform playerListPanel;
    public GameObject playerListItemPrefab;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(transform);
    }
    void Start()
    {
        SubscribeEvents();

        roomPanel.SetActive(false);
        loginPanel.SetActive(true);

    }

    void SubscribeEvents() {

        WS_Client.instance.OnPlayerIdReceived += OnIdReceivedFromServer;
        WS_Client.instance.OnRandomRoomJoined += OnRandomRoomJoined;
        WS_Client.instance.OnRoomListReceived += OnRoomListReceived;
        WS_Client.instance.OnSendToAllMsgReceived += OnSendToAllMsgReceived;
        WS_Client.instance.OnSendToOthersMsgReceived += OnSendToOthersMsgReceived;
        WS_Client.instance.OnPlayerListReceived += OnPlayerListReceived;
        WS_Client.instance.OnSendToTargetMsgReceived += OnSendToTargetMsgReceived;
        WS_Client.instance.OnMasterClientStatusReceived += OnMasterClientStatusReceived;

    }

   

    #region Subscribed Events

    public void OnIdReceivedFromServer(string id) {
      
        playerID.text = id;
        loginPanel.SetActive(false);
        roomPanel.SetActive(true);
    }

    private void OnRandomRoomJoined(string joinedRoomID, JToken playersInRoom)
    {

        roomName.text =  joinedRoomID;
        
        var data = new
        {
            action = "roomsList",
           payload ="",
        };
        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("roomsList", json);

        // ask server to check if the current player is masterclient or not ?
        isMasterClient();


    }

    private void OnMasterClientStatusReceived(JToken payload)
    {
        bool isMasterClient = (bool)payload["isMasterClient"];
        masterClientText.text = isMasterClient.ToString();
    }


    private void OnRoomListReceived(JToken roomsList) {

        foreach (Transform child in roomListPanel.transform)
        {
            Destroy(child.gameObject); 
        }

        foreach (JToken room in roomsList)
        {
            string roomName = (string)room["roomName"];
            JToken currentPlayers = room["currentPlayers"];
            List<string> playersInRoom = currentPlayers.ToObject<List<string>>();

            GameObject _roomPrefab = Instantiate(roomPrefab);
            _roomPrefab.transform.Find("JoinRoomBtn").GetComponentInChildren<Text>().text = roomName;
            _roomPrefab.transform.Find("TotalPlayers").GetComponent<Text>().text = "Total Players : "+playersInRoom.Count;
            _roomPrefab.transform.SetParent(roomListPanel.transform,false);


        }
    }

    private void OnSendToAllMsgReceived(JToken payload)
    {
        string roomID = (string)payload["roomID"];
        string senderPlayerID = (string)payload["sender"];
        string senderPlayerName = (string)payload["playerName"];
        // getting the message data
        JToken message = payload["message"];
        string score = (string)message["score"];
        string msg = (string)message["msg"];
       // updating the UI
        GameObject messageText = Instantiate(messageTextPrefab);
        messageText.transform.SetParent(messagePanel, false);
        messageText.GetComponent<Text>().text = "("+ senderPlayerID + ") "+ senderPlayerName + " >> " + msg;
    }

    private void OnSendToOthersMsgReceived(JToken payload)
    {
        string roomID = (string)payload["roomID"];
        string senderPlayerID = (string)payload["sender"];
        string senderPlayerName = (string)payload["playerName"];
        // getting the message data
        JToken message = payload["message"];
        string msg = (string)message["msg"];
        // updating the UI
        GameObject messageText = Instantiate(messageTextPrefab);
        messageText.transform.SetParent(messagePanel, false);
        messageText.GetComponent<Text>().text = "(" + senderPlayerID + ") " + senderPlayerName + " >> " + msg;
    }

    private void OnSendToTargetMsgReceived(JToken payload)
    {
        string roomID = (string)payload["roomID"];
        string senderPlayerID = (string)payload["sender"];
        string senderPlayerName = (string)payload["playerName"];
        // getting the message data
        JToken message = payload["message"];
        string msg = (string)message["msg"];
        // updating the UI
        GameObject messageText = Instantiate(messageTextPrefab);
        messageText.transform.SetParent(messagePanel,false);
        messageText.GetComponent<Text>().text = "(" + senderPlayerID + ") " + senderPlayerName + " >> " + msg;
    }
    private void OnPlayerListReceived(JToken payload)
    {
        foreach (Transform child in playerListPanel.transform)
        {
            Destroy(child.gameObject);
        }

        JToken playersList = payload["playerList"];
      
        foreach (JToken player in playersList)
        {
            string playerID = (string)player["playerID"];
            string playerName = (string)player["playerName"];
         
            // updating the UI
            GameObject playerListItem = Instantiate(playerListItemPrefab);
            Text playerNameText = playerListItem.transform.Find("PlayerName").GetComponent<Text>();
            Text playerIDText = playerListItem.transform.Find("PlayerID").GetComponent<Text>();

            playerNameText.text = playerName;
            playerIDText.text = playerID;
            playerListItem.transform.Find("btnSend").GetComponent<Button>().onClick.AddListener(() => onClickSendToSpecificTarget(playerID));
            playerListItem.transform.SetParent(playerListPanel,false);

            //if (isMasterClient)
            //{
            //    playerNameText.color = Color.green;
            //    playerIDText.color = Color.green;
            //}
        }
        

    }

    public void onClickSendToSpecificTarget( string targetPlayerID) {
       
        var messageToSend = getMessage("Target");
      
        var data = new
        {
            action = "sendToTarget",
            payload = new
            {
                senderID = playerID.text,
                targetID = targetPlayerID,
                roomID = roomName.text,
                senderName = playerName.text,
                message = messageToSend,

            }
        };
        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("sendToTarget", json); ;
    }
  
    #endregion

    #region Button Clicks
    public void OnJoinLobby()
    {
        var data = new
        {
            action = "newConnection",
            payload = new
            {  
                playerName = playerName.text,
                
            }
        };
        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("newConnection", json); ;
    }

    public void OnJoinRandomRoom()
    { 
        var data = new
        {
            action = "joinOrCreateRoom",
            payload = new
            {
                playerId = playerID.text,

            }
        };
        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("joinOrCreateRoom", json); ;
    }

    public void onSendMessageToAll() {
      
        var messageToSend = getMessage("ToAll");
        
        var data = new
        {
            action = "sendToAll",
            payload = new
            {
                playerID = playerID.text,
                roomID = roomName.text,
                playerName = playerName.text,
                message= messageToSend,

            }
        };

        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("sendToAll", json);

    }

    public void onSendMessageToOthers()
    {

        var messageToSend = getMessage("ToOthers");

        var data = new
        {
            action = "sendToOthers",
            payload = new
            {
                playerID = playerID.text,
                roomID = roomName.text,
                playerName = playerName.text,
                message = messageToSend,

            }
        };

        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("sendToOthers", json);
    }
    private dynamic getMessage(string msgValue) {

        var message = new
        {
            msg  = msgValue,
            score  = 56,
            cards = new[] { "A", "B" },
        };

        return message;
    }
    
    public void OnNameInputValueChange(string value)
    {
       
        playerName.text = value;
    }

   
    
    public  void OnSendToTargetClick() {

        sendToTargetPanel.gameObject.SetActive(true);
        var data = new
        {
            action = "playerList",
            payload = new
            {
                senderID = playerID.text,
                roomID = roomName.text,
            }
        };

        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("playerList", json);

        
    }

    public void OnSendToTargetClose() {
        sendToTargetPanel.gameObject.SetActive(false);
    }


    public void isMasterClient()
    {
        var data = new
        {
            action = "isMasterClient",
            payload = new
            {
                playerId = playerID.text,

            }
        };
        string json = JsonConvert.SerializeObject(data);
        WS_Client.instance.SendToServer("isMasterClient", json); ;
    }
    #endregion Button_Clicks
}



