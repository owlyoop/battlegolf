using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class BlurNode : FilterNode
{

    public int blurRings = 1;
	protected override void Init()
    {
		base.Init();
	}

    //TODO: i am fetching the terrain map values way too many times. too many duplicates. i can make this faster
    public float Blur(Vector3 pos)
    {
        if (worldGen == null)
            worldGen = FindObjectOfType<WorldGenerator>();

        float sum = 0;

        //dont blur points on the edge of the world
        if (pos.x < 0 || pos.y < 0 || pos.z < 0 || pos.x >= (float)(worldGraph.endNode.width * MarchingCubesData.ChunkWidth) / 1
            || pos.y >= (float)(worldGraph.endNode.height * MarchingCubesData.ChunkHeight) / 1
            || pos.z >= (float)(worldGraph.endNode.length * MarchingCubesData.ChunkWidth) / 1)
        {
            return map[(int)pos.x,(int)pos.y,(int)pos.z];
        }
        else
        {
            for (int x = -blurRings; x < blurRings + 1; x++)
            {
                for (int y = -blurRings; y < blurRings + 1; y++)
                {
                    for (int z = -blurRings; z < blurRings + 1; z++)
                    {
                        if ((int)pos.x + x < map.GetLength(0) && (int)pos.x + x >= 0 && 
                            (int)pos.y + y < map.GetLength(1) && (int)pos.y + y >= 0 && 
                            (int)pos.z + z < map.GetLength(2) && (int)pos.z + z >= 0)
                        {
                            sum += map[(int)pos.x + x, (int)pos.y + y, (int)pos.z + z];

                        }
                        else
                        {
                            sum += 0;
                            //sum += map[(int)pos.x, (int)pos.y, (int)pos.z];
                            //return map[(int)pos.x, (int)pos.y, (int)pos.z];
                        }

                    }
                }
            }
        }

        int size = ((blurRings * 2) + 1);
        return sum / (size*size*size);
    }



	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        float value = Blur(worldGraph.noiseGenPoint);
        return value;
	}

    public override void UpdatePreview()
    {
        base.UpdatePreview();
    }
}