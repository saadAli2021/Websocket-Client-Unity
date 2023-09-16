

using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using Newtonsoft.Json.Linq;


public class WS_Client : MonoBehaviour
{
    public event Action<string> OnPlayerIdReceived;
    public event Action<string,JToken> OnRandomRoomJoined;
    public event Action<JToken> OnRoomListReceived;
    public event Action<JToken> OnSendToAllMsgReceived;
    public event Action<JToken> OnSendToOthersMsgReceived;
    public event Action<JToken> OnPlayerListReceived;
    public event Action<JToken> OnSendToTargetMsgReceived;
    public event Action<JToken> OnMasterClientStatusReceived;


    public static WS_Client instance;
    
    public  WebSocket ws;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(transform);
    }
    void Start()
    {
        Connect();
       
    }

    public void Connect()
    {
        ws = new WebSocket("ws://localhost:8080");
        ws.OnMessage += OnMessageReceived;
        ws.OnClose += OnConnectionClose;
        ws.Connect();
    }


    private void OnConnectionClose(object sender, CloseEventArgs e)
    {
        Debug.Log("server disconnected");
    }


    private void OnMessageReceived(object sender, MessageEventArgs e)
    {

        try
        {
            string data = e.Data;
            JObject jsonObject = JObject.Parse(data);

            string action = (string)jsonObject["action"];
            JToken payload = jsonObject["payload"];
            HandleReceivedDataFromServer(action, payload);

        }
        catch (System.Exception ex)
        {
            Debug.Log("error occured" + ex.Message);
        }

    }


    public void SendToServer(string action, string payload, WebSocket socket = null)
    {
        switch (action)
        {
            case "newConnection":

                ws.Send(payload);
                break;

            case "roomsList":

                ws.Send(payload);
                break;

            case "isMasterClient":

                ws.Send(payload);
                break;

            case "joinOrCreateRoom":
                ws.Send(payload);
                break;

            case "sendToAll":
                ws.Send(payload);
                break;

            case "sendToOthers":
                ws.Send(payload);
                break;

            case "sendToTarget":
                ws.Send(payload);
                break;

            case "playerList":
                ws.Send(payload);
                break;

            default:
                Debug.Log("Unhandled action: " + action);
                break;
        }


    }


    public void HandleReceivedDataFromServer(string action, JToken payload)
    {
        switch (action)
        {
            case "playerIdAssinged":
                {
                    string playerID = (string)payload["playerID"];
                    MainThreadDispatcher.Execute(() =>
                    {
                        Debug.Log("Received player ID: " + playerID);
                        OnPlayerIdReceived?.Invoke(playerID);
                    });
                    break;
                }

            case "roomJoined":
                {
                    MainThreadDispatcher.Execute(() =>
                    {
                       
                        string roomName = (string)payload["roomName"];
                        string roomID = (string)payload["roomID"];
                     
                        JToken playersInRoom = payload["currentPlayers"];

                         OnRandomRoomJoined?.Invoke(roomID, playersInRoom);
                    });
                   
                    break;
                }

            case "roomsList":
                {
                    MainThreadDispatcher.Execute(() =>
                    {

                    JToken roomsList = payload["roomsList"];
                        OnRoomListReceived?.Invoke(roomsList);

                    });
                    break;
                }

            case "sendToAll":
                {
                    MainThreadDispatcher.Execute(() =>
                    {
                        
                        OnSendToAllMsgReceived?.Invoke(payload);

                    });
                    break;
                }
            case "sendToOthers":
                {
                    MainThreadDispatcher.Execute(() =>
                    {

                        OnSendToOthersMsgReceived?.Invoke(payload);

                    });
                    break;
                }


            case "sendToTarget":
                {
                    MainThreadDispatcher.Execute(() =>
                    {

                        OnSendToTargetMsgReceived?.Invoke(payload);

                    });
                    break;
                }
            case "playerList":
                {
                    MainThreadDispatcher.Execute(() =>
                    {

                        OnPlayerListReceived?.Invoke(payload);

                    });
                    break;
                }
            case "isMasterClient":
                {
                    MainThreadDispatcher.Execute(() =>
                    {

                        OnMasterClientStatusReceived?.Invoke(payload);

                    });
                    break;
                }

            default:
                Debug.Log("Unhandled action: " + action);
                break;
        }


    }

    
}


