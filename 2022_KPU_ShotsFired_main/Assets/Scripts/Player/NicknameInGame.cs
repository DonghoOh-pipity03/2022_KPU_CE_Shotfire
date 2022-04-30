using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class NicknameInGame : MonoBehaviourPunCallbacks
{
    [SerializeField]
    [Header("Nickname")]
    public TextMeshProUGUI ingame_nickname;
    // Start is called before the first frame update
    void Start()
    {
        ingame_nickname.text = base.photonView.IsMine ? PhotonNetwork.NickName : base.photonView.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
