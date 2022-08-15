using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    BattlePlayer player;

    List<WeaponMotor> weapons = new List<WeaponMotor>();
    GameManager gm;

    WeaponMotor currentWeapon;

    public void Start()
    {
        gm = GameManager.GetInstance();
        player = GetComponentInParent<BattlePlayer>();
        PopulateWithWeapons();
        currentWeapon = weapons[0];
    }

    void PopulateWithWeapons()
    {
        for (int i = 0; i < GameManager.GetInstance().weaponList.Count; i++)
        {
            GameObject go = Instantiate(GameManager.GetInstance().weaponList[i].weaponPrefab, this.transform);
            WeaponMotor wep = go.GetComponent<WeaponMotor>();
            wep.InitalizeWeapon(GameManager.GetInstance().weaponList[i]);
            weapons.Add(wep);
            wep.gameObject.SetActive(false);
        }
    }

    public void EnableWeapon(int index)
    {
        weapons[index].gameObject.SetActive(true);
    }

    public void DisableAllWeapons()
    {
        foreach (WeaponMotor wep in weapons)
        {
            wep.gameObject.SetActive(false);
        }
    }

    public WeaponMotor GetCurrentWeapon()
    {
        return currentWeapon;
    }

    //Returns the owner of this weapon manager
    public BattlePlayer GetPlayer()
    {
        return player;
    }


    void SetCurrentWeapon()
    {

    }
}
