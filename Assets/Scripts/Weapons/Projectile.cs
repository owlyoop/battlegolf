using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    BattlePlayer owner;


    [Header("Projectile Properties")]
    public float Lifetime = 15; //how long in seconds before the gameobject gets destroyed.
    public float LaunchStrength = 1;
    public float GravityStrength = 1;

    public float MaxCurve = 64f;
    public float MaxForce = 80f;
    public float CurveMultiplier = 1;

    public Vector3 LaunchDirection;
    public Vector3 GravityDirection;
    public Vector3 CurveDirection;

    Vector3 currentVelocity;

    [Header("References")]
    public Pawn Shooter;
    public Rigidbody rb;
    public Collider col;
    public Explosion ExplosionPrefab;
    public GameObject CameraFollowPoint;

    int baseDamage;

    public LayerMask ValidCollisions;

    Vector3 prevPos;

    private void Start()
    {
        Invoke(nameof(Kill), Lifetime);
        rb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(col, Shooter.col);
        prevPos = transform.position;
        owner = Shooter.Owner;
        CameraFollowPoint.transform.position = this.transform.position;
        CameraFollowPoint.transform.rotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        UpdateVelocity();
    }

    private void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(prevPos, rb.velocity.normalized, out hit, 0.4f, 1 << LayerMask.NameToLayer("World Geometry")))
        {
            //Debug.Log("Raycast hit! Projectile explosion position: " + this.transform.position + " prevpos" + prevPos);
            if (Shooter.Owner != null && Shooter.Owner.isServer)
            {
                GameManager.GetInstance().WorldNetwork.RpcAlterTerrainRadius(this.transform.position, true, 1, 5);
            }
            Kill();
        }
        prevPos = transform.position;
        CameraFollowPoint.transform.position = this.transform.position;
        CameraFollowPoint.transform.rotation = Quaternion.identity;
    }

    void UpdateVelocity()
    {
        var t = Time.deltaTime;

        //TODO: add a cap on how many degrees it can turn
        //make the curve behave like curve in a golf game. gotta research that

        rb.velocity = Quaternion.Euler(0, (MaxCurve * CurveMultiplier) * t, 0) * rb.velocity;

        rb.velocity += (Vector3.down * GravityStrength) * t;
    }

    public void Launch(Quaternion launchDir, float forceMulti, float curveMulti)
    {
        transform.rotation = launchDir;
        rb.velocity = transform.forward * (MaxForce * forceMulti);
        CurveMultiplier = curveMulti;
    }

    void Kill()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == 6 || col.gameObject.layer == 10)
        {
            //-Debug.Log("Projectile explosion position: " + this.transform.position);
            //GameManager.GetInstance().worldGen.AlterTerrainRadius(this.transform.position, true, 1, 5, explosionPrefab);
            if (Shooter.Owner != null && Shooter.Owner.isServer)
            {
                GameManager.GetInstance().WorldNetwork.RpcAlterTerrainRadius(this.transform.position, true, 1, 5);
                owner.NetworkCalls.RpcOnProjectileKilled();
            }
            Kill();
        }
    }
}
