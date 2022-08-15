using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnHitbox : MonoBehaviour, IDamagable
{
    public Collider col;
    public Pawn pawn;


    //TODO: implement
    public enum BodyPart { }

    public BodyPart bodyPart;

    public void OnDeath()
    {
        
    }

    public void TakeDamage(int damageTaken, GameObject giver, Vector3 damageSourcePoisiton)
    {
        throw new System.NotImplementedException();
    }
}
