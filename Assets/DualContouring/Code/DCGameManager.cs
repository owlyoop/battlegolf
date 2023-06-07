using UnityEngine;
using System.Collections;
using Code.Utils;
using Code;
using System.Collections.Generic;

public class DCGameManager : MonoBehaviour
{
    const int MAX_THRESHOLDS = 5;
	static float[] THRESHOLDS = new float[MAX_THRESHOLDS] { -1.0f, 0.1f, 1.0f, 10.0f, 50.0f };
	static int thresholdIndex = -1;

	OctreeNode root = null;

    public Material terrain;
    public WorldGenGraph worldGraph;

    // octreeSize must be a power of two!
    const int octreeSize = 8;

    void Awake()
    {
    }

	public void Start()
    {
        Code.Utils.DCPlayerInput.Start();

		thresholdIndex = (thresholdIndex + 1) % MAX_THRESHOLDS;
        List<MeshVertex> vertices = new List<MeshVertex>();
		List<int> indices = new List<int>();


        root = Octree.BuildOctree(new Vector3(-octreeSize / 2, -octreeSize / 2, -octreeSize / 2), octreeSize, THRESHOLDS[thresholdIndex]);

        if (root == null)
            Debug.Log("root is null");

        Octree.GenerateMeshFromOctree(root, vertices, indices);
	}
	
	// Update is called once per frame
	void Update ()
    {
        DCPlayerInput.Update();
	}

    void OnApplicationQuit()
    {
        Octree.DestroyOctree(root);
    }

    void OnGUI ()
    {
        //You need a killer computer to be able to use this!! (or drop the octree size some.. )
        //Octree.DrawOctree(root, 3);
    }


}
