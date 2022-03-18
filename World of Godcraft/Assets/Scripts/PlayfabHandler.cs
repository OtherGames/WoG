using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayfabHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text log;
    [SerializeField]
    private LobbyManager lobby;

    private void Start()
    {
        lobby.onPhotonConnection += PhotonMasterServer_Connected;
    }

    private void PhotonMasterServer_Connected(string nickname)
    {
        var request = new LoginWithCustomIDRequest { CustomId = nickname, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnRequestFailure);
    }

   
    private void OnLoginSuccess(LoginResult result)
    {
        Log("Заебок, мы в плейфабе");
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = LobbyManager.GetNickname() };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdateNameSuccess, OnRequestFailure);
    }

    private void OnUpdateNameSuccess(UpdateUserTitleDisplayNameResult obj)
    {
        
    }

    private void OnRequestFailure(PlayFabError error)
    {
        Log("Блэт, какая-то хуйня");
    }

    void Log(string msg)
    {
        log.text += "\n";
        log.text += msg;
    }
}
