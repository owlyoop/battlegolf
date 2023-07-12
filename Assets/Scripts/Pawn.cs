using KinematicCharacterController;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Pawns represent little controllable characters in the game world. They do not represent the player. A player can control multiple pawns.
public class Pawn : NetworkBehaviour
{
    public enum InputState
    {
        Inactive,
        Moving,
        Firing
    }

    [SyncVar]
    public BattlePlayer Owner;

    public Collider col;
    public PawnController Controller;

    public Transform CameraTarget;
    public Transform WeaponParent;

    public WeaponManager WepManager;

    public float Health = 100f;

    public bool IsSelected;

    [SyncVar]
    public uint IdOfBattlePlayer;

    private void Start()
    {
        if (Owner != null && Owner.Manager != null)
        {
            if(Owner.Manager.isLocalPlayer)
            {
                Controller = GetComponent<PawnController>();
                WepManager = Owner.WepManager;
            }
        }
    }

    //TODO: implement
    public void SetOwner(BattlePlayer owner)
    {
        owner.OwnedPawns.Add(this);
        this.Owner = owner;
        
    }

    private void LateUpdate()
    {
        if (IsSelected && Owner.Manager.isLocalPlayer)
        {
            WeaponParent.rotation = Owner.Cam.transform.rotation;
        }
    }

    public void OnSelected()
    {
        IsSelected = true;
        WepManager.transform.SetParent(WeaponParent);
        WepManager.transform.localPosition = Vector3.zero;
        WepManager.transform.localRotation = new Quaternion(0,0,0,0);
        WepManager.EnableWeapon(0);
    }

    public void OnDeselected()
    {
        IsSelected = false;
    }
}
