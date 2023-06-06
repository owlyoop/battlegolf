using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkCalls : NetworkBehaviour
{
    BattlePlayer player;
    private void Start()
    {
        
    }

    [Command]
    public void CmdFireProjectile(int prefabIndex, Vector3 spawnPosition, Quaternion shotRotation, float force, float curve, GameObject shooter)
    {
        //TODO: Validate whether the player can shoot or not here
        RpcFireProjectile(prefabIndex, spawnPosition, shotRotation, force, curve, shooter);
    }

    [ClientRpc]
    public void RpcFireProjectile(int prefabIndex, Vector3 spawnPosition, Quaternion shotRotation, float force, float curve, GameObject shooter)
    {
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            GameObject projGo = Instantiate(lobby.spawnPrefabs[prefabIndex]);
            Projectile proj = projGo.GetComponent<Projectile>();
            proj.transform.position = spawnPosition;
            proj.transform.rotation = shotRotation;
            proj.shooter = shooter.GetComponent<Pawn>();
            proj.Launch(proj.transform.rotation, force, curve);
        }
        
    }
}
