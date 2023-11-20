using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


/* TODO: I will have to rewrite the marching cubes algorithm to use an array of a fixed size ((width * height) * 15) to use BurstCompile
*/

//[BurstCompile]
public struct MarchingCubesJob : IJob
{

    public float surfaceLevel;
    public bool smoothTerrain;
    public bool flatShading;
    Vector3Int chunkGridPos;
    Vector3Int position;

    VoxelCorners<float> densities;

    public int numVoxelsWidth;
    public int numVoxelsHeight;
    public float voxelsPerMeter;
    public NativeCounter VertexCountCounter { get; set; }

    public NativeArray<Vertex> jobVertices;

    public NativeArray<float> terMap; //x + WIDTH * (y + DEPTH * z) 

    int iter;

    public NativeHashMap<int2, int> vertexMap;

    public NativeList<Vector3> processedVertices;
    public NativeList<int> processedTriangles;

    int indexFromCoord(Vector3Int coord)
    {
        coord = coord - (chunkGridPos * 17);
        return coord.z * 17 * 17 + coord.y * 17 + coord.x;
    }

    int GetCubeConfig(VoxelCorners<float> cube)
    {
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] < surfaceLevel)
                index |= 1 << i;
        }

        return index;
    }

    void MarchCube(Vector3Int position, VoxelCorners<float> cube)
    {
        int configIndex = GetCubeConfig(densities);

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
                    float vert1Sample = densities[MarchingCubesData.EdgeIndexes[indice, 0]];
                    float vert2Sample = densities[MarchingCubesData.EdgeIndexes[indice, 1]];

                    float difference = vert2Sample - vert1Sample;

                    if (difference == 0)
                        difference = surfaceLevel;
                    else
                        difference = (surfaceLevel - vert1Sample) / difference;

                    vertPosition = vert1 + ((Vector3)(vert2 - vert1) * difference);

                }
                else
                {
                    //midpoint of edge
                    vertPosition = (Vector3)(vert1 + vert2) / 2f;
                }

                int index = VertexCountCounter.Increment();

                Vertex vert = new Vertex();
                vert.position = vertPosition;
                var indexA = indexFromCoord(vert1);
                var indexB = indexFromCoord(vert2);
                vert.edgeID = new Unity.Mathematics.int2(Mathf.Min(indexA, indexB), Mathf.Max(indexA, indexB));
                jobVertices[index] = vert;
                iter++;
                edgeIndex++;
            }
        }
    }

    public void Execute()
    {
        iter = 0;
        for (int x = 0; x < numVoxelsWidth; x++)
        {
            for (int y = 0; y < numVoxelsHeight; y++)
            {
                for (int z = 0; z < numVoxelsWidth; z++)
                {
                    position = new Vector3Int(x,y,z);

                    for (int i = 0; i < 8; i++)
                    {
                        densities[i] = terMap[((x + MarchingCubesData.CornerTable[i].x) * 17 * 17) + 
                            ((y + MarchingCubesData.CornerTable[i].y) * 17) + 
                            z + MarchingCubesData.CornerTable[i].z];
                    }
                    MarchCube(position, densities);
                }
            }
        }

        int triIndex = 0;

        for (int i = 0; i < iter; i++)
        {
            int sharedIndex;

            //This is slow
            if (!flatShading && vertexMap.TryGetValue(jobVertices[i].edgeID, out sharedIndex))
            {
                processedTriangles.Add(sharedIndex);
            }
            else
            {
                if (!flatShading)
                {
                    vertexMap.Add(jobVertices[i].edgeID, triIndex);
                }
                processedVertices.Add(jobVertices[i].position / voxelsPerMeter);
                processedTriangles.Add(triIndex);
                triIndex++;
            }
        }
    }
}
