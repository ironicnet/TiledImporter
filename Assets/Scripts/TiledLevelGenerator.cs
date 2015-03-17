using UnityEngine;
using System.Collections;

public class TiledLevelGenerator : MonoBehaviour {

	public TiledFileInfo FileInfo;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Generate()
    {

        for (int i = 0; i < FileInfo.TiledFile.Map.Layers.Length; i++)
        {
            var layer = FileInfo.TiledFile.Map.Layers[i];
            for (int j = 0; j < layer.data.tiles.Length; j++)
            {

            }

        }
    }
}
