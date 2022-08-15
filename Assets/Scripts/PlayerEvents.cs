using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public delegate void StartTurn();
    public static event StartTurn OnStartTurn;

    public delegate void EndTurn();
    public static event EndTurn OnEndTurn;

    public delegate void StartMovement();
    public static event StartMovement OnStartMovement;
}
