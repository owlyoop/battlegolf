using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SurfaceNets : MonoBehaviour
{
    private struct sn_point
    {
        float sdf_value;
    }

    /*Voxel corner indices
    
       7          6
	   o----------o
      /|         /|
	4/ |       5/ |
	o--|-------o  |
	|  o-------|--o
    | /3       | /2
	|/         |/
	o----------o
    0          1

	*/
	

    const int NUM_VOXELS_WIDTH = 16;
    const int NUM_VOXELS_LENGTH = 16;
    const int NUM_VOXELS_HEIGHT = 16;

    public float surfaceLevel = 0f;
    public float voxelsPerMeter = 1f;
    public int smoothing = 3;

    float[,,] terrainMap = new float[NUM_VOXELS_WIDTH+1, NUM_VOXELS_LENGTH+1, NUM_VOXELS_HEIGHT+1];

    public Material mat;
    public WorldGenGraph worldGraph;
    Mesh mesh;
    MeshFilter meshF;
    MeshRenderer meshR;

    Vector3[] vertices = new Vector3[NUM_VOXELS_WIDTH * NUM_VOXELS_LENGTH * NUM_VOXELS_HEIGHT];
    int[] triangles = new int[6666];

    Dictionary<Vector3Int, int> activeCubeToVertexIndex = new Dictionary<Vector3Int, int>();

    sn_point[,,] world = new sn_point[NUM_VOXELS_WIDTH, NUM_VOXELS_LENGTH, NUM_VOXELS_HEIGHT];

    private void Start()
    {
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        TerrainMap();
        Generate();
    }

    void TerrainMap()
    {
        for (int x = 0; x < NUM_VOXELS_WIDTH+1; x++)
        {
            for (int z = 0; z < NUM_VOXELS_LENGTH+1; z++)
            {
                for (int y = 0; y < NUM_VOXELS_HEIGHT+1; y++)
                {
                    terrainMap[x, y, z] = worldGraph.endNode.GenerateTerrainMap(
                        new Vector3(x * voxelsPerMeter ,y * voxelsPerMeter ,z * voxelsPerMeter));
                }
            }
        }
    }

    void Generate()
    {
        int verticesIter = 0;
        int trianglesIter = 0;

        mesh = new Mesh();

        for (int x = 0; x < NUM_VOXELS_WIDTH; x++)
        {
            for (int z = 0; z < NUM_VOXELS_LENGTH; z++)
            {
                for (int y = 0; y < NUM_VOXELS_HEIGHT; y++)
                {
                    //visit every cube and determine which ones are intersected by the implicit surface function
                    //if the signs change between the vertices on an edge, it's an intersection

                    /*Voxel corner indices
    
                       7          6
	                   o----------o
                  Y   /|         /|
	                4/ |  Z    5/ |
	                o--|-------o  |
	                |  o-------|--o
                    | /3       | /2
	                |/         |/
	                o----------o    X
                    0          1

	                */

                    Vector3Int position = new Vector3Int(x, y, z);

                    //find which edges are bipolar
                    float[] cornerValues = new float[8];
                    for (int c = 0; c < 8; c++)
                    {
                        cornerValues[c] = terrainMap[SurfaceNetsData.CornerTable[c].x + x,
                                                    SurfaceNetsData.CornerTable[c].y + y,
                                                    SurfaceNetsData.CornerTable[c].z + z];
                    }

                    bool[] bipolarityArray = new bool[12];

                    for (int b = 0; b < SurfaceNetsData.EdgeIndexes.GetLength(0); b++)
                    {
                        bipolarityArray[b] = IsEdgeBipolar(cornerValues[SurfaceNetsData.EdgeIndexes[b, 0]], 
                                                           cornerValues[SurfaceNetsData.EdgeIndexes[b, 1]]);
                    }

                    //active voxel must have atleast 1 bipolar edge
                    bool isVoxelActive = false;
                    foreach (var b in bipolarityArray)
                    {
                        if (b) isVoxelActive = true;
                    }
                    //cubes that aren't active don't create vertices
                    if (!isVoxelActive)
                        continue;

                    //store all edge intersection points
                    List<Vector3> edgeIntersectionPoints = new List<Vector3>();

                    //visit every bipolar edge
                    for (int e = 0; e < bipolarityArray.Length; e++)
                    {
                        if (bipolarityArray[e])
                        {
                            //get points p1, p2 of the edge
                            Vector3Int p1 = position + SurfaceNetsData.CornerTable[SurfaceNetsData.EdgeIndexes[e, 0]];
                            Vector3Int p2 = position + SurfaceNetsData.CornerTable[SurfaceNetsData.EdgeIndexes[e, 1]];

                            Vector3 vertPosition;

                            //get the densities at each point
                            float vert1Sample = cornerValues[SurfaceNetsData.EdgeIndexes[e, 0]];
                            float vert2Sample = cornerValues[SurfaceNetsData.EdgeIndexes[e, 1]];

                            //linear interpolation
                            var t = (surfaceLevel - vert1Sample) / (vert2Sample - vert1Sample);
                            edgeIntersectionPoints.Add(p1 + t * ((Vector3)p2 - (Vector3)p1));
                        }
                    }

                    /*approximate the generated mesh's vertex using the geometric center of all bipolar edges' intersection
                    point with with the implicit surface*/
                    float numIntersectionPoints = edgeIntersectionPoints.Count;
                    Vector3 sumOfIntersectionPoints = Vector3.zero;
                    foreach (var i in edgeIntersectionPoints)
                    {
                        sumOfIntersectionPoints += i;
                    }
                    Vector3 geometricCenterOfEdgeIntersections = (sumOfIntersectionPoints/ numIntersectionPoints);

                    //map geometric center from local grid coords to world space
                    //TODO: actually do the above
                    Vector3 meshVertex = new Vector3(
                        geometricCenterOfEdgeIntersections.x,
                        geometricCenterOfEdgeIntersections.y,
                        geometricCenterOfEdgeIntersections.z);

                    vertices[verticesIter] = meshVertex;
                    //store mapping from this active cube to the mesh's vertex index for triangulation later
                    //int activeCubeIndex = x + (y * NUM_VOXELS_WIDTH) + (z * NUM_VOXELS_WIDTH * NUM_VOXELS_HEIGHT);
                    int vertexIndex = verticesIter;
                    //activeCubeToVertexIndex[activeCubeIndex] = vertexIndex;
                    activeCubeToVertexIndex[new Vector3Int(x,y,z)] = vertexIndex;

                    verticesIter++;
                }
            }
        }

        //Triangulation
        //visit every active cube and look at neighbours which theres a possible triangulation to be made
        //a quad is generated when four active cubes share a common edge
        //at each iteration, we will look at neighbouring voxels of the current active cube, and will create a quad if theyre all active cubes
        foreach (var cube in activeCubeToVertexIndex)
        {
            //int activeCubeIndex = cube.Key;
            var activeCube = cube.Key;
            int vertexIndex = cube.Value;

            //Vector3Int gridPos = GetGridPosFromIndex(activeCubeIndex);

            /*lower boundary cubes have missing neighbour voxels, so no need to triangulate
             
            * Active cube
		
		        7          6
		        o----------o
		       /|         /|
		     4/ |       5/ |
		     o--|-------o  |
		     |  o-------|--o
		     | /3       | /2
		     |/         |/
		     o----------o
		     0          1
		
		     Potentially crossed edges
		
		     4
		     o
		     |  o
		     | /3
		     |/
		     o----------o
		     0          1
		
            only consider these crossed edges so every interior edge is only visited ones


            */
            if (IsGridPosLowerBounds(activeCube))
                continue;

            /* indices of neighbor voxels
             * 
             *  topdown view. top level
             *  0A          A = active cube
             *  12
             *  
             *  bottom level
             *  54
             *   3
             */

            Vector3Int[] cornersOfInterest = new Vector3Int[4] {
            new Vector3Int(activeCube.x, activeCube.y, activeCube.z), //0
            new Vector3Int(activeCube.x, activeCube.y + 1, activeCube.z), //4
            new Vector3Int(activeCube.x + 1, activeCube.y, activeCube.z),//1
            new Vector3Int(activeCube.x, activeCube.y, activeCube.z + 1)}; //3

            float[,] edgeTerrainValues = new float[3, 2] {
                {terrainMap[activeCube.x, activeCube.y, activeCube.z], terrainMap[activeCube.x, activeCube.y, activeCube.z+1]  }, //0,4
                {terrainMap[activeCube.x, activeCube.y+1, activeCube.z], terrainMap[activeCube.x, activeCube.y, activeCube.z]  }, //3,0
                {terrainMap[activeCube.x, activeCube.y, activeCube.z], terrainMap[activeCube.x+1, activeCube.y, activeCube.z]  }, //0,1
            };

            /* the 3 potential generated quads all share the currect active cube's mesh vertex.
             * store the current active cube's neighbours for each potentially generated quad
             * 
             * the order of the neighbours is such that the quads are generated with an outward normal direction that's in
             * the same direction as the directed/oriented edges:
             * (0,4) (3,0) and (0,1)
             * 
             * looking at the gradient of the density function along the edge, 
             * we can flip the quad's outward normal direction to match the gradient's
             */
            int[,] quadNeighbours = new int[3, 3] {
                {0,1,2},
                {0,5,4},
                {2,3,4}
            };

            /*
		    * For directed edge e that has the same direction as the
		    * gradient along e, the correct order of neighbor vertices
		    * is 0,1,2. If the direction of e is opposite the gradient,
		    * the correct order is 2,1,0.
		    *
		    * For current active cube's mesh vertex v, we will then have
		    * quads: v,0,1,2 or v,2,1,0
		    */

            int[,] quadNeighbourOrders = new int[2, 3] {
                { 0, 1, 2 },
                { 2, 1, 0 }
            };

            //look at each potential quad
            for (int i = 0; i < 3; i++)
            {
                var neighbour1 = new Vector3Int(
                    activeCube.x + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 0], 0],
                    activeCube.y + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 0], 1],
                    activeCube.z + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 0], 2]);

                var neighbour2 = new Vector3Int(
                    activeCube.x + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 1], 0],
                    activeCube.y + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 1], 1],
                    activeCube.z + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 1], 2]);

                var neighbour3 = new Vector3Int(
                    activeCube.x + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 2], 0],
                    activeCube.y + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 2], 1],
                    activeCube.z + SurfaceNetsData.NeighborGridPositions[quadNeighbours[i, 2], 2]);

                //only generate a quad if all neighbours are active cubes
                if (!activeCubeToVertexIndex.ContainsKey(neighbour1)
                    || !activeCubeToVertexIndex.ContainsKey(neighbour2)
                    || !activeCubeToVertexIndex.ContainsKey(neighbour3))
                    continue;

                int[] neighbourVertices = new int[3] {
                    activeCubeToVertexIndex[neighbour1],
                    activeCubeToVertexIndex[neighbour2],
                    activeCubeToVertexIndex[neighbour3] };

                int[] neighbourVerticesOrder = new int[3];
                if (edgeTerrainValues[i, 1] < edgeTerrainValues[i, 0])
                {
                    neighbourVerticesOrder[0] = 0;
                    neighbourVerticesOrder[1] = 1;
                    neighbourVerticesOrder[2] = 2;
                }
                else
                {
                    neighbourVerticesOrder[0] = 2;
                    neighbourVerticesOrder[1] = 1;
                    neighbourVerticesOrder[2] = 0;
                }

                //generate the quad
                var v0 = vertexIndex;
                var v1 = neighbourVertices[neighbourVerticesOrder[0]];
                var v2 = neighbourVertices[neighbourVerticesOrder[1]];
                var v3 = neighbourVertices[neighbourVerticesOrder[2]];

                triangles[trianglesIter] = v0;
                triangles[trianglesIter+1] = v1;
                triangles[trianglesIter+2] = v2;

                triangles[trianglesIter+3] = v0;
                triangles[trianglesIter+4] = v2;
                triangles[trianglesIter+5] = v3;

                trianglesIter += 6;

                /*triangles[trianglesIter] = v0;
                triangles[trianglesIter + 1] = v1;
                triangles[trianglesIter + 2] = v2;
                triangles[trianglesIter + 3] = v3;

                trianglesIter += 4;*/
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.SetIndices(triangles, MeshTopology.Quads, 0);
        mesh.RecalculateNormals();
        meshF.mesh = mesh;
        meshR.material = mat;
    }

    Vector3Int GetGridPosFromIndex(int index)
    {
        return new Vector3Int(index % NUM_VOXELS_WIDTH,
            (index / NUM_VOXELS_WIDTH) % NUM_VOXELS_HEIGHT,
            index / (NUM_VOXELS_WIDTH * NUM_VOXELS_HEIGHT));
    }

    bool IsGridPosLowerBounds(Vector3Int gridPos)
    {
        return (gridPos.x == 0 || gridPos.y == 0 || gridPos.z == 0);
    }

    bool IsScalarPositive(float v)
    {
        return v >= surfaceLevel;
    }

    bool IsEdgeBipolar(float v1, float v2)
    {
        return IsScalarPositive(v1) != IsScalarPositive(v2);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        /*for (int x = 0; x < NUM_VOXELS_WIDTH; x++)
        {
            for (int z = 0; z < NUM_VOXELS_LENGTH; z++)
            {
                for (int y = 0; y < NUM_VOXELS_HEIGHT; y++)
                {
                    Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(0.1f,0.1f,0.1f));
                }
            }
        }*/

        /*for (int x = 0; x < terrainMap.GetLength(0); x++)
        {
            for (int y = 0; y < terrainMap.GetLength(1); y++)
            {
                for (int z = 0; z < terrainMap.GetLength(2); z++)
                {
                    
                }
            }
        }*/
    }
}
