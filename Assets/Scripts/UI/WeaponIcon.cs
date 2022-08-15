using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIcon : MonoBehaviour
{

    TextMesh numStocksText;
    Sprite icon;
    Weapon weapon;

    public void BuildWeaponIcon(Weapon wep)
    {
        icon = wep.icon;
        numStocksText.text = wep.defaultNumUses.ToString();
    }
}
