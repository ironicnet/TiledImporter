using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using System;

namespace Ironicnet.TiledImporter
{
    public class TiledLevelGenerator : MonoBehaviour
    {

        public string Filename;
        public Tiled tiled;
        public TilesetData[] tilesetData;
        public Tile DefaultPrefab;


        public int UnitsPerPixel = 100;
        public float ZOrderDepth = 0.16f;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        [ContextMenu("Generate")]
        void Generate()
        {
            tiled = new TiledParser().Parse(Filename);
            tilesetData = new TilesetData[tiled.Map.Tilesets.Length];
            for (int i = 0; i < tiled.Map.Tilesets.Length; i++)
            {
                var tileset = tiled.Map.Tilesets[i];
                var path = tileset.Image.Source.Replace("..", "Assets");
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                if (assets.Length > 0)
                {
                    Sprite[] allSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Cast<Sprite>().ToArray();

                    tilesetData[i] = new TilesetData()
                    {
                        Tileset = tileset,
                        Sprites = allSprites
                    };
                }
                else
                {
                    tilesetData[i] = new TilesetData()
                    {
                        Tileset = tileset,
                        Sprites = new Sprite[0]
                    };
                }
            }

            for (int i = 0; i < tiled.Map.Layers.Length; i++)
            {
                var layer = tiled.Map.Layers[i];
                Debug.Log(layer.data);
                int x = 0;
                int y = 0;
                float zOrder = (tiled.Map.Layers.Length - 1 - i) * ZOrderDepth;
                if (layer.Properties.Any(p => p.Name == "ZOrder"))
                {
                    //zOrder = float.Parse(layer.Properties.First(p => p.Name == "ZOrder").Value);
                }
                GameObject layerObject = new GameObject(layer.Name);
                layerObject.transform.position = Vector3.zero;


                for (int j = 0; j < layer.data.tiles.Length; j++)
                {
                    long gid = layer.data.tiles[j];
                    if (gid > 0)
                    {
                        TilesetData tileset = GetTileset(gid);
                        var widthFactor = ((float)tileset.Tileset.TileWidth / (float)UnitsPerPixel);
                        var heightFactor = ((float)tileset.Tileset.TileHeight / (float)UnitsPerPixel);
                        long spriteIndex = gid - tileset.Tileset.FirstGID;

                        Sprite sprite = tileset.Sprites[spriteIndex];
                        Vector3 position = new Vector3(x * widthFactor, y * heightFactor, zOrder);
                        Tile tile = GameObject.Instantiate(DefaultPrefab, position, new Quaternion()) as Tile;
                        tile.gameObject.name = string.Concat("Tile[", x, ",", y, "] - gid:(", gid, ")");
                        tile.SetSprite(sprite);
                        tile.config = tileset.GetConfig(gid);
                        tile.transform.parent = layerObject.transform;
                    }
                    else
                    {
                    }
                    if (x >= tiled.Map.width - 1)
                    {
                        x = 0;
                        y--;
                    }
                    else
                    {
                        x++;
                    }
                }

            }
        }

        private TilesetData GetTileset(long gid)
        {
            int lastMatching = 0;
            for (int i = 0; i < tilesetData.Length; i++)
            {
                Debug.Log(tilesetData[i]);
                if (tilesetData[i].Tileset.FirstGID <= gid)
                {
                    lastMatching = i;
                }
            }
            return tilesetData[lastMatching];
        }

        [Serializable]
        public class TilesetData
        {
            public TiledTileset Tileset;
            public Sprite[] Sprites;

            internal TiledTilesetTileConfig GetConfig(long gid)
            {
                return Tileset.GetConfig(gid);
            }
        }
    }

}