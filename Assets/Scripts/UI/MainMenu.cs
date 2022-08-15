using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum MainMenuUIState
{
    TitleScreen,
    JoinGame,
    Options
}

public class MainMenu : MonoBehaviour
{
    public MainMenuUIState uiState;

    public GameObject MainMenuCanvas;
    public GameObject JoinGameCanvas;
    public GameObject OptionsCanvas;

    public static MainMenu instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        uiState = MainMenuUIState.TitleScreen;
    }
    public void OnClickJoinGame()
    {
        //SceneManager.LoadScene("mainscene");
        SwitchUIState(MainMenuUIState.JoinGame);
    }
    public void OnClickExitGame()
    {
        Application.Quit();
    }

    public void OnClickOptions()
    {

    }

    public void SwitchUIState(MainMenuUIState toState)
    {
        OnStateExit(uiState);

        switch (toState)
        {
            case MainMenuUIState.TitleScreen:
                break;

            case MainMenuUIState.JoinGame:
                JoinGameCanvas.SetActive(true);
                break;

            case MainMenuUIState.Options:
                break;
        }

        void OnStateExit(MainMenuUIState fromState)
        {
            switch (fromState)
            {
                case MainMenuUIState.TitleScreen:
                    MainMenuCanvas.SetActive(false);
                    break;

                case MainMenuUIState.JoinGame:
                    JoinGameCanvas.SetActive(false);
                    break;

                case MainMenuUIState.Options:
                    OptionsCanvas.SetActive(false);
                    break;
            }
        }
    }
}
