using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Vertex
{
    public float3 position;
    public int2 edgeID;
}

public class Chunk : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRend;
    public GameObject ChunkObject;
    public WorldGenerator World;

    public GameObject Backside;
    MeshRenderer backsideMeshRend;
    MeshFilter backsideMesh;
    Mesh mesh;

    public Vector3 ChunkPosition;
    public Vector3Int ChunkGridPosition;

    int width { get { return MarchingCubesData.ChunkWidth; } }
    int height { get { return MarchingCubesData.ChunkHeight; } }


    List<Vector3> processedVertices = new List<Vector3>();

    List<int> processedTriangles = new List<int>();

    //List<Vertex> vertices = new List<Vertex>();
    int maxLength;

    Vertex[] vertexCopy;
    NativeArray<Vertex> vertices;
    [ReadOnly]
    NativeArray<float> terMap;
    MarchingCubesJob marchingCubesJob;

    Dictionary<int2, int> vertexMap;

    public float[,,] TerrainMap;

    public bool IsDirty; //True if the mesh hasnt been updated to match the terrain map values
    bool smoothTerrain;
    bool flatShading;

    /// <summary>
    /// Called when WorldGenerator iterates through every chunk for the initial world generation.
    /// </summary>
    public void Initialize(WorldGenerator world, Vector3 _position, bool _smoothTerrain, bool _flatShading)
    {
        TerrainMap = new float[width + 1, height + 1, width + 1];

        vertexMap = new Dictionary<int2, int>();

        maxLength = MarchingCubesData.ChunkHeight * MarchingCubesData.ChunkWidth * MarchingCubesData.ChunkWidth * 15;

        vertexCopy = new Vertex[maxLength];

        smoothTerrain = _smoothTerrain;
        flatShading = _flatShading;

        ChunkObject = this.gameObject;
        ChunkObject.name = string.Format("Chunk {0}, {1}, {2}", _position.x, _position.y, _position.z);
        ChunkPosition = _position;
        ChunkObject.transform.position = ChunkPosition;
        ChunkObject.transform.tag = "Terrain";
        Backside = new GameObject();
        Backside.name = ChunkObject.name + " backside";
        Backside.transform.parent = ChunkObject.transform;
        this.World = world;

        meshFilter = ChunkObject.AddComponent<MeshFilter>();
        meshCollider = ChunkObject.AddComponent<MeshCollider>();
        meshRend = ChunkObject.AddComponent<MeshRenderer>();
        meshRend.material = world.WorldMaterial;
        meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        mesh = new Mesh();

        backsideMesh = Backside.AddComponent<MeshFilter>();
        backsideMeshRend = Backside.AddComponent<MeshRenderer>();
        backsideMeshRend.material = world.BacksideMaterial;

        this.gameObject.layer = 10;

        GenerateTerrainMap();
        CreateMeshData();
    }

    /// <summary>
    /// Creates the values for the terrainmap for this chunk, then builds the mesh
    /// </summary>
    public void CreateMeshData()
    {
        ClearMeshData();
        
        vertices = new NativeArray<Vertex>(maxLength, Allocator.TempJob);

        terMap = new NativeArray<float>((width + 1) * (height + 1) * (width + 1), Allocator.TempJob);

        for (int x = 0; x < width +1; x++)
        {
            for (int y = 0; y < height +1; y++)
            {
                for (int z = 0; z < width +1; z++)
                {
                    terMap[x * 17 * 17 + y * 17 + z] = TerrainMap[x, y, z];
                }
            }
        }

        NativeCounter vertexCountCounter = new NativeCounter(Allocator.TempJob);

        MarchingCubesJob marchingCubesJob = new MarchingCubesJob
        {
            numVoxelsHeight = height,
            numVoxelsWidth = width,
            surfaceLevel = World.SurfaceLevel,
            smoothTerrain = World.SmoothTerrain,
            VertexCountCounter = vertexCountCounter,
            jobVertices = vertices,
            terMap = terMap
        };

        JobHandle jobHandle = marchingCubesJob.Schedule();

        //TODO: delete
        //old code that doesnt use multithreading. keeping for reference just incase if i forget things
        #region
        //Look through each cube of terrain
        /*for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    //Float for each corner of a cube. get the value from our terrainmap
                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        cube[i] = terrainMap[x + MarchingCubesData.CornerTable[i].x, y + MarchingCubesData.CornerTable[i].y, z + MarchingCubesData.CornerTable[i].z];
                    }

                    //Pass value into MarchCube
                    MarchCube(new Vector3Int(x,y,z), cube);
                }
            }
        }*/
        #endregion

        jobHandle.Complete();

        int vertexCount = marchingCubesJob.VertexCountCounter.Count;

        vertices.CopyTo(vertexCopy);
        vertices.Dispose();
        terMap.Dispose();
        vertexCountCounter.Dispose();

        BuildMeshData();
        BuildMesh();
        //BuildFloorUndersideMesh();

    }

    private void OnDisable()
    {
        //TODO: delete. not needed with the jobs
        //vertices.Dispose();
        //terMap.Dispose();
    }

    void ClearMeshData()
    {
        vertexMap.Clear();
        processedVertices.Clear();
        processedTriangles.Clear();
        if (mesh != null) mesh.Clear();
    }

    void BuildMesh()
    {
        mesh.MarkDynamic();

        mesh.SetVertices(processedVertices);
        mesh.SetTriangles(processedTriangles,0,false);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        if (processedVertices.Count >= 3)
            meshCollider.sharedMesh = mesh;
        backsideMesh.mesh = mesh;
    }

    /// <summary>
    /// Generates the values for the terrainmap 3d array using Xnode
    /// </summary>
    public void GenerateTerrainMap()
    {
        float thisPoint;
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    thisPoint = World.WorldGraph.endNode.GenerateTerrainMap(new Vector3(x + ChunkPosition.x * World.VoxelsPerMeter
                        , y + ChunkPosition.y * World.VoxelsPerMeter
                        , z + ChunkPosition.z * World.VoxelsPerMeter));

                    TerrainMap[x, y, z] = thisPoint;
                }
            }
        }
        #region
        /*float[,,] map = world.worldGraph.endNode.GenerateFinalTerrainMap();
        for (int x = 0; x < width + 1; x++)
        {
            for (int z = 0; z < width + 1; z++)
            {
                for (int y = 0; y < height + 1; y++)
                {
                    terrainMap[x, y, z] = map[x + (int)chunkPosition.x
                        , y + (int)chunkPosition.y
                        , z + (int)chunkPosition.z];
                }
            }
        }*/
        #endregion
    }

    //TODO: look into the job system for generating the terrain map with xnode. Check if that's even a performance impact
    public struct BuildTerrainMapJob : IJobParallelFor
    {
        float thisPoint;
        //x + WIDTH * (y + DEPTH * z) 
        public void Execute(int i)
        {
            /*thisPoint = world.worldGraph.endNode.GenerateTerrainMap(new Vector3(x + chunkPosition.x * world.voxelsPerMeter
                        , y + chunkPosition.y * world.voxelsPerMeter
                        , z + chunkPosition.z * world.voxelsPerMeter));

            terrainMap[x, y, z] = thisPoint;*/

            //Noise.Evaluate(new Vector3());
        }
    }

    /// <summary>
    /// Alters the terrain of a single voxel in the terrain map
    /// </summary>
    /// <param name="voxel">The voxel to alter</param>
    /// <param name="isDigging">True if we are subtracting the value, "digging" into the terrain</param>
    /// <param name="amount">How much to reduce the voxel's value by</param>
    /// <param name="updateMesh">True if we should update the mesh after this finishes. Should be false if you are altering more than 1 voxel at a time. False marks the mesh dirty.</param>
    public void AlterTerrain(Voxel voxel, bool isDigging, float amount, bool updateMesh)
    {
        float newTerrainValue;
        if (isDigging)
            newTerrainValue = -amount;
        else newTerrainValue = amount;

        var x = voxel.localGridPosition.x;
        var y = voxel.localGridPosition.y;
        var z = voxel.localGridPosition.z;

        //tmap length is 17

        TerrainMap[x, y, z] += newTerrainValue;

        int border = 0;
        Vector3Int newGridPos = ChunkGridPosition;
        int xlocal = -1;
        int ylocal = -1;
        int zlocal = -1;
        bool isvalid = true;
        
        if (x == 0)
        {
            xlocal = MarchingCubesData.ChunkWidth;
            newGridPos.x--;
            border++;
        }
        else if (x == MarchingCubesData.ChunkWidth)
        {
            xlocal = 0;
            newGridPos.x++;
            border++;
        }

        if (y == 0)
        {
            ylocal = MarchingCubesData.ChunkHeight;
            newGridPos.y--;
            border++;
        }
        else if (y == MarchingCubesData.ChunkHeight)
        {
            ylocal = 0;
            newGridPos.y++;
            border++;
        }

        if (z == 0)
        {
            zlocal = MarchingCubesData.ChunkWidth;
            newGridPos.z--;
            border++;
        }
        else if (z == MarchingCubesData.ChunkWidth)
        {
            zlocal = 0;
            newGridPos.z++;
            border++;
        }

        if (newGridPos.x < 0 || newGridPos.x > World.NumChunksWidth || newGridPos.y < 0 || newGridPos.y > World.NumChunksHeight || newGridPos.z < 0 || newGridPos.z > World.NumChunksLength)
            isvalid = false;

        if (border == 1 && isvalid)
        {
            if (xlocal != -1)
            {
                AlterBorderChunk(newGridPos.x, ChunkGridPosition.y, ChunkGridPosition.z, xlocal, y, z, newTerrainValue, updateMesh);
            }                                                            
            else if (ylocal != -1)                                        
            {
                AlterBorderChunk(ChunkGridPosition.x, newGridPos.y, ChunkGridPosition.z, x, ylocal, z, newTerrainValue, updateMesh);
            }                                                            
            else if (zlocal != -1)                                        
            {
                AlterBorderChunk(ChunkGridPosition.x, ChunkGridPosition.y, newGridPos.z, x, y, zlocal, newTerrainValue, updateMesh);
            }
        }
        else if (border == 2 && isvalid)
        {
            if (xlocal == -1)
            {
                AlterBorderChunk(newGridPos.x, newGridPos.y, ChunkGridPosition.z, x, ylocal, z, newTerrainValue, updateMesh);
                AlterBorderChunk(newGridPos.x, ChunkGridPosition.y, newGridPos.z, x, y, zlocal, newTerrainValue, updateMesh);
                AlterBorderChunk(newGridPos.x, newGridPos.y, newGridPos.z, x, ylocal, zlocal, newTerrainValue, updateMesh);
            }
            else if (ylocal == -1)
            {
                AlterBorderChunk(newGridPos.x, newGridPos.y, ChunkGridPosition.z, xlocal, y, z, newTerrainValue, updateMesh);
                AlterBorderChunk(ChunkGridPosition.x, newGridPos.y, newGridPos.z, x, y, zlocal, newTerrainValue, updateMesh);
                AlterBorderChunk(newGridPos.x, newGridPos.y, newGridPos.z, xlocal, y, zlocal, newTerrainValue, updateMesh);
            }
            else if (zlocal == -1)
            {
                AlterBorderChunk(newGridPos.x, ChunkGridPosition.y, newGridPos.z, xlocal, y, z, newTerrainValue, updateMesh);
                AlterBorderChunk(ChunkGridPosition.x, newGridPos.y, newGridPos.z, x, ylocal, z, newTerrainValue, updateMesh);
                AlterBorderChunk(newGridPos.x, newGridPos.y, newGridPos.z, xlocal, ylocal, z, newTerrainValue, updateMesh);
            }
        }
        else if (border == 3 && isvalid)
        {
            AlterBorderChunk(newGridPos.x,newGridPos.y, newGridPos.z, xlocal, ylocal, zlocal, newTerrainValue, updateMesh);

            AlterBorderChunk(newGridPos.x, ChunkGridPosition.y, ChunkGridPosition.z, xlocal, y, z, newTerrainValue, updateMesh);
            AlterBorderChunk(ChunkGridPosition.x, newGridPos.y, ChunkGridPosition.z, x, ylocal, z, newTerrainValue, updateMesh);
            AlterBorderChunk(ChunkGridPosition.x, ChunkGridPosition.y, newGridPos.z, x, y, zlocal, newTerrainValue, updateMesh);

            AlterBorderChunk(newGridPos.x, newGridPos.y, ChunkGridPosition.z, xlocal, ylocal, z, newTerrainValue, updateMesh);
            AlterBorderChunk(newGridPos.x, ChunkGridPosition.y, newGridPos.z, xlocal, y, zlocal, newTerrainValue, updateMesh);
            AlterBorderChunk(ChunkGridPosition.x, newGridPos.y, newGridPos.z, x, ylocal, zlocal, newTerrainValue, updateMesh);
        }

        if (updateMesh)
        {
            //voxel.chunk.CreateMeshData();
            World.ChunkGrid[voxel.index.x, voxel.index.y, voxel.index.z].CreateMeshData();
        }
        else
        {
            //voxel.chunk.isDirty = true;
            World.ChunkGrid[voxel.index.x, voxel.index.y, voxel.index.z].IsDirty = true;
        }
    }

    void AlterBorderChunk(int chunkGridX, int chunkGridY, int chunkGridZ, int localX, int localY, int localZ, float newTerrainValue, bool updateMesh)
    {
        // Chunk ch = world.chunkGrid[chunkGridX, chunkGridY, chunkGridZ];

        World.ChunkGrid[chunkGridX, chunkGridY, chunkGridZ].TerrainMap[localX, localY, localZ] += newTerrainValue;

        if (updateMesh)
        {
            World.ChunkGrid[chunkGridX, chunkGridY, chunkGridZ].CreateMeshData();
        }
        else
        {
            World.ChunkGrid[chunkGridX, chunkGridY, chunkGridZ].IsDirty = true;
        }
    }

    /*
    /// <summary>
    /// Raises or Digs the terrain at a single point. Will check if the point is on the chunk border
    /// </summary>
    /// <param name="isDigging"></param>
    /// <param name="pos"> Global position </param>
    /// <param name="updateMesh">Update the mesh after altering the terrain map value. If false, chunk will be marked as dirty</param>
    public void AlterTerrain(Vector3 pos, bool isDigging, float amount, bool updateMesh)
    {
        float newTerrainValue;
        if (isDigging)
            newTerrainValue = -amount;
        else newTerrainValue = amount;

        var worldPos = pos;

        pos -= chunkPosition;
        Vector3Int tMap = new Vector3Int((int)Mathf.Round(pos.x * world.voxelsPerMeter), (int)Mathf.Round(pos.y * world.voxelsPerMeter), (int)Mathf.Round(pos.z * world.voxelsPerMeter));

        int mapXLength = terrainMap.GetLength(0);
        int mapYHeight = terrainMap.GetLength(1);
        int mapZLength = terrainMap.GetLength(2);

        //Debug.Log(chunkObject.name + " altered at " + tMap.x.ToString() + " " + tMap.y.ToString() + " " + tMap.z.ToString());
        terrainMap[tMap.x, tMap.y, tMap.z] += newTerrainValue;


        float worldX = chunkPosition.x;
        float worldY = chunkPosition.y;
        float worldZ = chunkPosition.z;


        //if the point is on an end face, 1 extra chunk has to be updated (not including original chunk)
        //if on edge, 3 extra
        //if on corner, 7 extra

        int border = 0;
        int xValue = -1;
        int yValue = -1;
        int zValue = -1;

        if (tMap.x == 0)
        {
            worldX = chunkPosition.x - (MarchingCubesData.ChunkWidth / world.voxelsPerMeter);
            border++;
            xValue = MarchingCubesData.ChunkWidth;
        }
        else if (tMap.x == mapXLength - 1)
        {
            worldX = chunkPosition.x + (MarchingCubesData.ChunkWidth / world.voxelsPerMeter);
            border++;
            xValue = 0;
        }

        if (tMap.y == 0)
        {
            worldY = chunkPosition.y - (MarchingCubesData.ChunkHeight / world.voxelsPerMeter);
            border++;
            yValue = MarchingCubesData.ChunkHeight;
        }
        else if (tMap.y == mapYHeight - 1)
        {
            worldY = chunkPosition.y + (MarchingCubesData.ChunkHeight / world.voxelsPerMeter);
            border++;
            yValue = 0;
        }

        if (tMap.z == 0)
        {
            worldZ = chunkPosition.z - (MarchingCubesData.ChunkWidth / world.voxelsPerMeter);
            border++;
            zValue = MarchingCubesData.ChunkWidth;
        }
        else if (tMap.z == mapZLength - 1)
        {
            worldZ = chunkPosition.z + (MarchingCubesData.ChunkWidth / world.voxelsPerMeter);
            border++;
            zValue = 0;
        }

        if (border != 0)
        {
            if (border == 1)    //faces. 2 chunks should be altered (1 + the original)
            {
                if (xValue != -1)
                {
                    AlterBorderChunk(new Vector3(worldX, chunkPosition.y, chunkPosition.z), xValue, tMap.y, tMap.z, newTerrainValue, updateMesh);
                }
                else if (yValue != -1)
                {
                    AlterBorderChunk(new Vector3(chunkPosition.x, worldY, chunkPosition.z), tMap.x, yValue, tMap.z, newTerrainValue, updateMesh);
                }
                else if (zValue != -1)
                {
                    AlterBorderChunk(new Vector3(chunkPosition.x, chunkPosition.y, worldZ), tMap.x, tMap.y, zValue, newTerrainValue, updateMesh);
                }
                
            }
            else if (border == 2)   //edges. 4 chunks should be altered (3 + the original)
            {
                if (xValue == -1)
                {
                    AlterBorderChunk(new Vector3(worldX, worldY, worldZ), tMap.x, yValue ,zValue, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(worldX, chunkPosition.y, worldZ), tMap.x, tMap.y, zValue, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(worldX, worldY, chunkPosition.z), tMap.x, yValue, tMap.z, newTerrainValue, updateMesh);
                }
                else if (yValue == -1)
                {
                    AlterBorderChunk(new Vector3(worldX, worldY, worldZ), xValue, tMap.y, zValue, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(chunkPosition.x, worldY, worldZ), tMap.x, tMap.y, zValue, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(worldX, worldY, chunkPosition.z), xValue, tMap.y, tMap.z, newTerrainValue, updateMesh);
                }
                else if (zValue == -1)
                {
                    AlterBorderChunk(new Vector3(worldX, worldY, worldZ), xValue, yValue, tMap.z, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(chunkPosition.x, worldY, worldZ), tMap.x, yValue, tMap.z, newTerrainValue, updateMesh);
                    AlterBorderChunk(new Vector3(worldX, chunkPosition.y, worldZ), xValue, tMap.y, tMap.z, newTerrainValue, updateMesh);
                }
            }
            else if (border == 3)   //corners. 8 chunks should be altered (7 + the original)
            {
                AlterBorderChunk(new Vector3(worldX, worldY, worldZ),xValue, yValue, zValue, newTerrainValue, updateMesh);

                AlterBorderChunk(new Vector3(chunkPosition.x, worldY, worldZ), tMap.x, yValue, zValue, newTerrainValue, updateMesh);
                AlterBorderChunk(new Vector3(worldX, chunkPosition.y, worldZ), xValue, tMap.y, zValue, newTerrainValue, updateMesh);
                AlterBorderChunk(new Vector3(worldX, worldY, chunkPosition.z), xValue, yValue, tMap.z, newTerrainValue, updateMesh);

                AlterBorderChunk(new Vector3(chunkPosition.x, chunkPosition.y, worldZ), tMap.x, tMap.y, zValue, newTerrainValue, updateMesh);
                AlterBorderChunk(new Vector3(chunkPosition.x, worldY, chunkPosition.z), tMap.x, yValue, tMap.z, newTerrainValue, updateMesh);
                AlterBorderChunk(new Vector3(worldX, chunkPosition.y, chunkPosition.z), xValue, tMap.y, tMap.z, newTerrainValue, updateMesh);

            }
        }

        CreateMeshData();
    }*/

    //TODO: test if it even works
    public float GetTerrainMapValueFromWorldPos(Vector3 pos)
    {
        Vector3 localPos = pos - ChunkPosition;
        Vector3Int tMap = new Vector3Int((int)Mathf.Round(localPos.x * World.VoxelsPerMeter), (int)Mathf.Round(localPos.y * World.VoxelsPerMeter), (int)Mathf.Round(localPos.z * World.VoxelsPerMeter));

        return TerrainMap[tMap.x, tMap.y, tMap.z];
    }

    //TODO: Delete this but save it somewhere for reference
    //Old marching cubes function. New one uses the job system.
    void MarchCube(Vector3Int position, float[] cube)
    {
        int configIndex = GetCubeConfig(cube);

        //These 2 configs means that the cube has no triangles
        if (configIndex == 0 || configIndex == 255)
            return;

        //Loop through the triangles. Never more than 5 triangles to a cube and only 3 vertices per triangle
        int edgeIndex = 0;

        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                //Get current indice. Increment triangle index each loop
                int indice = MarchingCubesData.TriangleTable[configIndex, edgeIndex];

                //If the indice is -1, that means no more indices for that config and we can exit
                if (indice == -1)
                    return;

                //Get the vertices for the start and end of the edge
                Vector3Int vert1 = position + MarchingCubesData.CornerTable[MarchingCubesData.EdgeIndexes[indice, 0]];
                Vector3Int vert2 = position + MarchingCubesData.CornerTable[MarchingCubesData.EdgeIndexes[indice, 1]];

                Vector3 vertPosition;
                
                if (smoothTerrain)
                {
                    //Get terrain values at each end of edge, then interpolate the vert point on the edge
                    float vert1Sample = cube[MarchingCubesData.EdgeIndexes[indice, 0]];
                    float vert2Sample = cube[MarchingCubesData.EdgeIndexes[indice, 1]];

                    float difference = vert2Sample - vert1Sample;

                    if (difference == 0)
                        difference = World.SurfaceLevel;
                    else
                        difference = (World.SurfaceLevel - vert1Sample) / difference;

                    vertPosition = vert1 + ((Vector3)(vert2 - vert1) * difference);

                }
                else
                {
                    //midpoint of edge
                    vertPosition = (Vector3)(vert1 + vert2) / 2f;
                }

                
                Vertex vert = new Vertex();
                vert.position = vertPosition;
                var indexA = indexFromCoord(vert1);
                var indexB = indexFromCoord(vert2);
                vert.edgeID = new int2(Mathf.Min(indexA, indexB), Mathf.Max(indexA, indexB));
                //vertices.Add(vert);

                /*if (flatShading)
                {
                    processedVertices.Add(vertPosition/world.voxelsPerMeter);
                    processedTriangles.Add(processedVertices.Count - 1);
                }
                else
                {
                    processedTriangles.Add(VertForIndice(vertPosition/world.voxelsPerMeter));
                }*/
                edgeIndex++;
            }
        }
    }

    //This is slow if I want to update the terrain mesh every frame. TODO: Use jobs(?)
    void BuildMeshData()
    {
        int triIndex = 0;

        for (int i = 0; i < vertexCopy.Length; i++)
        {
            int sharedIndex;

            if (!flatShading && vertexMap.TryGetValue(vertexCopy[i].edgeID, out sharedIndex))
            {
                processedTriangles.Add(sharedIndex);
            }
            else
            {
                if (!flatShading)
                {
                    vertexMap.Add(vertexCopy[i].edgeID, triIndex);
                }
                processedVertices.Add(vertexCopy[i].position / World.VoxelsPerMeter);
                processedTriangles.Add(triIndex);
                triIndex++;
            }
        }
        
    }

    int indexFromCoord(Vector3Int coord)
    {
        coord = coord - (ChunkGridPosition * 17);
        return coord.z * 17 * 17 + coord.y * 17 + coord.x;
    }

    //TODO: Delete this but save it for reference, just incase
    int VertForIndice(Vector3 vert)
    {
        for (int i = 0; i < processedVertices.Count; i++)
        {
            //if we find a vert that matches ours, just return the index
            if (processedVertices[i] == vert)
                return i;
        }

        processedVertices.Add(vert);
        return processedVertices.Count - 1;
    }

    //Gets the config of the cube from the tables based on the points being below the terrain surface
    int GetCubeConfig(float[] cube)
    {
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] < World.SurfaceLevel)
                index |= 1 << i;
        }

        return index;
    }

}