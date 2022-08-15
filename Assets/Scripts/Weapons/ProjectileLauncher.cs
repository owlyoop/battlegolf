using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : WeaponMotor
{
    public Projectile projectilePrefab;

    float projForce = 0; // 0 to 100;
    bool forceIncreasing = true;

    float projCurve = 0; //-100 to 100
    bool curveIncreasing = true;

    float powerRate = 100;
    float curveRate = 100;

    public AnimationCurve powerBar;
    public AnimationCurve curveBar;

    public enum WeaponState
    {
        Disabled,
        WaitingForPrimaryFire,
        ChoosingPower,
        ChoosingCurve
    }

    public WeaponState wepState;

    private void Start()
    {
        wepState = WeaponState.WaitingForPrimaryFire;
    }

    private void Update()
    {
        
        if (wepState == WeaponState.ChoosingPower)
        {
            var t = Time.deltaTime;

            shooter.owner.ui.projectileForce.SetText(projForce.ToString("0.00"));

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

        if (wepState == WeaponState.ChoosingCurve)
        {
            var t = Time.deltaTime;

            shooter.owner.ui.projectileForce.SetText(projCurve.ToString("0.00"));

            if (curveIncreasing)
            {
                projCurve += powerRate * t;
                if (projCurve > 100)
                    curveIncreasing = false;
            }
            else if (!curveIncreasing)
            {
                projCurve -= powerRate * t;
                if (projCurve <= -100)
                    curveIncreasing = true;
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
            wepState = WeaponState.ChoosingCurve;
        }
        else if (wepState == WeaponState.ChoosingCurve)
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
        shooter = weaponManager.GetPlayer().selectedPawn;
        wepState = WeaponState.WaitingForPrimaryFire;
        forceIncreasing = true;
        curveIncreasing = true;
        projForce = 0;
        projCurve = 0;
    }

    public void FireProjectile()
    {
        Projectile proj = Instantiate(projectilePrefab);
        proj.transform.position = gunEnd.position;
        proj.transform.rotation = gunEnd.rotation;
        proj.shooter = weaponManager.GetPlayer().selectedPawn;
        proj.Launch(this.transform.rotation, projForce / 100f, projCurve / 100f);

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
