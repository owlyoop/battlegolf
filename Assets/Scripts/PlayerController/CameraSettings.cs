using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Battlegolf/CameraSettings")]
public class CameraSettings : ScriptableObject
{
    [Header("Framing")]
    public Vector2 FollowPointFraming;
    public float FollowSharpness;

    [Header("Distance")]
    public float DefaultDistance;
    public float MinDistance;
    public float MaxDistance;
    public float TargetDistance;
    public float DistanceMovespeed;
    public float DistanceMoveSharpness;

}
