using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// The player represented ingame. For the player represented in the network, see "PlayerManager".
/// </summary>
public class BattlePlayer : NetworkBehaviour
{
    public string playerName;

    [SyncVar]
    public List<Pawn> OwnedPawns = new List<Pawn>();

    [SyncVar]
    int selectedPawnIndex = 0;

    [SyncVar]
    public bool IsReadyToBegin = false;

    public Pawn SelectedPawn;
    public bool HasPawnSelected = false;

    [SyncVar]
    public bool IsTurn;

    public GameObject OverviewControllerPrefab;
    public WeaponManager WepManager { get; private set; }
    public CameraController Cam { get; private set; }
    public UIManager UI { get; private set; }
    public PlayerInput PlayerInputManager { get; private set; }
    public PlayerNetworkCalls NetworkCalls { get; private set; }

    [SyncVar]
    public PlayerManager Manager;

    

    public delegate void FireWeapon();
    public static event FireWeapon OnFireWeapon;

    public delegate void SelectPawn();
    public static event SelectPawn OnPawnSelected;

    private void Awake()
    {
        PlayerInputManager = GetComponent<PlayerInput>();
        Cam = GetComponent<CameraController>();
        NetworkCalls = GetComponent<PlayerNetworkCalls>();
        UI = GetComponentInChildren<UIManager>();
        UI.gameObject.SetActive(false);
        WepManager = GetComponentInChildren<WeaponManager>();

        GameObject overview = Instantiate(OverviewControllerPrefab);
        OverviewControllerPrefab = overview;

        PlayerInputManager.SetOverviewController(OverviewControllerPrefab.GetComponent<PawnController>());
        PlayerInputManager.WorldOverviewControl.TransitionToState(CharacterState.WorldOverview);
    }

    public void SetManager(PlayerManager manager)
    {
        //this.GetComponent<PlayerInput>().worldOverviewControl = this.overviewController.GetComponent<PawnController>();
        this.Manager = manager;
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
        WepManager.GetCurrentWeapon().PrimaryFire();
    }

    public void SetSelectedPawn(int index)
    {
        if (index > OwnedPawns.Count - 1)
        {
            index = OwnedPawns.Count - 1;
        }
        if (index < 0)
        {
            index = 0;
        }
    }

    public void SelectNextPawn()
    {
        selectedPawnIndex++;
        if (selectedPawnIndex > OwnedPawns.Count - 1)
        {
            selectedPawnIndex = 0;
        }

        if (SelectedPawn != null)
        {
            SelectedPawn.OnDeselected();
        }

        SelectedPawn = OwnedPawns[selectedPawnIndex];
        SelectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        //Cam.SetFollowTransform(SelectedPawn.transform);
        Cam.FollowTransform = SelectedPawn.transform;
        //Cam.FollowTransform.localPosition = SelectedPawn.CameraTarget.localPosition;
    }

    public Pawn GetSelectedPawn()
    {
        return SelectedPawn;
    }

    public void SelectPreviousPawn()
    {
        selectedPawnIndex--;
        if(selectedPawnIndex < 0)
        {
            selectedPawnIndex = OwnedPawns.Count - 1;
        }
        if (SelectedPawn != null)
        {
            SelectedPawn.OnDeselected();
        }

        SelectedPawn = OwnedPawns[selectedPawnIndex];
        SelectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        //Cam.SetFollowTransform(SelectedPawn.transform);
        Cam.FollowTransform = SelectedPawn.transform;
        //Cam.FollowTransform.localPosition = SelectedPawn.CameraTarget.localPosition;
    }

    public void SelectPawnByIndex(int index)
    {
        if(index >= OwnedPawns.Count)
        {
            //TODO: throw error
        }
        SelectedPawn = OwnedPawns[index];
        SelectedPawn.OnSelected();

        if (OnPawnSelected != null)
            OnPawnSelected();

        //Cam.SetFollowTransform(SelectedPawn.transform);
        Cam.FollowTransform = SelectedPawn.transform;
        //Cam.FollowTransform.localPosition = SelectedPawn.CameraTarget.localPosition;
    }

    public void OnStartTurn()
    {
        IsTurn = true;

    }

    public void OnEndTurn()
    {
        IsTurn = false;
    }

    public void OnNetworkSpawn()
    {
        if (Manager.isLocalPlayer)
            UI.gameObject.SetActive(true);
    }
}
