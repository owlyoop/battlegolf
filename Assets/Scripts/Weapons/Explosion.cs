using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radius;

    public AnimationCurve terrainFalloff;

    public void Start()
    {
        Invoke("Kill", 10);
    }

    void Kill()
    {
        Destroy(this);
    }
}
