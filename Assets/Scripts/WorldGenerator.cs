using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using XNode;
using Random = UnityEngine.Random;

public struct Voxel
{
    public int3 index; //Index for the chunk in the chunkGrid 3d array in WorldGenerator.cs (x,y,z)
    public float3 worldPosition;
    public int3 localGridPosition;
    public int3 gridPosition;
}

public class WorldGenerator : MonoBehaviour
{
    [HideInInspector]
    public int numChunksWidth = 16;
    [HideInInspector]
    public int numChunksLength = 16;
    [HideInInspector]
    public int numChunksHeight = 4;

    public Voxel[,,] voxels;

    public Chunk[,,] chunkGrid;

    [Header("Terrain Settings")]
    public float surfaceLevel = 0f; //Negative is air, Positive is depth.
    public bool smoothTerrain;
    public bool flatShading;
    public float voxelsPerMeter = 1f; //size in unity meters.
    public bool useDualContouring = false;
    float steps;

    public Material worldMaterial;
    public Material backsideMaterial;

    public List<Vector3> pawnSpawns = new List<Vector3>();

    [Header("References")]
    public GameManager gameManager;
    public WorldGenGraph worldGraph;

    public delegate void WorldGenerationCompleted();

    public static event WorldGenerationCompleted OnWorldGenerationComplete;

    GameObject floorMesh;


    private void Start()
    {
        steps = 1 / voxelsPerMeter;
        gameManager = FindObjectOfType<GameManager>();
        DestroyWorldInPlayMode();
    }

    public void Initialize()
    {
        worldGraph.GetEndNode();
        worldGraph.endNode.GetAllNoiseNodes();
        worldGraph.endNode.GetAllFilterNodes();
        numChunksLength = worldGraph.GetNumChunksLength();
        numChunksWidth = worldGraph.GetNumChunksWidth();
        numChunksHeight = worldGraph.GetNumChunksHeight();
        chunkGrid = new Chunk[numChunksWidth,numChunksHeight,numChunksLength];
    }

    /// <summary>
    /// Initializes every chunk, building their mesh.
    /// </summary>
    void Generate()
    {
        DestroyWorldInEditor();

        int counter = 0;

        for (int x = 0; x < numChunksWidth; x++)
        {
            for (int y = 0; y < numChunksHeight; y++)
            {
                for (int z = 0; z < numChunksLength; z++)
                {
                    Vector3 chunkPos = new Vector3((x * MarchingCubesData.ChunkWidth) / voxelsPerMeter, (y * MarchingCubesData.ChunkHeight) / voxelsPerMeter, (z * MarchingCubesData.ChunkWidth) / voxelsPerMeter);

                    var go = new GameObject();
                    go.AddComponent<Chunk>();
                    go.transform.parent = this.transform;
                    var chunk = go.GetComponent<Chunk>();
                    chunk.Initialize(this.gameObject.GetComponent<WorldGenerator>(), chunkPos, smoothTerrain, flatShading);

                    //chunks.Add(chunkPos, new Chunk(this, chunkPos, smoothTerrain, flatShading));
                    //chunks.Add(counter, chunk);
                    chunkGrid[x, y, z] = chunk;
                    chunk.chunkGridPosition = new Vector3Int(x,y,z);
                    chunk.chunkObject.transform.SetParent(transform);
                    chunk.backside.transform.SetParent(chunkGrid[x,y,z].chunkObject.transform);
                    chunk.backside.transform.localPosition = Vector3.zero;
                    chunk.chunkObject.layer = 10;
                    counter++;
                }
            }
        }

        BuildVoxelMap();
        BuildFloorMesh();
        UpdateDirtyChunks();

        if (gameManager != null)
        {
            //center point of world
            gameManager.worldCenterPoint.position = new Vector3(numChunksWidth * MarchingCubesData.ChunkWidth * (1f / voxelsPerMeter) / 2f,
                numChunksHeight * MarchingCubesData.ChunkHeight * (1f / voxelsPerMeter) / 2f,
                numChunksLength * MarchingCubesData.ChunkWidth * (1f / voxelsPerMeter) / 2f);
        }
    }

    /// <summary>
    /// Builds the 3D array of voxels to make my life easier.
    /// </summary>
    void BuildVoxelMap()
    {
        voxels = new Voxel[numChunksWidth * MarchingCubesData.ChunkWidth, numChunksHeight * MarchingCubesData.ChunkHeight, numChunksLength * MarchingCubesData.ChunkWidth];

        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int y = 0; y < voxels.GetLength(1); y++)
            {
                for (int z = 0; z < voxels.GetLength(2); z++)
                {
                    Chunk ch = GetChunkFromWorldPos(new Vector3(x / voxelsPerMeter, y / voxelsPerMeter, z / voxelsPerMeter));
                    //voxels[x, y, z].chunk = GetChunkFromWorldPos(new Vector3(x / voxelsPerMeter , y / voxelsPerMeter , z / voxelsPerMeter));
                    voxels[x, y, z].index = new int3(ch.chunkGridPosition.x, ch.chunkGridPosition.y, ch.chunkGridPosition.z);
                    ch = chunkGrid[voxels[x, y, z].index.x, voxels[x, y, z].index.y, voxels[x, y, z].index.z];
                    voxels[x, y, z].gridPosition = new int3(x, y, z);
                    voxels[x, y, z].worldPosition = new Vector3(x / voxelsPerMeter, y / voxelsPerMeter, z / voxelsPerMeter);

                    voxels[x, y, z].localGridPosition = new int3(x - (ch.chunkGridPosition.x * MarchingCubesData.ChunkWidth)
                        , y - (ch.chunkGridPosition.y * MarchingCubesData.ChunkHeight)
                        , z - (ch.chunkGridPosition.z * MarchingCubesData.ChunkWidth));
                }
            }
        }
    }

    public void DestroyWorldInEditor()
    {
        /*foreach (KeyValuePair<Vector3, Chunk> ch in chunks)
        {
            DestroyImmediate(ch.Value.chunkObject);
        }
        chunks.Clear();*/
        if (chunkGrid != null)
        {
            foreach (Chunk ch in chunkGrid)
            {
                if (ch != null)
                    DestroyImmediate(ch.chunkObject);
            }
            DestroyImmediate(floorMesh);
        }
    }

    public void DestroyWorldInPlayMode()
    {
        /*foreach (KeyValuePair<Vector3, Chunk> ch in chunks)
        {
            Destroy(ch.Value.chunkObject);
        }
        chunks.Clear();*/
        if (chunkGrid != null)
        {
            foreach (Chunk ch in chunkGrid)
            {
                if (ch != null)
                    DestroyImmediate(ch.chunkObject);
            }
            Destroy(floorMesh);
        }
    }

    /// <summary>
    /// Builds the bottom plane with the underside material
    /// </summary>
    void BuildFloorMesh()
    {
        GameObject go = new GameObject("Floor underside mesh");
        go.transform.parent = this.transform;
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();

        Vector3[] vertices = {
            new Vector3(0,0.5f,0),
            new Vector3(0, 0.5f ,numChunksLength * MarchingCubesData.ChunkWidth / voxelsPerMeter ),
            new Vector3(numChunksWidth * MarchingCubesData.ChunkWidth / voxelsPerMeter , 0.5f ,numChunksLength * MarchingCubesData.ChunkWidth / voxelsPerMeter ),
            new Vector3(numChunksWidth * MarchingCubesData.ChunkWidth / voxelsPerMeter, 0.5f, 0)};
        int[] triangles = { 0, 3, 2, 2, 1, 0};

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
        renderer.material = backsideMaterial;

        mesh.RecalculateNormals();
        floorMesh = go;
    }

    /// <summary>
    /// Returns the Chunk the world position is on
    /// </summary>
    /// <param name="pos">The world position</param>
    /// <returns></returns>
    public Chunk GetChunkFromWorldPos(Vector3 pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.z < 0 || pos.x >= (numChunksWidth * MarchingCubesData.ChunkWidth) / voxelsPerMeter
            || pos.y >= (numChunksHeight * MarchingCubesData.ChunkHeight) / voxelsPerMeter
            || pos.z >= (numChunksLength * MarchingCubesData.ChunkWidth) / voxelsPerMeter )
        {
            Debug.Log("GetChunkFromWorldPos null! " + pos.ToString());
            return null;
        }
        else
        {
            int x = Mathf.FloorToInt(pos.x / (MarchingCubesData.ChunkWidth / voxelsPerMeter));
            int y = Mathf.FloorToInt(pos.y / (MarchingCubesData.ChunkHeight / voxelsPerMeter));
            int z = Mathf.FloorToInt(pos.z / (MarchingCubesData.ChunkWidth / voxelsPerMeter));

            #region
            /*Vector3 result = new Vector3(Mathf.Round(pos.x / (MarchingCubesData.ChunkWidth / voxelsPerMeter)) * (MarchingCubesData.ChunkWidth / voxelsPerMeter),
                Mathf.Round(pos.y / (MarchingCubesData.ChunkHeight / voxelsPerMeter)) * (MarchingCubesData.ChunkHeight / voxelsPerMeter),
                Mathf.Round(pos.z / (MarchingCubesData.ChunkWidth / voxelsPerMeter)) * (MarchingCubesData.ChunkWidth / voxelsPerMeter));

            float worldWidth = MarchingCubesData.ChunkWidth / voxelsPerMeter;
            float worldHeight = MarchingCubesData.ChunkHeight / voxelsPerMeter;

            float rx = Mathf.Floor(pos.x);
            rx = Mathf.Floor(rx / worldWidth) * worldWidth;

            float ry = Mathf.Floor(pos.y);
            ry = Mathf.Floor(ry / worldHeight) * worldHeight;

            float rz = Mathf.Floor(pos.z);
            rz = Mathf.Floor(rz / worldWidth) * worldWidth;

            Vector3 result2 = new Vector3(rx,ry,rz);*/
            #endregion

            //if (chunks.TryGetValue(result2, out Chunk value))
            if (chunkGrid[x,y,z] != null)
            {
                return chunkGrid[x,y,z];
            }
            else
            {
                Debug.Log("GetChunkFromWorldPos null! ");
                return null;
            }
        }
    }

    /// <summary>
    /// Returns the closest voxel to a given world position
    /// </summary>
    /// <param name="pos">The world position</param>
    /// <returns></returns>
    Voxel GetClosestVoxelFromWorldPos(Vector3 pos)
    {
        var x = Math.Round(pos.x / steps, MidpointRounding.AwayFromZero) * steps;
        var y = Math.Round(pos.y / steps, MidpointRounding.AwayFromZero) * steps;
        var z = Math.Round(pos.z / steps, MidpointRounding.AwayFromZero) * steps;

        int xi = (int)(x / steps);
        int yi = (int)(y / steps);
        int zi = (int)(z / steps);

        //Debug.Log("Closest center multiple of voxelspermeter steps = x: " + xi + " y: " + yi + " z: " + zi);

        if (xi > voxels.GetLength(0) || yi > voxels.GetLength(1) || zi > voxels.GetLength(2) || xi < 0 || yi < 0 || zi < 0)
        {
            Debug.LogError("Out of range of the array of worldgen.voxels " + xi + " " + yi + " " + zi);
            return voxels[0, 0, 0];
        }
        else
        {
            if (xi == voxels.GetLength(0))
                xi--;
            if (yi == voxels.GetLength(1))
                yi--;
            if (zi == voxels.GetLength(2))
                zi--;
            //Debug.Log("Voxel world pos data = " + voxels[xi,yi,zi].worldPosition);
            return voxels[xi, yi, zi];
        }
        
    }
    
    /// <summary>
    /// Alters the terrain values in a sphere from a given world position.
    /// </summary>
    /// <param name="pos">The center point of the terrain alteration.</param>
    /// <param name="isDigging">True if we are "digging" into the terrain i.e. making holes</param>
    /// <param name="amount">The strength of the alteration. Terrain values should be between 1 and -1, so 1 is considered full strength</param>
    /// <param name="radius">The radius of the terrain alteration.</param>
    /// <param name="exp">A curve that changes the strength of the alteration depending on the distance from the center point</param>
    public void AlterTerrainRadius(Vector3 pos, bool isDigging, float amount, float radius, Explosion exp)
    {
        Chunk centerChunk = GetChunkFromWorldPos(pos);

        int rings = Mathf.CeilToInt(radius);
        float distanceFromCenter;

        var centerVoxel = GetClosestVoxelFromWorldPos(pos);

        for (int x = centerVoxel.gridPosition.x - rings; x < centerVoxel.gridPosition.x + rings + 1; x++)
        {
            for (int y = centerVoxel.gridPosition.y - rings; y < centerVoxel.gridPosition.y + rings + 1; y++)
            {
                for (int z = centerVoxel.gridPosition.z - rings; z < centerVoxel.gridPosition.z + rings + 1; z++)
                {
                    if (x < 0 || y < 0 || z < 0 
                        || x >= (numChunksWidth * MarchingCubesData.ChunkWidth) / voxelsPerMeter
                        || y >= (numChunksHeight * MarchingCubesData.ChunkHeight) / voxelsPerMeter
                        || z >= (numChunksLength * MarchingCubesData.ChunkWidth) / voxelsPerMeter)
                    {
                        //out of bounds

                    }
                    else
                    {
                        var vox = voxels[x, y, z];

                        distanceFromCenter = Vector3.Distance(pos, vox.worldPosition);
                        if (distanceFromCenter <= radius)
                        {
                            //distance of 0 dig amount should be 1, distance of radius should be 0 dig amount

                            float eval = -((distanceFromCenter / radius) - 1);

                            float result = exp.terrainFalloff.Evaluate(eval);
                            //Debug.Log("result is " + result + "|| distance: " + distanceFromCenter);

                            //vox.chunk.AlterTerrain(vox, isDigging, result * amount, false);
                            chunkGrid[vox.index.x, vox.index.y, vox.index.z].AlterTerrain(vox, isDigging, result * amount, false);
                        }
                    }
                }
            }
        }

        UpdateDirtyChunks();
    }

    /// <summary>
    /// Iterates through every chunk and updates the chunk's meshes that have had their terrain map values changed but not the meshes.
    /// </summary>
    public void UpdateDirtyChunks()
    {
        foreach(Chunk ch in chunkGrid)
        {
            if (ch.isDirty)
            {
                ch.CreateMeshData();
                ch.isDirty = false;
            }
        }
    }

    /// <summary>
    /// The main function to generate the world from scratch
    /// </summary>
    public void GenerateWorld()
    {
        Initialize();
        Generate();
        if (Application.isPlaying)
        {
            GeneratePawnSpawnpoints(8);
            if (OnWorldGenerationComplete != null)
            {
                OnWorldGenerationComplete();
            }
        }
    }

    //TODO: Make this more sophisticated. Spawnpoints are just created randomly, they should be in good spots and evenly distanced instead of 2 pawns being right next to eachother.
    //Also a few times it has spawned a pawn underground, or on the ceiling of a cave
    /// <summary>
    /// Creates random points in the world for pawn spawnpoints, they shouldn't be inside the terrain or too high in the sky.
    /// </summary>
    /// <param name="count">The number of spawnpoints to generate</param>
    public void GeneratePawnSpawnpoints(int count)
    {
        pawnSpawns.Clear();
        //a spawn point should be above land, and have enough not-land voxels above for clearence. additionally i could check the steepness of the land below so a pawn doesnt just slide off a cliff
        
        List<Vector3> validSpawns = new List<Vector3>();

        int width = numChunksWidth * MarchingCubesData.ChunkWidth;
        int height = numChunksHeight * MarchingCubesData.ChunkHeight;
        int length = numChunksLength * MarchingCubesData.ChunkWidth;

        //wonder if just generating random points and checking if theyre valid would be better than getting every possible spawnpoint and then randomly picking them

        //generating random points and then checking method
        for (int i = 0; i < count; i++)
        {
            //generate random world positions. they have to be in increments of the voxel size. probably better to just generate terrain map indexes. or not
            
            bool validSpawn = false;

            while (validSpawn == false)
            {
                //random world position to terrain map value.
                float x = Random.Range(1f, (numChunksWidth * MarchingCubesData.ChunkWidth) / voxelsPerMeter);
                float y = Random.Range(1f, (numChunksHeight * MarchingCubesData.ChunkHeight) / voxelsPerMeter);
                float z = Random.Range(1f, (numChunksLength * MarchingCubesData.ChunkWidth) / voxelsPerMeter);

                Vector3 worldPos = new Vector3(x,y,z);

                //wait. why the fuck am i not just doing a raycast up and doing a raycast down instead of getting the terrain map value
                //oh if the random point was deep underground the raycasts wouldnt be able to tell. i only need to tell if the first point is above ground.
                //TODO: make spawns on the very edge of the map not valid. also one time i saw a spawn be in a cave ceiling. need to figure out what caused that
                Chunk ch = GetChunkFromWorldPos(worldPos);
                
                if (ch.GetTerrainMapValueFromWorldPos(worldPos) < 0)
                {
                    //raycast above and below the point. above shouldnt hit anything. below should hit land. then check the steepness of the surface below
                    RaycastHit hit;
                    if (Physics.Raycast(worldPos, Vector3.up, out hit, 2f))
                    {
                        validSpawn = false;
                    }
                    else if (Physics.Raycast(worldPos, Vector3.down, out hit, 2f))
                    {
                        float angle = Vector3.Angle(Vector3.up, hit.normal);
                        if (angle < 32f)
                        {
                            validSpawn = true;
                            validSpawns.Add(worldPos);
                        }
                        else
                        {
                            validSpawn = false;
                        }
                    }
                    #region
                    /* OLD. I dont need to check the above voxels I can just do a raycast. Not deleting incase if I need to do this method and forget how
                     * //check the voxels above if it's air
                    bool aboveIsAir = false;
                    for (int a = 0; a < 3; a++)
                    {
                        Chunk above = GetChunkFromWorldPos(new Vector3(worldPos.x, worldPos.y + ((1/voxelsPerMeter) * a), worldPos.z));
                        float aboveVal = above.GetTerrainMapValueFromWorldPos(new Vector3(worldPos.x, worldPos.y + ((1 / voxelsPerMeter) * a), worldPos.z));
                        if (aboveVal < 0)
                            aboveIsAir = true;
                    }

                    if (aboveIsAir)
                    {
                        //check the steepness of the surface blow
                        RaycastHit hit;
                        
                    }*/
                    #endregion
                }
            }
        }

        for (int i = 0; i < validSpawns.Count; i++)
        {
            pawnSpawns.Add(validSpawns[i]);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = validSpawns[i];
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            go.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
            go.GetComponent<Collider>().enabled = false;
        }
    }

    //Trash I don't need but might want to look back for reference
    #region
    //Deprecated. TODO: Make this work with custom voxel size
    /*public float GetTerrainMapValueFromWorldPos(int x, int y, int z)
    {
        Chunk chunk = GetChunkFromWorldPos(new Vector3(x,y,z));

        if (chunk != null)
        {
            float value = chunk.terrainMap[x % MarchingCubesData.ChunkWidth, y % MarchingCubesData.ChunkHeight, z % MarchingCubesData.ChunkWidth];
            //Debug.Log("GetTerrainMapValueFromWorldPos Value is " + value.ToString());
            return value;
        }
        else
        {
            return 2;
        }
    }*/



    //TODO: Move the alter terrain function here. I should be able to alter terrain just from a world pos, i sohuldnt need to know what chunk to do it on beforehand
    /*public void AlterTerrain(Vector3 worldPos, bool isDigging, float amount, int outerRings, bool updateMesh)
    {
        float newTerrainValue;
        if (isDigging)
            newTerrainValue = amount;
        else newTerrainValue = -amount;

        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);
        int z = Mathf.RoundToInt(worldPos.z);

        Chunk chunk = GetChunkFromWorldPos(worldPos);

        if (chunk != null)
        {
            chunk.AlterTerrain(worldPos, isDigging, amount, updateMesh);

            for (int ring = 0; ring < outerRings; ring++)
            {
                for (int xi = ring - 1; xi < ring + 2; xi++)
                {
                    for (int zi = ring - 1; zi < ring + 2; zi++)
                    {
                        if (xi != 0 && zi != 0)
                        {
                            Chunk c = GetChunkFromWorldPos(new Vector3(x + (xi*voxelsPerMeter), y
                                , z + (zi*voxelsPerMeter)));
                            if (c != null)
                            {
                                c.AlterTerrain(new Vector3(x + (xi*voxelsPerMeter), worldPos.y, z + (zi*voxelsPerMeter)), isDigging, amount, updateMesh);
                            }
                        }
                    }
                }
            }
        }
    }*/

    /*
     * void ConvertRange(float oldVal, float oldMax, float oldMin, float newMax, float newMin)
    {
        float oldRange = oldMax - oldMin;
        float newRange = newMax - newMin;
    }
     * */
    #endregion

}
