using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace PlayerNameInput
{
    public class PlayerNameInput : MonoBehaviour
    {
        #region Private Constants

        public GameObject ID_input;
        [SerializeField] TextMeshProUGUI m_Object ;
        private string Nickname = string.Empty;
        #endregion

        #region MonoBehaviour 
        void Start() {
            // ID_input = GameObject.Find("ID_Input");
        }

        void Update() {
            // Nickname = ID_input.GetComponent<TextMesh>().text;
            // Debug.Log(m_Object.text);
            // Debug.Log(Nickname);
        }
        
        #endregion

        #region Public Methods
        // Update is called once per frame
        
        #endregion
    }
}