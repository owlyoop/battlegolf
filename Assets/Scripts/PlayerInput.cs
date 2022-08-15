using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerActionInputs
{
    public bool PrimaryFire; //default key is mouse1
    public bool SecondaryFire; // default mouse2
    public bool Interact; // default f

    public bool NextPawn; //r

    public bool Confirm; // e

    public bool OpenMainMenu; //esc
    public bool OpenWeaponMenu; //tab
    public bool OpenScoreboard; //`
}

public class PlayerInput : MonoBehaviour
{

    public enum InputState
    {
        WorldOverview,
        ControllingPawn,
        None
    }
    InputState currentInputState;

    public float worldCameraSpeed = 2f;

    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";

    [Header("References")]
    PawnController currentControlledPawn;
    public PawnController worldOverviewControl;
    public Transform camFollowPoint;
    BattlePlayer player;
    public CameraController cam;
    WorldGenerator worldGen;
    Material terrainInstanced;
    public Explosion explosionPrefab;

    private void Start()
    {
        player = GetComponent<BattlePlayer>();
        worldGen = FindObjectOfType<WorldGenerator>();
        terrainInstanced = worldGen.worldMaterial;
        Cursor.lockState = CursorLockMode.Locked;

        camFollowPoint = worldOverviewControl.transform;
        cam.SetFollowTransform(camFollowPoint);

        cam.IgnoredColliders.Clear();
        //cam.IgnoredColliders.AddRange(currentControlledPawn.GetComponentsInChildren<Collider>());
        cam.IgnoredColliders.AddRange(worldOverviewControl.GetComponentsInChildren<Collider>());

        currentInputState = InputState.WorldOverview;
        //inputState = InputState.ControllingPawn;
        worldOverviewControl.TransitionToState(CharacterState.WorldOverview);
        worldOverviewControl.IgnoredColliders.AddRange(worldGen.GetComponentsInChildren<Collider>());

        SetCameraToWorldCenter();

    }

    private void Update()
    {
        HandlePlayerInput();
        HandleCameraInput();
        HandlePawnControllerInput();
        CheckIfPawnCamObstructed();

    }

    void SwitchInputState(InputState toState)
    {
        switch (toState)
        {
            case InputState.WorldOverview:

                break;

            case InputState.ControllingPawn:

                break;

            default:

                break;
        }
    }

    void ChangeCameraConfig(InputState state)
    {
        //TODO: DONT HARDCODE THIS
        switch (state)
        {
            case InputState.ControllingPawn:
                cam.FollowPointFraming = new Vector2(1, 1.43f);
                cam.MinDistance = 4;
                cam.MaxDistance = 4;
                cam.TargetDistance = 4;
                cam.FollowingSharpness = 16;
                cam.DistanceMovementSharpness = 8;
                break;

            case InputState.WorldOverview:
                cam.FollowPointFraming = new Vector2(0, 1);
                cam.MinDistance = 20;
                cam.MaxDistance = 64;
                cam.TargetDistance = 32;
                cam.FollowingSharpness = 1000;
                cam.DistanceMovementSharpness = 8f;
                break;

            default:
                break;
        }
            
    }

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
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
        PlayerActionInputs playerInputs = new PlayerActionInputs();

        playerInputs.PrimaryFire = Input.GetMouseButtonDown(0);
        playerInputs.SecondaryFire = Input.GetMouseButtonDown(1);
        playerInputs.Interact = Input.GetKeyDown(KeyCode.F);
        playerInputs.NextPawn = Input.GetKeyDown(KeyCode.R);
        playerInputs.Confirm = Input.GetKeyDown(KeyCode.E);
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

        if (Input.GetKey(KeyCode.C))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                worldGen.AlterTerrainRadius(hit.point, false, 0.024f, 5f, explosionPrefab);
            }
        }

        if (playerInputs.NextPawn == true)
        {
            player.SelectNextPawn();
            currentInputState = InputState.ControllingPawn;
            currentControlledPawn = player.selectedPawn.controller;
            ChangeCameraConfig(InputState.ControllingPawn);
        }

        if (playerInputs.OpenWeaponMenu == true)
        {

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
