using Mirror;
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

    int maxLength;

    NativeArray<Vertex> vertexCopy;
    NativeHashMap<int2, int> vertexMapCopy;
    NativeList<Vector3> vertices;
    NativeList<int> triangles;
    [ReadOnly]
    NativeArray<float> terMap;
    MarchingCubesJob marchingCubesJob;

    List<Vector3> tempVerts = new List<Vector3>();
    List<int> tempTris = new List<int>();

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

        maxLength = MarchingCubesData.ChunkHeight * MarchingCubesData.ChunkWidth * MarchingCubesData.ChunkWidth * 15;

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
        
        vertices = new NativeList<Vector3>(maxLength, Allocator.TempJob);
        triangles = new NativeList<int>(maxLength * 3, Allocator.TempJob);
        vertexCopy = new NativeArray<Vertex>(maxLength, Allocator.TempJob);
        vertexMapCopy = new NativeHashMap<int2, int>(maxLength, Allocator.TempJob);
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
            voxelsPerMeter = World.VoxelsPerMeter,
            surfaceLevel = World.SurfaceLevel,
            smoothTerrain = World.SmoothTerrain,
            flatShading = World.FlatShading,
            VertexCountCounter = vertexCountCounter,
            processedVertices = vertices,
            processedTriangles = triangles,
            jobVertices = vertexCopy,
            vertexMap = vertexMapCopy,
            terMap = terMap
        };

        JobHandle jobHandle = marchingCubesJob.Schedule();

        jobHandle.Complete();

        int vertexCount = marchingCubesJob.VertexCountCounter.Count;

        BuildMesh();

        vertices.Dispose();
        triangles.Dispose();
        terMap.Dispose();
        vertexCopy.Dispose();
        vertexMapCopy.Dispose();
        vertexCountCounter.Dispose();

    }
    void ClearMeshData()
    {
        if (mesh != null) mesh.Clear();
    }

    void BuildMesh()
    {
        for (int v = 0; v < vertices.Length; v++)
            tempVerts.Add(vertices[v]);

        for (int t = 0; t < triangles.Length; t++)
            tempTris.Add(triangles[t]);


        mesh.MarkDynamic();

        mesh.SetVertices(tempVerts);
        mesh.SetTriangles(tempTris,0,false);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        if (tempVerts.Count >= 3)
            meshCollider.sharedMesh = mesh;
           
        backsideMesh.mesh = mesh;

        tempVerts.Clear();
        tempTris.Clear();
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

                    /*var point = new Vector3(x + ChunkPosition.x * World.VoxelsPerMeter
                        , y + ChunkPosition.y * World.VoxelsPerMeter
                        , z + ChunkPosition.z * World.VoxelsPerMeter);

                    thisPoint = Noise.Evaluate(point * 0.025f);*/


                    TerrainMap[x, y, z] = thisPoint;
                }
            }
        }
    }

    //TODO: look into the job system for generating the terrain map with xnode. Check if that's even a performance impact
    // uhhhh. major slowdowns are xnode ports and TryGetValue in BuildMeshData
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

   
    //TODO: test if it even works
    public float GetTerrainMapValueFromWorldPos(Vector3 pos)
    {
        Vector3 localPos = pos - ChunkPosition;
        Vector3Int tMap = new Vector3Int((int)Mathf.Round(localPos.x * World.VoxelsPerMeter), (int)Mathf.Round(localPos.y * World.VoxelsPerMeter), (int)Mathf.Round(localPos.z * World.VoxelsPerMeter));

        return TerrainMap[tMap.x, tMap.y, tMap.z];
    }

    //This is slow if I want to update the terrain mesh every frame. TODO: Use jobs(?)
    /*void BuildMeshData()
    {
        int triIndex = 0;

        for (int i = 0; i < vertexCopy.Length; i++)
        {
            int sharedIndex;

            //This is slow
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
        
    }*/

    /*int indexFromCoord(Vector3Int coord)
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
    }*/

}