using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


namespace PhotonInit
{
    class PhotonInit : MonoBehaviourPunCallbacks
    {
        public PhotonView PV;

        #region  MonoBehaviourCallBack
        void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        void Start() {
            Lobby_position.Add(new Vector3(49.0f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(50.5f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(52.0f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(53.5f, 1.5f, 0.5f));
            UI_delete_map();
            UI_delete();
            UI_Select(MainHome_UI,MainHome_Map);
        }
        #endregion

        #region Private value  
        private string gameVersion = "1";
        private byte maxPlayersPerRoom = 4;
        private List<Vector3>Lobby_position = new List<Vector3>();
        bool isConnecting;

        
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
            Player_UI.SetActive(false);
            MainHome_UI.SetActive(false);
            lodding_UI.SetActive(false);
            IdInput_UI.SetActive(false);
            RoomList_UI.SetActive(false);
            Lobby_UI.SetActive(false);
            Report_UI.SetActive(false);
        }
        
        #endregion
        
        #region Public value
        [SerializeField]
        [Header("Network Nickname")]
        public TextMeshProUGUI nickname;
        public TextMeshProUGUI roomname;
        public TextMeshProUGUI Lobby_roomName;

        [Header("UI Selecter")]
        public GameObject MainHome_UI;
        public GameObject lodding_UI;
        public GameObject IdInput_UI;
        public GameObject RoomList_UI;
        public GameObject Lobby_UI;
        public GameObject Player_UI;
        public GameObject Report_UI;
        public GameObject MainHome_Map;
        public GameObject Lobby_Map;

        [Header("LobbyPanel")]
        public Button[] CellBtn;
        public Button PreviousBtn;
        public Button NextBtn;

        List<RoomInfo> myList = new List<RoomInfo>();
        int currentPage = 1, maxPage, multiple;
        
        
        #endregion


        #region  Public Method 
        public void ConnectNetwork()
        {
            isConnecting = true;
            if(!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        public void Disconnect() => PhotonNetwork.Disconnect();
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
        public void MyListRenewal()
        {
            // 최대페이지
            maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

            // 이전, 다음버튼
            PreviousBtn.interactable = (currentPage <= 1) ? false : true;
            NextBtn.interactable = (currentPage >= maxPage) ? false : true;

            // 페이지에 맞는 리스트 대입
            multiple = (currentPage - 1) * CellBtn.Length;
            for (int i = 0; i < CellBtn.Length; i++)
            {
                CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
                CellBtn[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";

            }
        }
        public void MyListClick(int num)
        {
            if(num == -2) --currentPage; //페이지 제거
            else if(num == -1) ++currentPage; //페이지 추가
            else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
            MyListRenewal();
        }
        public void start_game()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                // PhotonNetwork.LoadLevel("Temp Stage 1-1");
                // SceneManager.LoadScene("Temp Stage 1-1", LoadSceneMode.Additive);
                base.photonView.RPC("game_start",RpcTarget.All);
            }
        }

        #endregion

        #region  MonoBehaviourPunCallbacks
        public override void OnConnectedToMaster()
        {
            if (isConnecting)
			{
                Debug.Log("Connect to server");
                UI_delete();
                UI_Select(IdInput_UI);
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            isConnecting = false;
            Debug.LogWarningFormat("Photon disconnected {0}", cause);
        }

        public override void OnJoinedLobby()
        {
            Debug.LogFormat("{0} joined Lobby", PhotonNetwork.NickName);
            UI_delete();
            UI_Select(RoomList_UI);
            myList.Clear();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            int roomCount = roomList.Count;
            for (int i = 0; i < roomCount; i++)
            {
                if (!roomList[i].RemovedFromList)
                {
                    if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                    else myList[myList.IndexOf(roomList[i])] = roomList[i];
                }
                else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
            }
            MyListRenewal();
        }

        public override void OnCreatedRoom()
        {
            // 자기 자신의 캐릭터 생성 
            PhotonNetwork.Instantiate("CharacterDemo", Lobby_position[0],Quaternion.identity);
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
            // PV.RPC("log",RpcTarget.All);
            // 들어온 캐릭터 별로 생성 
            PhotonNetwork.Instantiate("CharacterDemo", Lobby_position[1],Quaternion.identity);
            if(PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("{0} is master", PhotonNetwork.NickName);
            }
        }
        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("Player Left {0}", other.NickName);
            if(PhotonNetwork.IsMasterClient)
            {
                // PV.RPC("exitRoom", RpcTarget.All);
                // 오류 해결 
            }
        }
        #endregion

        #region 
        [PunRPC]
        void game_start()
        {
            UI_delete();
            UI_delete_map();
            UI_Select(Player_UI);
            // PhotonNetwork.Destroy();
            SceneManager.LoadScene("Temp Stage 1-1", LoadSceneMode.Additive);
            // 범위내에서 랜덤한 위치 리스폰 필요 
            Vector3 pos = new Vector3(Random.Range(99.3f,100.3f),Random.Range(-0.3f,0.3f),1.0f);
            PhotonNetwork.Instantiate("Prototype Player", pos,Quaternion.identity);
        }
        #endregion
    }
}
