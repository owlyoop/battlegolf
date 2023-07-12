using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LobbyUI : NetworkBehaviour
{
    [Header("UI Elements")]
    public Toggle readyToggle;
    public Button startGameButton;
    public Button sendMessageButton;
    public Button disconnectButton;
    public TMP_InputField chatMessage;
    public Text chatHistory;
    public Scrollbar scrollbar;
    public GridLayoutGroup userListGrid;

    [Header("Prefabs")]
    public TMP_Text usernameListText;

    [Header("Diagnostic - Do Not Edit")]
    public string localPlayerName;

    Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();

    public static LobbyUI instance;

    void Awake()
    {
        instance = this;
    }

    [Command(requiresAuthority = false)]
    public void CmdSend(string message, NetworkConnectionToClient sender = null)
    {
        if (!connNames.ContainsKey(sender))
            connNames.Add(sender, sender.identity.GetComponent<PlayerManager>().PlayerName);

        if (!string.IsNullOrWhiteSpace(message))
            RpcReceive(connNames[sender], message.Trim());
    }

    [ClientRpc]
    public void RpcReceive(string playerName, string message)
    {
        string prettyMessage = playerName == localPlayerName ?
            $"<color=red>{playerName}:</color> {message}" :
            $"<color=blue>{playerName}:</color> {message}";
        AppendMessage(prettyMessage);
    }

    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    // Called by UI element MessageField.OnEndEdit
    public void OnEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
            SendMessage();
    }

    // Called by OnEndEdit above and UI element SendButton.OnClick
    public void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(chatMessage.text))
        {
            CmdSend(chatMessage.text.Trim());
            chatMessage.text = string.Empty;
            chatMessage.ActivateInputField();
        }
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }

    //Called by the StartGameButton UI element
    public void OnClickStartGame()
    {
        FindObjectOfType<BattlegolfNetworkManager>().OnHostClickStartGame();
    }

    public void OnClickDisconnect()
    {

    }
}
