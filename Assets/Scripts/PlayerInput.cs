using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;

public struct PlayerActionInputs
{
    public bool PrimaryFire; //default key is mouse1
    public bool SecondaryFire; // default mouse2
    public bool Interact; // default f

    public bool ToggleWorldOverview; //r

    public bool Confirm; //enter?

    public bool Left; // a
    public bool Right; // d

    public bool OpenMainMenu; //esc
    public bool OpenWeaponMenu; //tab
    public bool OpenScoreboard; //`
}

public class PlayerInput : NetworkBehaviour
{

    public enum InputState
    {
        WorldOverview,
        ControllingPawn,
        SelectingPawn,
        Spectating,
        None
    }

    InputState currentInputState;

    public float worldCameraSpeed = 2f;

    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";
    

    public CameraSettings pawnCamSettings;
    public CameraSettings overviewCamSettings;
    public CameraSettings selectingPawnCamSettings;
    public CameraSettings projectileCamSettings;
    public Explosion explosionPrefab;

    PawnController currentControlledPawn;
    BattlePlayer player;
    WorldGenerator worldGen;
    CameraController cam;
    public PawnController worldOverviewControl { get; private set; }

    public delegate void SelectingPawnWorldOverviewToggle();
    public static event SelectingPawnWorldOverviewToggle OnSelectingPawnWorldOverviewToggle;

    public delegate void SelectingPawnConfirm();
    public static event SelectingPawnConfirm OnSelectingPawnWorldConfirm;

    private void Start()
    {
        player = GetComponent<BattlePlayer>();
        cam = GetComponent<CameraController>();
        if (player.manager != null && player.manager.identity != null && player.manager.identity.hasAuthority)
        {
            worldGen = FindObjectOfType<WorldGenerator>();
            Cursor.lockState = CursorLockMode.Locked;

            //camFollowPoint = worldOverviewControl.transform;
            //cam.SetFollowTransform(camFollowPoint);

            cam.IgnoredColliders.Clear();
            cam.IgnoredColliders.AddRange(worldOverviewControl.GetComponentsInChildren<Collider>());

            
            worldOverviewControl.TransitionToState(CharacterState.WorldOverview);
            worldOverviewControl.IgnoredColliders.AddRange(worldGen.GetComponentsInChildren<Collider>());


            SetCameraToWorldCenter();
            SwitchInputState(InputState.SelectingPawn);
        }
    }

    public void SetOverviewController(PawnController controller)
    {
        worldOverviewControl = controller;
    }

    private void OnEnable()
    {
        WorldGenerator.OnWorldGenerationComplete += WorldGenFinished;
    }

    private void OnDisable()
    {
        WorldGenerator.OnWorldGenerationComplete -= WorldGenFinished;
    }

    private void Update()
    {
        if(player.manager != null && player.manager.identity != null && player.manager.identity.hasAuthority)
        {
            HandlePlayerInput();
            HandlePawnControllerInput();
            CheckIfPawnCamObstructed();
        }
    }

    private void LateUpdate()
    {
        if (player.manager != null && player.manager.identity != null && player.manager.identity.hasAuthority)
        {
            HandleCameraInput();
        }
    }


    public void WorldGenFinished()
    {
        SwitchInputState(InputState.SelectingPawn);
    }

    public InputState GetCurrentInputState()
    {
        return currentInputState;
    }

    void SwitchInputState(InputState toState)
    {
        currentInputState = toState;
        switch (toState)
        {
            case InputState.WorldOverview:
                currentControlledPawn = worldOverviewControl;
                ChangeCameraConfig(InputState.WorldOverview);
                break;

            case InputState.ControllingPawn:
                if (player.selectedPawn == null)
                {
                    player.SelectPawnByIndex(0);
                }
                currentControlledPawn = player.selectedPawn.controller;
                ChangeCameraConfig(InputState.ControllingPawn);
                break;

            case InputState.SelectingPawn:
                currentControlledPawn = worldOverviewControl;
                ChangeCameraConfig(InputState.SelectingPawn);
                break;

            default:
                break;
        }

    }

    void ChangeCameraConfig(InputState state)
    {
        switch (state)
        {
            case InputState.ControllingPawn:
                CopyCamSettings(pawnCamSettings, cam);
                cam.SetFollowTransform(currentControlledPawn.transform);
                break;

            case InputState.WorldOverview:
                CopyCamSettings(overviewCamSettings, cam);
                cam.SetFollowTransform(worldOverviewControl.CameraFollowPoint);
                break;

            case InputState.SelectingPawn:
                CopyCamSettings(selectingPawnCamSettings, cam);
                if(player.ownedPawns != null && player.ownedPawns.Count > 0)
                    cam.SetFollowTransform(player.ownedPawns[player.GetSelectedPawnIndex()].transform);
                break;
            default:
                break;
        }
    }
    void CopyCamSettings(CameraSettings settings, CameraController cam)
    {
        cam.FollowPointFraming = settings.FollowPointFraming;
        cam.MinDistance = settings.MinDistance;
        cam.MaxDistance = settings.MaxDistance;
        cam.TargetDistance = settings.TargetDistance;
        cam.FollowingSharpness = settings.FollowSharpness;
        cam.DistanceMovementSharpness = settings.DistanceMoveSharpness;
    } 

    void SetCameraToWorldCenter()
    {
        worldOverviewControl.Motor.MoveCharacter(GameManager.GetInstance().worldCenterPoint.position);
    }

    void HandlePawnControllerInput()
    {
        PlayerCharacterInputs pawnInputs = new PlayerCharacterInputs();

        //build the inputs struct
        pawnInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
        pawnInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
        pawnInputs.CameraRotation = cam.Transform.rotation;
        pawnInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
        pawnInputs.JumpHeld = Input.GetKey(KeyCode.Space);
        pawnInputs.CrouchHeld = Input.GetKey(KeyCode.X);


        if (currentInputState == InputState.ControllingPawn)
        {
            currentControlledPawn.SetInputs(ref pawnInputs);
        }
        else if (currentInputState == InputState.WorldOverview)
        {
            worldOverviewControl.SetInputs(ref pawnInputs);
        }
    }

    void HandlePlayerInput()
    {
        //TODO: Organize this
        PlayerActionInputs playerInputs = new PlayerActionInputs();

        playerInputs.PrimaryFire = Input.GetMouseButtonDown(0);
        playerInputs.SecondaryFire = Input.GetMouseButtonDown(1);
        playerInputs.Interact = Input.GetKeyDown(KeyCode.F);
        playerInputs.ToggleWorldOverview = Input.GetKeyDown(KeyCode.R);
        playerInputs.Confirm = Input.GetKeyDown(KeyCode.Return);
        playerInputs.Left = Input.GetKeyDown(KeyCode.A);
        playerInputs.Right = Input.GetKeyDown(KeyCode.D);
        playerInputs.OpenMainMenu = Input.GetKeyDown(KeyCode.Escape);
        playerInputs.OpenScoreboard = Input.GetKeyDown(KeyCode.Tilde);
        playerInputs.OpenWeaponMenu = Input.GetKeyDown(KeyCode.Tab);

        if (playerInputs.PrimaryFire == true)
        {
            if (currentInputState == InputState.ControllingPawn)
            {
                player.PrimaryFireWeapon();
            }
        }

        bool skip = false;
        if(currentInputState == InputState.SelectingPawn)
        {
            if (playerInputs.Left == true)
            {
                player.SelectPreviousPawn();
            }
            if (playerInputs.Right == true)
            {
                player.SelectNextPawn();
            }
            if(playerInputs.Confirm == true)
            {
                SwitchInputState(InputState.ControllingPawn);
                if(OnSelectingPawnWorldConfirm != null)
                {
                    OnSelectingPawnWorldConfirm();
                }

            }
            if (playerInputs.ToggleWorldOverview == true)
            {
                SwitchInputState(InputState.WorldOverview);
                if(OnSelectingPawnWorldOverviewToggle != null)
                {
                    OnSelectingPawnWorldOverviewToggle();
                }
                skip = true;
            }
        }

        if(currentInputState == InputState.WorldOverview)
        {
            if (playerInputs.ToggleWorldOverview == true && !skip)
            {
                SwitchInputState(InputState.SelectingPawn);
                if (OnSelectingPawnWorldOverviewToggle != null)
                {
                    OnSelectingPawnWorldOverviewToggle();
                }
            }
        }
        skip = false;

        if (Input.GetKey(KeyCode.C))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                worldGen.AlterTerrainRadius(hit.point, false, 0.024f, 5f, explosionPrefab);
            }
        }
    }

    void HandleCameraInput()
    {
        float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
        float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
        Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVector = Vector3.zero;
        }
        // Input for zooming the camera (disabled in WebGL because it can cause problems)
        float scrollInput = -Input.GetAxis(MouseScrollInput);

        #if UNITY_WEBGL
        scrollInput = 0f;
        #endif

        // Apply inputs to the camera
        cam.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);
    }

    void CheckIfPawnCamObstructed()
    {
        if (currentInputState == InputState.ControllingPawn)
        {
            RaycastHit hit;

            if (Physics.Raycast(cam.transform.position, currentControlledPawn.CameraFollowPoint.position - cam.transform.position, out hit, cam.MaxDistance + 1f))
            {
                //layer 10 should be player objects
                //this may look backwards. and it is. but i dont know why doing it normally doesnt do what i want. oh well!
                if (hit.collider.gameObject.layer == 10)
                {
                    if (worldGen.worldMaterial.IsKeywordEnabled("_MASKENABLED_ON"))
                        worldGen.worldMaterial.DisableKeyword("_MASKENABLED_ON");
                }
                else
                {
                    if (!worldGen.worldMaterial.IsKeywordEnabled("_MASKENABLED_ON"))
                        worldGen.worldMaterial.EnableKeyword("_MASKENABLED_ON");
                }
            }
        }
        /*else if (currentInputState == InputState.WorldOverview)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, worldOverviewControl.CameraFollowPoint.position - cam.transform.position, out hit, cam.MaxDistance + 1f))
            {
                if (hit.collider.gameObject.layer == 10)
                {
                    if (worldGen.worldMaterial.IsKeywordEnabled("_MASKENABLED_ON"))
                        worldGen.worldMaterial.DisableKeyword("_MASKENABLED_ON");
                }
                else
                {
                    if (!worldGen.worldMaterial.IsKeywordEnabled("_MASKENABLED_ON"))
                        worldGen.worldMaterial.EnableKeyword("_MASKENABLED_ON");
                }
            }
        }*/
    }
}
