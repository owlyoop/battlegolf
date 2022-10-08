using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class BattlePlayer : NetworkBehaviour
{
    public string playerName;

    public List<Pawn> ownedPawns = new List<Pawn>();

    [SyncVar]
    int selectedPawnIndex = 0;

    [SyncVar]
    public bool isReadyToBegin = false;

    public Pawn selectedPawn;
    public bool hasPawnSelected = false;

    [Header("References")]
    public WeaponManager weaponManager;
    public CameraController cam;
    public UIManager ui;
    public GameObject overviewController;
    PlayerInput playerInput;
    public PlayerManager manager;

    public delegate void FireWeapon();
    public static event FireWeapon OnFireWeapon;

    public delegate void SelectPawn();
    public static event SelectPawn OnPawnSelected;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        cam = GetComponent<CameraController>();
        playerInput.cam = cam;

        GameObject overview = Instantiate(overviewController);
        overviewController = overview;

        //TODO: find out why this doesnt work in playerinput

        playerInput.worldOverviewControl = overviewController.GetComponent<PawnController>();
        playerInput.worldOverviewControl.TransitionToState(CharacterState.WorldOverview);
        playerInput.camFollowPoint = playerInput.worldOverviewControl.transform;
        playerInput.cam.SetFollowTransform(playerInput.camFollowPoint);


        if (NetworkManager.singleton is BattlegolfNetworkManager room && room.currentState == BattlegolfNetworkState.Battle)
        {
            //room.battlePlayers.Add(this);
        }


    }

    public void Initialize()
    {

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
}
