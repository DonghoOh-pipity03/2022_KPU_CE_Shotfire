using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


namespace PhotonInit
{
    class PhotonInit : MonoBehaviourPunCallbacks
    {
        #region  MonoBehaviourCallBack
        void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        void Start() {
            UI_delete_map();
            UI_delete();
            UI_Select(MainHome_UI,MainHome_Map);
        }
        #endregion

        #region Private value  
        private string gameVersion = "1";
        private byte maxPlayersPerRoom = 4;
        #endregion

        #region  Private Method
        // UI select 
        private void UI_Select(GameObject UI)
        {
            UI.SetActive(true);
        }
        private void UI_Select(GameObject UI, GameObject Map)
        {
            
            UI.SetActive(true);
            Map.SetActive(true);
        }
        private void UI_delete_map()
        {
            MainHome_Map.SetActive(false);
            Lobby_Map.SetActive(false);
        }
        private void UI_delete()
        {
            MainHome_UI.SetActive(false);
            lodding_UI.SetActive(false);
            IdInput_UI.SetActive(false);
            RoomList_UI.SetActive(false);
            Lobby_UI.SetActive(false);
        }

        #endregion
        
        #region Public value
        [SerializeField]
        [Header("Network Nickname")]
        public TextMeshProUGUI nickname;
        public TextMeshProUGUI roomname;
        public TextMeshProUGUI Lobby_roomName;
        public GameObject player_Lobby;
        [Header("UI Selecter")]
        public GameObject MainHome_UI;
        public GameObject lodding_UI;
        public GameObject IdInput_UI;
        public GameObject RoomList_UI;
        public GameObject Lobby_UI;
        public GameObject MainHome_Map;
        public GameObject Lobby_Map;

        #endregion

        #region  Public Method 
        public void ConnectNetwork()
        {
            if(!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        public void selectName()
        {
            PhotonNetwork.NickName = nickname.text;
            PhotonNetwork.JoinLobby();
            // Debug.Log(PhotonNetwork.GetRoomList());
        }

        public void JoinRoom()
        {
            if(string.IsNullOrEmpty(roomname.text))
            {
                return ;
                // empty or null
            }
            PhotonNetwork.JoinRoom(roomname.text);
        }
        public void createRoom()
        {
            if(string.IsNullOrEmpty(roomname.text))
            {
                return ;
                // empty or null
            }
            PhotonNetwork.CreateRoom(roomname.text,new RoomOptions{MaxPlayers = maxPlayersPerRoom});
        }
        public void escRoom()
        {
            if(PhotonNetwork.IsConnected)
                PhotonNetwork.LeaveRoom();
                // 자동으로 서버는 연결된다. 
                // 방 로그아웃만 구현 
        }
        #endregion

        #region  MonoBehaviourPunCallbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connect to server");
            UI_delete();
            UI_Select(IdInput_UI);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("Photon disconnected {0}", cause);
        }

        public override void OnJoinedLobby()
        {
            Debug.LogFormat("{0} joined Lobby", PhotonNetwork.NickName);
            UI_delete();
            UI_Select(RoomList_UI);
        }
        public override void OnCreatedRoom()
        {
            Debug.LogFormat("{0} create room {1}", PhotonNetwork.NickName, roomname.text);
        }

        public override void OnJoinedRoom()
        {
            UI_delete();
            UI_delete_map();
            UI_Select(Lobby_UI, Lobby_Map);
            Lobby_roomName.text = "Room info :"+roomname.text;
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning("Join Failed : "+ message);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("Player entered {0}", other.NickName);
            Debug.Log(Instantiate(player_Lobby, new Vector3(0.2f,1.5f,0.5f),Quaternion.identity));
            if(PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("{0} master", PhotonNetwork.NickName);
            }
        }
        
        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("Player Left {0}", other.NickName);
            if(PhotonNetwork.IsMasterClient)
            {
                // testing 
            }
        }
        #endregion


    }
}
