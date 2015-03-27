using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ironicnet.TiledImporter
{
    [Serializable]
    public class Tiled
    {
        public TiledMap Map;
    }
    [Serializable]
    public class TiledMap
    {
        public string version;
        public string orientation;
        public string renderorder;
        public long width;
        public long height;
        public int tilewidth;
        public int tileheight;
        public long nextobjectid;
        public TiledTileset[] Tilesets;
        public TiledLayer[] Layers;
        public TiledObjectGroup[] ObjectGroups;
    }
    [Serializable]
    public class TiledTileset
    {
        public string Name;
        public int TileWidth;
        public int TileHeight;
        public long FirstGID;
        public TiledTilesetImage Image;
        public TiledTilesetTileConfig[] TilesConfig;

        internal TiledTilesetTileConfig GetConfig(long gid)
        {
            long internalGid = gid - FirstGID;
            for (int i = 0; i < TilesConfig.Length; i++)
            {
                if (TilesConfig[i].Id == internalGid)
                {
                    return TilesConfig[i];
                }
            }
            return null;
        }
    }
    [Serializable]
    public class TiledTilesetTileConfig
    {
        public long Id;
        public Property[] Properties;
    }
    [Serializable]
    public class TiledTilesetImage
    {
        public string Source;
        public float Width;
        public float Height;
    }
    [Serializable]
    public class TiledLayer
    {
        public string Name;
        public int Width;
        public int Height;
        public bool Visible = true;
        public Property[] Properties;

        public LayerData data = new LayerData();
    }
    [Serializable]
    public class LayerData
    {
        public long[] tiles;
    }
    [Serializable]
    public class TiledObjectGroup
    {
        public string Name;
        public Property[] Properties;
        public TiledObject[] Objects;
    }
    [Serializable]
    public class TiledObject
    {
        public string id;
        public string type;
        public string Name;
        public float x;
        public float y;
        public float width;
        public float height;
        public Property[] Properties;
    }
    [Serializable]
    public class Property
    {
        public string Name;
        public string Value;

        internal static int GetIntValue(string propertyName, Property[] properties, int defaultValue=0)
        {
            var property = properties.FirstOrDefault(p => p != null && p.Name == propertyName);
            if (property != null)
            {
                int.TryParse(property.Value, out defaultValue);
            }
            return defaultValue;
        }
    }
}
