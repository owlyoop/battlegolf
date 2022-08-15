using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    BattlePlayer owner;


    [Header("Projectile Properties")]
    public float lifetime = 15; //how long in seconds before the gameobject gets destroyed.
    public float launchStrength = 1;
    public float gravityStrength = 1;

    public float maxCurve = 64f;
    public float maxForce = 80f;
    public float curveMultiplier = 1;

    public Vector3 launchDirection;
    public Vector3 gravityDirection;
    public Vector3 curveDirection;

    Vector3 currentVelocity;

    [Header("References")]
    public Pawn shooter;
    public Rigidbody rb;
    public Collider col;
    public Explosion explosionPrefab;

    int baseDamage;

    public LayerMask validCollisions;

    Vector3 prevPos;

    private void Start()
    {
        Invoke(nameof(Kill), lifetime);
        rb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(col, shooter.col);
        prevPos = transform.position;
    }

    private void Update()
    {
        UpdateVelocity();
    }

    private void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(prevPos, rb.velocity.normalized, out hit, 0.4f, 1 << LayerMask.NameToLayer("World Geometry")))
        {
            //Debug.Log("Raycast hit! Projectile explosion position: " + this.transform.position + " prevpos" + prevPos);
            GameManager.GetInstance().worldGen.AlterTerrainRadius(this.transform.position, true, 1, 5, explosionPrefab);
            Kill();
        }
        prevPos = transform.position;
    }

    void UpdateVelocity()
    {
        var t = Time.deltaTime;

        //TODO: add a cap on how many degrees it can turn
        //make the rotation start out slower

        rb.velocity = Quaternion.Euler(0, (maxCurve * curveMultiplier) * t, 0) * rb.velocity;

        rb.velocity += (Vector3.down * gravityStrength) * t;
    }

    public void Launch(Quaternion launchDir, float forceMulti, float curveMulti)
    {
        transform.rotation = launchDir;
        rb.velocity = transform.forward * (maxForce * forceMulti);
        curveMultiplier = curveMulti;
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
            GameManager.GetInstance().worldGen.AlterTerrainRadius(this.transform.position, true, 1, 5, explosionPrefab);
            Kill();
        }
    }
}
