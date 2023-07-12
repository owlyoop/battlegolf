using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WorldNetworkCalls : NetworkBehaviour
{
    public WorldGenerator worldGen;
    public Explosion explosion;

    private void Start()
    {

    }

    [Command]
    public void CmdAlterTerrainRadius(Vector3 pos, bool isDigging, float amount, float radius)
    {
        //RpcAlterTerrainRadius(pos, isDigging, amount, radius, explosion);
    }

    [ClientRpc]
    public void RpcAlterTerrainRadius(Vector3 pos, bool isDigging, float amount, float radius)
    {
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Prefabs/Weapons/Explosion"), pos, new Quaternion(0, 0, 0, 0));
        Explosion exp = explosion.GetComponent<Explosion>();
        exp.radius = radius;
        GetComponent<GameManager>().WorldGen.AlterTerrainRadius(pos, isDigging, amount, radius, exp.GetComponent<Explosion>());
    }
}
