using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void TakeDamage(int damageTaken, GameObject giver, Vector3 damageSourcePoisiton);

    void OnDeath();
}
