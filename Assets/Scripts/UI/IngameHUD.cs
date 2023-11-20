using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IngameHUD : MonoBehaviour
{
    UIManager ui;

    [Header("References")]
    public GameObject teamHealthbarPrefab;

    public GameObject backToPawnSelectionText;
    public TextMeshProUGUI confirmText;
    public GameObject selectPawnText;
    public GameObject worldOverviewText;

    public TextMeshProUGUI projectileForce;
    public TextMeshProUGUI projectileForceBar;
    public TextMeshProUGUI timerText;


    private void Start()
    {
        ui = GetComponentInParent<UIManager>();
    }

    private void OnEnable()
    {
        PlayerInput.OnSelectingPawnWorldOverviewToggle += OnToggleOverview;
        PlayerInput.OnSelectingPawnWorldConfirm += OnSelectPawnConfirm;
    }

    private void OnDisable()
    {
        PlayerInput.OnSelectingPawnWorldOverviewToggle -= OnToggleOverview;
        PlayerInput.OnSelectingPawnWorldConfirm -= OnSelectPawnConfirm;
    }

    void OnToggleOverview()
    {
        if(ui.player.GetComponent<PlayerInput>().GetCurrentInputState() == PlayerInput.InputState.SelectingPawn)
        {
            backToPawnSelectionText.gameObject.SetActive(false);
            worldOverviewText.gameObject.SetActive(true);
        }
        else
        {
            backToPawnSelectionText.gameObject.SetActive(true);
            worldOverviewText.gameObject.SetActive(false);
            
        }
    }

    void OnSelectPawnConfirm()
    {
        if(ui.player.GetComponent<PlayerInput>().GetCurrentInputState() == PlayerInput.InputState.SelectingPawn)
        {
            backToPawnSelectionText.gameObject.SetActive(true);
            confirmText.gameObject.SetActive(true);
            selectPawnText.gameObject.SetActive(true);
            worldOverviewText.gameObject.SetActive(true);
        }
        else
        {
            backToPawnSelectionText.gameObject.SetActive(false);
            confirmText.gameObject.SetActive(false);
            selectPawnText.gameObject.SetActive(false);
            worldOverviewText.gameObject.SetActive(false);
        }
    }
}
