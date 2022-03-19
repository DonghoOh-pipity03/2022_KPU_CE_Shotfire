using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject game_start_button;
    TextMeshPro mText;

    void Start()
    {
        // 추후 수정 
        game_start_button = GameObject.Find("Button-game start");
        mText = gameObject.GetComponent<TextMeshPro>();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void ID_input()
    {
        Debug.Log(mText.text);
    }
    public void button_Action()
    {
        Debug.Log("Start Click");
        PhotonNetwork.CreateRoom("Testing");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect to server");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Join Room");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }
    
    // Update is called once per frame
    void Update()
    {
        // mText
    }
}
