using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LobbyPlayerUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public GameObject readyObject;

    public void OnUsernameTextChanged(string newUsername)
    {
        usernameText.text = newUsername;
    }

    public void OnReadyStateChanged(bool newReady)
    {
        readyObject.SetActive(newReady);
    }
}
