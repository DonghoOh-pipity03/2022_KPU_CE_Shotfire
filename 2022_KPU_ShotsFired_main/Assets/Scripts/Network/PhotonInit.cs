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

        // 싱글톤 
        private static PhotonInit instance;
        public static PhotonInit Instance 
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<PhotonInit>();
                return instance;
            }
        }   

        #region  MonoBehaviourCallBack
        void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        void Start() {
            Lobby_position.Add(new Vector3(49.0f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(50.5f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(52.0f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(53.5f, 1.5f, 0.5f));
            Lobby_position.Add(new Vector3(49.0f, 1.0f, 0.5f));
            Lobby_position.Add(new Vector3(50.5f, 1.0f, 0.5f));
            Lobby_position.Add(new Vector3(52.0f, 1.0f, 0.5f));
            Lobby_position.Add(new Vector3(53.5f, 1.0f, 0.5f));
            UI_delete_map();
            UI_delete();
            UI_Select(MainHome_UI,MainHome_Map);
        }
        #endregion

        #region Private value  
        private string gameVersion = "1";
        private byte maxPlayersPerRoom = 4;
        private List<Vector3>Lobby_position = new List<Vector3>();

        private List<GameObject> playerTemps = new List<GameObject>();
        private List<GameObject> playerProtypes = new List<GameObject>();
        private bool isConnecting;
        private List<GameObject> Readys = new List<GameObject>();
        private bool isReady = false;
        private int readycnt = 0;


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

        [Header("LobbyPanel")]
        public TextMeshProUGUI[] ChatText;
        public TMP_InputField ChatInput;

        [Header("멀티 플레이어 관리")]
        [SerializeField] public GameObject player1NameBackground;  // 플레이어_체력 GUI 배경
        [SerializeField] public TextMeshProUGUI player1Name;  // 플레이어_체력 텍스트
        [SerializeField] public GameObject player2NameBackground;  // 플레이어_체력 GUI 배경
        [SerializeField] public TextMeshProUGUI player2Name;  // 플레이어_체력 텍스트
        [SerializeField] public GameObject player3NameBackground;  // 플레이어_체력 GUI 배경
        [SerializeField] public TextMeshProUGUI player3Name;  // 플레이어_체력 텍스트

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
            readycnt = 0;
            if(string.IsNullOrEmpty(roomname.text))
            {
                return ;
                // empty or null
            }
            PhotonNetwork.JoinRoom(roomname.text);
        }
        public void createRoom()
        {
            readycnt = 0;
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
            {
                /*if(isReady == true)
                {
                    isReady = false;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                }*/
                PhotonNetwork.LeaveRoom();
            }
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

        public void update_room()
        {
            int cnt = 0;
            foreach(GameObject playerTemp in playerTemps)
            {
                if(playerTemp == null)
                {
                    //Debug.LogFormat("Continue");
                    continue;
                }
                Debug.LogFormat("Destory");
                PhotonNetwork.Destroy(playerTemp);
            }
            if(PhotonNetwork.MasterClient.GetNext() == PhotonNetwork.LocalPlayer)
            {
                //Debug.LogFormat("Cnt = 1");
                cnt = 1;
            }
            if(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()) == PhotonNetwork.LocalPlayer)
            {
                //Debug.LogFormat("Cnt = 2");
                cnt = 2;
            }
            if(PhotonNetwork.IsMasterClient)
            {
                //Debug.LogFormat("Cnt = 0");
                cnt = 0;
            }
            Debug.LogFormat("update_lobby");
            Debug.LogFormat("Cnt : {0}", cnt);
            playerTemps.Add(PhotonNetwork.Instantiate("CharacterDemo", Lobby_position[cnt],Quaternion.identity));
        }
        public void update_ready()
        {
            int cnt = 4;
            foreach(GameObject Ready in Readys)
            {
                if(Ready == null)
                {
                    //Debug.LogFormat("Continue");
                    continue;
                }
                PhotonNetwork.Destroy(Ready);
            }
            if(PhotonNetwork.IsMasterClient)
            {
                if(isReady == false)
                {
                    isReady = true;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                    cnt = 4;
                    Readys.Add(PhotonNetwork.Instantiate("Ready", Lobby_position[cnt],Quaternion.identity));
                }
                else
                {
                    isReady = false;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                }
            }
            else if(PhotonNetwork.MasterClient.GetNext() == PhotonNetwork.LocalPlayer)
            {
                if(isReady == false)
                {
                    isReady = true;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                    cnt = 5;
                    Readys.Add(PhotonNetwork.Instantiate("Ready", Lobby_position[cnt],Quaternion.identity));
                }
                else
                {
                    isReady = false;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                }
            }
            else if(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()) == PhotonNetwork.LocalPlayer)
            {
                if(isReady == false)
                {
                    isReady = true;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                    cnt = 6;
                    Readys.Add(PhotonNetwork.Instantiate("Ready", Lobby_position[cnt],Quaternion.identity));
                }
                else
                {
                    isReady = false;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                }
            }
            else if(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext())) == PhotonNetwork.LocalPlayer)
            {
                if(isReady == false)
                {
                    isReady = true;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                    cnt = 7;
                    Readys.Add(PhotonNetwork.Instantiate("Ready", Lobby_position[cnt],Quaternion.identity));
                }
                else
                {
                    isReady = false;
                    base.photonView.RPC("ready_count", RpcTarget.AllBuffered, isReady);
                }
            }
        }
        public void cancel_ready()
        {
            foreach(GameObject Ready in Readys)
            {
                if(Ready == null)
                {
                    //Debug.LogFormat("Continue");
                    continue;
                }
                PhotonNetwork.Destroy(Ready);
            }
        }
        public void send_msg()
        {
            if(ChatInput.text != "")
            {
                base.photonView.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
                ChatInput.text = "";
            }
        }   
        public void start_game()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                if(readycnt == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    base.photonView.RPC("game_start",RpcTarget.All);
                    PhotonNetwork.IsMessageQueueRunning = false;
                    PhotonNetwork.IsMessageQueueRunning = true;
                }
            }
            update_ready();
        }
        public void end_game()
        {
            Debug.Log("end_game");
            if(PhotonNetwork.IsMasterClient)
            {
                base.photonView.RPC("game_end",RpcTarget.All);
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
            Debug.LogFormat("{0} create room {1}", PhotonNetwork.NickName, roomname.text);
        }

        public override void OnJoinedRoom()
        {
            UI_delete();
            UI_delete_map();
            UI_Select(Lobby_UI, Lobby_Map);
            isReady = false;
            Lobby_roomName.text = "Room info : "+ PhotonNetwork.CurrentRoom.Name;
            ChatInput.text = "";
            for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
            playerTemps.Add(PhotonNetwork.Instantiate("CharacterDemo", Lobby_position[PhotonNetwork.CurrentRoom.PlayerCount-1],Quaternion.identity));
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning("Join Failed : "+ message);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("Player Entered {0}", other.NickName);
            ChatRPC("<color=yellow>Player Entered " + other.NickName + "</color>");
            if(PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("{0} is master", PhotonNetwork.NickName);
            }
        }
        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("Player Left {0}", other.NickName);
            isReady = false;
            cancel_ready();
            readycnt = 0;
            ChatRPC("<color=yellow>Player Left " + other.NickName + "</color>");
            update_room();
        }
        #endregion

        #region PunRPC
        [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
        void ChatRPC(string msg)
        {
            bool isInput = false;
            for (int i = ChatText.Length; i < 0; i--)
                if (ChatText[i].text == "")
                {
                    isInput = true;
                    ChatText[i].text = msg;
                    break;
                }
            if (!isInput) // 꽉차면 한칸씩 위로 올림
            {
                for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
                ChatText[ChatText.Length - 1].text = msg;
            }
        }
        [PunRPC]
        void ready_count(bool isReadyCheck)
        {
            if(isReadyCheck == true)
            {
                readycnt++;
                Debug.Log("레디 카운트 : " + readycnt);
            }
            else if(isReadyCheck == false)
            {
                readycnt--;
                Debug.Log("레디 카운트 : " + readycnt);
            }
            else
            {
                //작업 없음
            }
        }
        [PunRPC]
        void game_start()
        {
            GameUIManager.Instance.SetMouseLock(true);
            UI_delete();
            UI_delete_map();
            UI_Select(Player_UI);
            // PhotonNetwork.Destroy();
            if(PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
            SceneManager.LoadScene("Stage 1-1", LoadSceneMode.Additive);
            foreach(GameObject playerTemp in playerTemps)
            {
                if(playerTemp == null)
                {
                    //Debug.LogFormat("Continue");
                    continue;
                }
                PhotonNetwork.Destroy(playerTemp);
            }
            Vector3 pos = new Vector3(Random.Range(-20.0f,-10.0f),0.5f,Random.Range(6.0f,-1.0f));
            playerProtypes.Add(PhotonNetwork.Instantiate("Player", pos,Quaternion.identity));

            Debug.LogFormat("플레이어 수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            /*
            if(PhotonNetwork.IsMasterClient)
            {
                if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    player1NameBackground.SetActive(false);
                    player2NameBackground.SetActive(false);
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    player1Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player2NameBackground.SetActive(false);
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 3)
                {
                    player1Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()).NickName;
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    player1Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()).NickName;
                    player3Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext())).NickName;
                }
            }
            if(PhotonNetwork.MasterClient.GetNext() == PhotonNetwork.LocalPlayer)
            {
                //cnt = 1;
                if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2NameBackground.SetActive(false);
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 3)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()).NickName;
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()).NickName;
                    player3Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext())).NickName;
                }
                
            }
            if(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()) == PhotonNetwork.LocalPlayer)
            {
                //cnt = 2;
                if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2NameBackground.SetActive(false);
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 3)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player3Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext())).NickName;
                }
            }
            if(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext())) == PhotonNetwork.LocalPlayer)
            {
                //cnt = 3;
                if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2NameBackground.SetActive(false);
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 3)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player3NameBackground.SetActive(false);
                }
                else if(PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    player1Name.text = PhotonNetwork.MasterClient.NickName;
                    player2Name.text = PhotonNetwork.MasterClient.GetNext().NickName;
                    player3Name.text = PhotonNetwork.MasterClient.GetNextFor(PhotonNetwork.MasterClient.GetNext()).NickName;
                }
            }
            */
            // 추후 수정 필요 
        }
        [PunRPC]
        void game_end()
        {
            // 추후 수정 필요 
            PhotonNetwork.Destroy(playerProtypes[0]);
            GameUIManager.Instance.SetMouseLock(false);
        }
        #endregion
    }
}
