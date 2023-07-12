using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SurfaceNetsData
{
    public static int[,] EdgeIndexes = new int[12, 2]
    {
        {0, 1}, {1, 2}, {2, 3}, {3, 0}, {4, 5}, {5, 6}, {6, 7}, {7, 4}, {0, 4}, {1, 5}, {2, 6}, {3, 7}
    };

    public static Vector3Int[] CornerTable = new Vector3Int[8]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1)

    };

    public static int[,] NeighborGridPositions = new int[6, 3]
    {
            { -1, 0, 0},
            { -1,-1, 0},
            { 0 ,-1, 0},
            { 0 ,-1,-1},
            { 0 , 0,-1},
            { -1, 0,-1}
    };
}
