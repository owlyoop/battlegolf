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
    public BattlePlayer owner;

    public Collider col;
    public PawnController controller;

    public Transform cameraTarget;
    public Transform weaponParent;

    public WeaponManager weaponManager;

    public float health = 100f;

    public bool isSelected;

    [SyncVar]
    public uint IdOfBattlePlayer;

    private void Start()
    {
        if (owner != null && owner.manager != null)
        {
            if(owner.manager.isLocalPlayer)
            {
                controller = GetComponent<PawnController>();
                weaponManager = owner.weaponManager;
            }
        }
    }

    //TODO: implement
    public void SetOwner(BattlePlayer owner)
    {
        owner.ownedPawns.Add(this);
        this.owner = owner;
        
    }

    private void LateUpdate()
    {
        if (isSelected && owner.manager.isLocalPlayer)
        {
            weaponParent.rotation = owner.cam.transform.rotation;
        }
    }

    public void OnSelected()
    {
        isSelected = true;
        weaponManager.transform.SetParent(weaponParent);
        weaponManager.transform.localPosition = Vector3.zero;
        weaponManager.transform.localRotation = new Quaternion(0,0,0,0);
        weaponManager.EnableWeapon(0);
    }

    public void OnDeselected()
    {
        isSelected = false;
    }
}
