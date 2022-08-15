using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerUIState
{
    MainMenu,
    EscMenu,
    IngameHUD
}

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public BattlePlayer player;
    public GridLayoutGroup weaponGrid;
    public GameObject weaponIconPrefab;
    public GameObject teamHealthbarPrefab;
    public GameObject projectileLauncherHUD;

    public TextMeshProUGUI projectileForce;


    private void Start()
    {
        player = GetComponentInParent<BattlePlayer>();
    }
}
