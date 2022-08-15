using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    /*public int numErosionInterations = 1;
    [Range(0, 1)]
    public float inertia = .05f; // At zero, water will instantly change direction to flow downhill. At 1, water will never change direction. 
    public float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
    public float minSedimentCapacity = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain
    [Range(0, 1)]
    public float erodeSpeed = .3f;
    [Range(0, 1)]
    public float depositSpeed = .3f;
    [Range(0, 1)]
    public float evaporateSpeed = .01f;
    public float gravity = 4;
    public int maxDropletLifetime = 30;
    public float deltaMultiplyBySqrt = 0.05f;
    
    public float initialWaterVolume = 1;
    public float initialSpeed = 1;

    [Range(0, 4)]
    public int dropletBrushOuterRings = 1;
    public int seed;

    public WorldGenerator world;
    public LineRenderer lineRend;

    private void Start()
    {
        lineRend.positionCount = maxDropletLifetime;
    }
    public void Erode(int numInterations)
    {
        Random.InitState(seed);
        for (int iteration = 0; iteration < numInterations; iteration++)
        {
            //get a random chunk. chunk y coord will always be at the top of the world
            float fcx = Random.Range(0, world.numChunksWidth);
            int chunkX = (int)Mathf.Round(fcx);
            chunkX *= GameData.ChunkWidth;

            float fcz = Random.Range(0, world.numChunksWidth);
            int chunkZ = (int)Mathf.Round(fcz);
            chunkZ *= GameData.ChunkWidth;


            //get a random x z coord on the terrain map
            float ftx = Random.Range(0, GameData.ChunkWidth);
            int tMapX = (int)Mathf.Round(ftx);

            float ftz = Random.Range(0, GameData.ChunkWidth);
            int tMapZ = (int)Mathf.Round(ftz);

            Chunk chunk = world.GetChunkFromVector3(new Vector3(chunkX, (world.numChunksHeight * GameData.ChunkHeight) - GameData.ChunkHeight, chunkZ));
            //Debug.Log(chunk.chunkObject.name);

            int prevTMapY = GameData.ChunkHeight;
            int worldX = tMapX + chunk.chunkPosition.x;
            int worldY = prevTMapY + chunk.chunkPosition.y;
            int worldZ = tMapZ + chunk.chunkPosition.z;

            bool foundLand = false;
            bool firstContact = true;

            float sediment = 0;
            float speed = initialSpeed;
            float water = initialWaterVolume;

            //Debug.Log("Erosion chunk: " + chunkX.ToString() + " " + ((world.numChunksHeight * GameData.ChunkHeight) - GameData.ChunkHeight).ToString() + " " + chunkZ.ToString());
            //Debug.Log("Terrain map xz coords: " + tMapX.ToString() + " " + tMapZ.ToString());

            for (int lifetime = 0; lifetime < maxDropletLifetime; lifetime++)
            {
                Dictionary<Vector3Int, float> borderingPoints = new Dictionary<Vector3Int, float>();

                //start from the top of the map and go down until we hit land
                if (!foundLand)
                {
                    for (int i = world.numChunksHeight; i > 0; i--)
                    {
                        for (int tMapY = GameData.ChunkHeight; tMapY >= 0; tMapY--)
                        {
                            //found land
                            if (chunk.terrainMap[tMapX, tMapY, tMapZ] <= world.surfaceLevel)
                            {
                                foundLand = true;
                                prevTMapY = tMapY;
                                break;
                            }

                            //need to transfer to the chunk below the current chunk
                            if (tMapY == 0 && !foundLand)
                            {
                                Chunk prevChunk = chunk;
                                chunk = world.GetChunkFromVector3(new Vector3(chunkX, prevChunk.chunkPosition.y - GameData.ChunkHeight, chunkZ));
                                break;
                            }
                        }
                    }
                }
                //TODO: sediment capacity, inertia, check if flowing uphill so pools can form. maybe replace dictionary, dunno if thats making things slow. im stupid cant tell. 
                if (foundLand)
                {
                    if (firstContact)
                        worldY = prevTMapY + chunk.chunkPosition.y;
                    firstContact = false;

                    Debug.Log("Droplet at " + worldX + " " + worldY + " " + worldZ);

                    chunk = world.RoundWorldCoordsToGetChunk(worldX, worldY, worldZ);
                    if (chunk == null) break;

                    lineRend.SetPosition(lifetime, new Vector3(worldX, worldY, worldZ));

                    //Get the lowest points surrounding the current position.
                    int lowestY = worldY;
                    for (int i = 0; i < 8; i++)
                    {
                        int x = worldX;
                        int y = worldY;
                        int z = worldZ;
                        bool hit = false;

                        if (i == 2 || i == 4 || i == 6)
                            x = x + 1;
                        if (i == 3 || i == 5 || i == 7)
                            x = x - 1;
                        if (i == 0 || i == 4 || i == 5)
                            z = z + 1;
                        if (i == 1 || i == 6 || i == 7)
                            z = z - 1;

                        for (int seekY = worldY; seekY >= 0; seekY--)
                        {
                            if (world.GetTerrainMapValueFromWorldPos(x, seekY, z) < world.surfaceLevel && world.GetTerrainMapValueFromWorldPos(x, seekY + 1, z) > world.surfaceLevel)
                            {
                                y = seekY;
                                hit = true;
                                break;
                            }
                        }   

                        if (hit && y <= lowestY)
                        {
                            lowestY = y;
                            borderingPoints.Add(new Vector3Int(x,y,z), world.GetTerrainMapValueFromWorldPos(x,y,z));
                        }
                    }

                    //Get the lowest surrounding point
                    if (borderingPoints.Count > 0)
                    {
                        Vector3Int newKey = new Vector3Int(worldX, worldY, worldZ);
                        float lowestLerpPoint = 100;

                        float oldLerpPoint = (world.GetTerrainMapValueFromWorldPos(worldX, worldY, worldZ) + world.GetTerrainMapValueFromWorldPos(worldX, worldY + 1, worldZ)) / 2f;
                        oldLerpPoint += worldY;

                        int yDif = worldY - lowestY;

                        foreach (KeyValuePair<Vector3Int, float> point in borderingPoints)
                        {
                            if (point.Key.y == lowestY && (point.Value + world.GetTerrainMapValueFromWorldPos(point.Key.x, point.Key.y + 1, point.Key.z) / 2f) + point.Key.y < lowestLerpPoint)
                            {
                                newKey = point.Key;
                                lowestLerpPoint = (point.Value + world.GetTerrainMapValueFromWorldPos(point.Key.x, point.Key.y + 1, point.Key.z)) / 2f;
                                lowestLerpPoint += point.Key.y;
                            }
                        }

                        Debug.Log("oldValue = " + oldLerpPoint.ToString() + " newValue = " + lowestLerpPoint.ToString() + " lowestY = " + lowestY.ToString() + " worldY = " + worldY.ToString());

                        float delta = oldLerpPoint - lowestLerpPoint;
                        delta = deltaMultiplyBySqrt * Mathf.Sqrt(delta);
                        float sedimentCapacity = Mathf.Max(delta * speed * water * sedimentCapacityFactor, minSedimentCapacity);

                        //If carrying more sediment than capacity or if moving uphill
                        if (sediment > sedimentCapacity || oldLerpPoint < lowestLerpPoint)
                        {
                            float amountToDeposit;
                            //If moving uphill, try to fill up to current height
                            if (oldLerpPoint < lowestLerpPoint)
                            {
                                amountToDeposit = Mathf.Min(lowestLerpPoint - oldLerpPoint, sediment);
                            }
                            else //otherwise, deposit a fraction of the excess sediment
                            {
                                amountToDeposit = (sediment - sedimentCapacity) * depositSpeed;
                            }
                            sediment -= amountToDeposit;

                            //Add the sediment to the current point
                            amountToDeposit = Mathf.Sqrt(amountToDeposit);
                            world.AlterTerrain(new Vector3(worldX, worldY, worldZ), false, amountToDeposit, dropletBrushOuterRings, false);
                            Debug.Log("sediment added " + amountToDeposit.ToString() + " sediment = " + sediment.ToString() + " sedimentCapacity = " + sedimentCapacity.ToString());

                        }
                        else
                        {
                            //Erode a fraction of the droplet's current carry capacity
                            float amountToErode = Mathf.Min((sedimentCapacity - sediment) * erodeSpeed, delta);
                            world.AlterTerrain(new Vector3(newKey.x, newKey.y, newKey.z), true, amountToErode, dropletBrushOuterRings, false);
                            Debug.Log("eroded " + amountToErode.ToString() + " sediment = " + sediment.ToString() + " sedimentCapacity = " + sedimentCapacity.ToString());
                            sediment += amountToErode;
                            //Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet (if doing a 3x3 of erosion)

                        }

                        // Update droplet's speed and water content
                        speed = Mathf.Sqrt(speed * speed + delta * gravity);
                        water *= (1 - evaporateSpeed);

                        worldX = newKey.x;
                        worldY = newKey.y;
                        worldZ = newKey.z;

                    }
                    else //There were no surrounding points on the same y level or below compared to the current point. Means it shouldve ended in a pit
                    {
                        world.AlterTerrain(new Vector3(worldX, worldY, worldZ), false, 1f, dropletBrushOuterRings, false);
                        Debug.Log("Droplet in pit");
                        break;
                    }
                    borderingPoints.Clear();

                }
            }
        }
        world.UpdateDirtyChunks();
    }

    */
}
