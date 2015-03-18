using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

    public SpriteRenderer Renderer;
    public TiledFileInfo.TiledTilesetTileConfig config;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal void SetSprite(Sprite sprite)
    {
        Renderer.sprite = sprite;
    }
}
