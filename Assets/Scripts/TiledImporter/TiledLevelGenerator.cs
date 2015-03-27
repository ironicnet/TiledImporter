using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Ironicnet.TiledImporter
{
    public class TiledLevelGenerator : MonoBehaviour
    {

        public string Filename;
        public Tiled tiled;
        public TilesetData[] tilesetData;
        public Tile DefaultTilePrefab;

        public BoxCollider DefaultObjectPrefab;


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

            for (int layerIndex = 0; layerIndex < tiled.Map.Layers.Length; layerIndex++)
            {
                var layer = tiled.Map.Layers[layerIndex];
                int x = 0;
                int y = 0;
                float zOrder = (tiled.Map.Layers.Length - 1 - layerIndex) * ZOrderDepth;
                if (layer.Properties.Any(p => p.Name == "ZOrder"))
                {
                    //zOrder = float.Parse(layer.Properties.First(p => p.Name == "ZOrder").Value);
                }
                GameObject layerObject = new GameObject(layer.Name);
                layerObject.transform.position = Vector3.zero;

                Tile lastXTile = null;
                Tile[] lastRowsTiles = null;
                Tile[] currentRowsTiles = new Tile[tiled.Map.width];
                Tile[,] Tiles = new Tile[tiled.Map.height, tiled.Map.width];
                for (int j = 0; j < layer.data.tiles.Length; j++)
                {
                    bool ignoreThisTile = false;
                    long gid = layer.data.tiles[j];
                    Tile currentTile = null;
                    if (gid > 0)
                    {
                        TilesetData tileset = GetTileset(gid);
                        var widthFactor = ((float)tileset.Tileset.TileWidth / (float)UnitsPerPixel);
                        var heightFactor = ((float)tileset.Tileset.TileHeight / (float)UnitsPerPixel);
                        long spriteIndex = gid - tileset.Tileset.FirstGID;

                        Sprite sprite = tileset.Sprites[spriteIndex];
                        Vector3 position = new Vector3(x * widthFactor, -1*y * heightFactor, zOrder);
                        currentTile = GameObject.Instantiate(DefaultTilePrefab, position, new Quaternion()) as Tile;
                        currentTile.gameObject.name = string.Concat("Tile[", x, ",", y, "] - gid:(", gid, ")");
                        currentTile.X = x;
                        currentTile.Y = y;
                        currentTile.WidthPx = tileset.Tileset.TileWidth;
                        currentTile.WidthUnit = widthFactor;
                        currentTile.HeightPx = tileset.Tileset.TileHeight;
                        currentTile.HeightUnit = heightFactor;
                        currentTile.DepthUnit = ZOrderDepth;
                        currentTile.ZOrder = zOrder;
                        currentTile.SetSprite(sprite);
                        currentTile.SetConfig(tileset.GetConfig(gid));
                        currentTile.transform.parent = layerObject.transform;
                        currentRowsTiles[x] = currentTile;
                        Tiles[y, x] = currentTile;

                        Tile parentTile = null;
                        if (lastRowsTiles != null && lastRowsTiles[x] != null)
                        {
                            if ((currentTile.Y - lastRowsTiles[x].Y < lastRowsTiles[x].Rowspan) && (currentTile.X - lastRowsTiles[x].X < lastRowsTiles[x].Rowspan))
                            {
                                Debug.Log("Possible parent to Y" + currentTile.name, lastRowsTiles[x]);
                                parentTile = lastRowsTiles[x];
                            }
                        }
                        if (parentTile == null && lastXTile != null && lastXTile.config != null && lastXTile.Colspan > 1)
                        {
                            if (currentTile.X - lastXTile.X < lastXTile.Colspan)
                            {
                                parentTile = lastXTile;
                            }
                        }
                        if (parentTile != null)
                        {
                            GameObject.DestroyImmediate(currentTile.GetComponent<Collider>());
                            currentTile.transform.parent = parentTile.transform;
                            ignoreThisTile = true;
                            currentRowsTiles[x] = parentTile;
                        }

                    }
                    else
                    {
                        currentRowsTiles[x] = null;
                        Debug.Log(string.Concat("Y: ",y, ".X: ", x, ".Tiles: ",  Tiles));
                        Tiles[y, x] = null;
                    }

                    if (x >= tiled.Map.width - 1)
                    {
                        if (!ignoreThisTile)
                        {
                            lastXTile = null;
                        }
                        lastRowsTiles = new Tile[currentRowsTiles.Length];
                        currentRowsTiles.CopyTo(lastRowsTiles, 0);
                        x = 0;
                        y++;
                    }
                    else
                    {
                        x++;
                        if (!ignoreThisTile)
                        {
                            lastXTile = currentTile;
                        }
                    }
                }
                OptimizeColliders(Tiles);
            }
            for (int i = 0; i < tiled.Map.ObjectGroups.Length; i++)
            {
                var objGroup = tiled.Map.ObjectGroups[i];
                GameObject objGroupTransform = new GameObject();
                objGroupTransform.name = objGroup.Name;

                for (int j = 0; j < objGroup.Objects.Length; j++)
                {

                    var obj = objGroup.Objects[j];
                    BoxCollider unityObject = GameObject.Instantiate(DefaultObjectPrefab, new Vector3(obj.x / UnitsPerPixel, obj.y / UnitsPerPixel * -1, ZOrderDepth), new Quaternion()) as BoxCollider;
                    unityObject.name = obj.Name;
                    unityObject.size = new Vector3(obj.width / UnitsPerPixel, obj.height / UnitsPerPixel, 1);
                    unityObject.center = new Vector3(obj.width / UnitsPerPixel / 2, obj.height / UnitsPerPixel / 2 * -1, ZOrderDepth);
                    Debug.Log(string.Concat("X:", obj.x, ". Y: ", obj.y, ". Name: ", obj.Name));

                    unityObject.transform.parent = objGroupTransform.transform;
                }
            }
        }

        private void OptimizeColliders(Tile[,] tiles)
        {
            Dictionary<Guid, List<Tile>> groups = new Dictionary<Guid, List<Tile>>();
            for (var y = tiles.GetLowerBound(0); y < tiles.GetUpperBound(0); y++)
            {
                for (var x = tiles.GetLowerBound(1); x < tiles.GetUpperBound(1); x++)
                {
                    var current = tiles[y, x];
                    if (current==null || current.IsSpanned) continue;

                    var top = y > 0 ? tiles[y - 1, x] : null;
                    var bottom = y < tiles.GetUpperBound(0) - 1 ? tiles[y + 1, x] : null;
                    var left = x > 0 ? tiles[y, x - 1] : null;
                    bool canGroupLeft = (left != null && !left.IsSpanned);
                    bool leftIsNonGrouped = canGroupLeft && left.GroupId == Guid.Empty;
                    var right = x < tiles.GetUpperBound(1) - 1 ? tiles[y, x + 1] : null;
                    bool canGroupRight = (right != null && !right.IsSpanned);
                    bool rightIsNonGrouped = canGroupRight && right.GroupId == Guid.Empty;

                    if (canGroupLeft)
                    {
                        if (left.GroupId == Guid.Empty)
                        {
                            Guid guid = Guid.NewGuid();
                            left.GroupId = guid;
                            current.GroupId = guid;
                            groups.Add(guid, new List<Tile>() { left, current });
                        }
                        else
                        {
                            groups[left.GroupId].Add(current);
                        }
                    }
                }
            }
        }

        private TilesetData GetTileset(long gid)
        {
            int lastMatching = 0;
            for (int i = 0; i < tilesetData.Length; i++)
            {
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