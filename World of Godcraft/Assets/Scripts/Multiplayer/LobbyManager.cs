using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string sceneNameToload;

    [Space(3)]
    [SerializeField] byte maxPlayers = 10;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI logText;
    [SerializeField] Button btnConnect;
    [SerializeField] TMP_InputField inputField;

    ExitGames.Client.Photon.Hashtable roomProps;

    public System.Action<string> onPhotonConnection;

    void Awake()
    {
        // Первоначальные настройки клиента, тупо спизжено из тутора
        if (PhotonNetwork.NickName == string.Empty)
        {
            //PhotonNetwork.NickName = "Пидор_" + Random.Range(100, 999);
            PhotonNetwork.NickName = "Woger";
        }
        //PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";

        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 60;

        PhotonNetwork.ConnectUsingSettings();
        // ________________________________________________________
        roomProps = new ExitGames.Client.Photon.Hashtable();
        
        roomProps["idi_naxyi"] = PhotonNetwork.GameVersion;

        btnConnect.onClick.AddListener(JoinRoom);

    }

    public static string GetNickname()
    {
        return PhotonNetwork.NickName;
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.CountOfPlayers > 1)
            PhotonNetwork.NickName = "Pisko";
        Log("Некий хуежуй: " + PhotonNetwork.NickName + " присоеденился к пиздатой игруле");
        onPhotonConnection?.Invoke(PhotonNetwork.NickName);
    }

    public override void OnConnected()
    {
        Log("шо блять?");
    }

    void CreateRoom()
    {  
        string[] props = new string[1];
        props[0] = "idi_naxyi";

        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            MaxPlayers = maxPlayers,
            CleanupCacheOnLeave = false,
            IsOpen = true,
            IsVisible = true,
            //CustomRoomProperties = roomProps,
            //CustomRoomPropertiesForLobby = props
        });
    }

    public void JoinRoom()
    {
        print(PhotonNetwork.CountOfRooms);
        print(PhotonNetwork.CurrentRoom);
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinRandomRoom(roomProps, maxPlayers);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
        print("Не получилось заджойнитья");
    }

    public override void OnJoinedRoom()
    {
        print("Припиздяшил, Уебок " + PhotonNetwork.NickName);
        
        PhotonNetwork.LoadLevel(sceneNameToload);
    }

    public void OnChangedMaxPlayers()
    {
        //if (inputField.text.Length > 0)
        //{
        //    maxPlayers = byte.Parse(inputField.text);
        //}
    }

    

    void Log(string msg)
    {
        logText.text += "\n";
        logText.text += msg;
    }

    
}
