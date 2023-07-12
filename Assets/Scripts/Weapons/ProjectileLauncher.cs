using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : WeaponMotor
{
    public Projectile ProjectilePrefab;

    float projForce = 0; // 0 to 100;
    bool forceIncreasing = true;

    float projCurve = 0; //-100 to 100
    bool curveIncreasing = true;

    float powerRate = 100;
    float curveRate = 100;

    public AnimationCurve PowerBar;
    public AnimationCurve CurveBar;

    public enum WeaponState
    {
        Disabled,
        WaitingForPrimaryFire,
        ChoosingPower
    }

    WeaponState wepState;

    private void Start()
    {
        wepState = WeaponState.WaitingForPrimaryFire;
    }

    //TODO: remake this
    //the 2 ways power & curve could work would either be like mario golf, or ribbit king
    //mario golf: vertical power bar. button press to build power. button press to stop building
    //secondary bar fills up; you can adjust curve while it fills up
    //
    //ribbit king: horizontal bar. same as above, button press to start, button press to stop
    //curve bar starts automatically. button press to stop.
    //depending on how far left/right it stopped in relation to where the power stopped determines how much curve
    private void Update()
    {
        if (wepState == WeaponState.ChoosingPower)
        {
            var t = Time.deltaTime;

            shooter.Owner.UI.ingameHUD.projectileForce.SetText(projForce.ToString("0.00"));

            if (forceIncreasing)
            {
                projForce += powerRate * t;
                if (projForce > 100)
                    forceIncreasing = false;
            }
            else if (!forceIncreasing)
            {
                projForce -= powerRate * t;
                if (projForce <= 0)
                    forceIncreasing = true;
            }
        }
    }

    public override void PrimaryFire()
    {
        base.PrimaryFire();

        if (wepState == WeaponState.WaitingForPrimaryFire)
        {
            wepState = WeaponState.ChoosingPower;
        }
        else if (wepState == WeaponState.ChoosingPower)
        {
            FireProjectile();
        }
    }

    private void OnEnable()
    {
        BattlePlayer.OnPawnSelected += OnPawnSelected;
    }

    private void OnDisable()
    {
        BattlePlayer.OnPawnSelected -= OnPawnSelected;
    }

    void OnPawnSelected()
    {
        shooter = weaponManager.GetPlayer().SelectedPawn;
        wepState = WeaponState.WaitingForPrimaryFire;
        forceIncreasing = true;
        curveIncreasing = true;
        projForce = 0;
        projCurve = 0;
    }

    public void FireProjectile()
    {
        weaponManager.GetPlayer().NetworkCalls.CmdFireProjectile(2, gunEnd.position, gunEnd.rotation, projForce / 100f, projCurve / 100f, weaponManager.GetPlayer().SelectedPawn.gameObject);
        wepState = WeaponState.WaitingForPrimaryFire;

        projForce = 0;
        projCurve = 0;
        forceIncreasing = true;
        curveIncreasing = true;
        
    }

    public override void SecondaryFire()
    {

    }
}
