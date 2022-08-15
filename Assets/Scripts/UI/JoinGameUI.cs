using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public Button hostButton;
    public Button clientButton;
    public Button backButton;

    public static JoinGameUI instance;

    private void Awake()
    {
        instance = this;
    }

    public void ToggleButtons(string username)
    {
        hostButton.interactable = !string.IsNullOrWhiteSpace(username);
        clientButton.interactable = !string.IsNullOrWhiteSpace(username);
    }

    public void OnClickBack()
    {
        MainMenu.instance.SwitchUIState(MainMenuUIState.TitleScreen);
    }

    public void OnClickHostGame()
    {

    }

    public void OnClickJoin()
    {

    }
}
