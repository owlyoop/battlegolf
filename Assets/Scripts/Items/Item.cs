using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour, IDamagable
{
    public int health;

    public abstract void OnDeath();

    public abstract void TakeDamage(int damageTaken, GameObject giver, Vector3 damageSourcePoisiton);
}
