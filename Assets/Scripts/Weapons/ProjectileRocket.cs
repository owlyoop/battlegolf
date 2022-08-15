using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRocket : Projectile
{

    public Explosion explosion;


    private void Start()
    {
        rb.AddForce(transform.forward * launchStrength, ForceMode.VelocityChange);
    }

    private void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

}
