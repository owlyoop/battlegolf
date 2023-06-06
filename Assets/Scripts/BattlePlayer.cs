using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class BattlePlayer : NetworkBehaviour
{
    public string playerName;

    [SyncVar]
    public List<Pawn> ownedPawns = new List<Pawn>();

    [SyncVar]
    int selectedPawnIndex = 0;

    [SyncVar]
    public bool isReadyToBegin = false;

    public Pawn selectedPawn;
    public bool hasPawnSelected = false;

    [SyncVar]
    public bool isTurn;

    public GameObject overviewController;
    public WeaponManager weaponManager { get; private set; }
    public CameraController cam { get; private set; }
    public UIManager ui { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public PlayerNetworkCalls networkCalls { get; private set; }

    [SyncVar]
    public PlayerManager manager;

    

    public delegate void FireWeapon();
    public static event FireWeapon OnFireWeapon;

    public delegate void SelectPawn();
    public static event SelectPawn OnPawnSelected;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        cam = GetComponent<CameraController>();
        networkCalls = GetComponent<PlayerNetworkCalls>();
        ui = GetComponentInChildren<UIManager>();
        ui.gameObject.SetActive(false);
        weaponManager = GetComponentInChildren<WeaponManager>();

        GameObject overview = Instantiate(overviewController);
        overviewController = overview;

        playerInput.SetOverviewController(overviewController.GetComponent<PawnController>());
        playerInput.worldOverviewControl.TransitionToState(CharacterState.WorldOverview);
    }

    public void SetManager(PlayerManager manager)
    {
        //this.GetComponent<PlayerInput>().worldOverviewControl = this.overviewController.GetComponent<PawnController>();
        this.manager = manager;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public int GetSelectedPawnIndex()
    {
        return selectedPawnIndex;
    }

    public void PrimaryFireWeapon()
    {
        weaponManager.GetCurrentWeapon().PrimaryFire();
    }

    public void SetSelectedPawn(int index)
    {
        if (index > ownedPawns.Count - 1)
        {
            index = ownedPawns.Count - 1;
        }
        if (index < 0)
        {
            index = 0;
        }
    }

    public void SelectNextPawn()
    {
        selectedPawnIndex++;
        if (selectedPawnIndex > ownedPawns.Count - 1)
        {
            selectedPawnIndex = 0;
        }

        if (selectedPawn != null)
        {
            selectedPawn.OnDeselected();
        }

        selectedPawn = ownedPawns[selectedPawnIndex];
        selectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        cam.SetFollowTransform(selectedPawn.transform);
        cam.FollowTransform.localPosition = selectedPawn.cameraTarget.localPosition;
    }


    public void SelectPreviousPawn()
    {
        selectedPawnIndex--;
        if(selectedPawnIndex < 0)
        {
            selectedPawnIndex = ownedPawns.Count - 1;
        }
        if (selectedPawn != null)
        {
            selectedPawn.OnDeselected();
        }

        selectedPawn = ownedPawns[selectedPawnIndex];
        selectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        cam.SetFollowTransform(selectedPawn.transform);
        cam.FollowTransform.localPosition = selectedPawn.cameraTarget.localPosition;
    }

    public void SelectPawnByIndex(int index)
    {
        if(index >= ownedPawns.Count)
        {
            //TODO: throw error
        }
        selectedPawn = ownedPawns[index];
        selectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        cam.SetFollowTransform(selectedPawn.transform);
        cam.FollowTransform.localPosition = selectedPawn.cameraTarget.localPosition;
    }

    public void OnStartTurn()
    {
        isTurn = true;

    }

    public void OnEndTurn()
    {
        isTurn = false;
    }

    public void OnNetworkSpawn()
    {
        if (manager.isLocalPlayer)
            ui.gameObject.SetActive(true);
    }
}
