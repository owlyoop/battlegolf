using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMotor : MonoBehaviour
{
    public Weapon weaponData;

    Camera cam;
    protected WeaponManager weaponManager;
    protected Pawn shooter;

    public int remainingUses;

    public Transform gunEnd;

    public virtual void PrimaryFire()
    {
        if (remainingUses <= 0)
            return;
    }

    public virtual void SecondaryFire()
    {

    }

    public void InitalizeWeapon(Weapon wepData)
    {
        weaponData = wepData;
        remainingUses = weaponData.defaultNumUses;
        weaponManager = GetComponentInParent<WeaponManager>();
    }

    void OnPawnSelect()
    {

    }
}
