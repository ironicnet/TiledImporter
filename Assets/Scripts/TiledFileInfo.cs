using UnityEngine;
using System.Collections;
using System;

public class TiledFileInfo : MonoBehaviour
{
    public string Filename;

    // Use this for initialization
    void Start()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Tiled TiledFile;

    [ContextMenu("Import Tiled File")]
    public void ImportTilemap()
    {

        System.Xml.XmlDocument tilemapXml = new System.Xml.XmlDocument();
        Debug.Log("Importing " + Filename);
        tilemapXml.Load(Filename);

        TiledFile = new Tiled();
        TiledFile.Map = ParseTileMap(tilemapXml.SelectSingleNode("map"));
    }

    private TiledMap ParseTileMap(System.Xml.XmlNode tilemapXml)
    {
        TiledMap map = new TiledMap();
        map.Tilesets = ParseTilesets(tilemapXml);
        map.Layers = ParseLayers(tilemapXml);
        map.ObjectGroups = ParseGroups(tilemapXml);
        map.version = tilemapXml.Attributes["version"].Value;
        map.orientation = GetAttributeValue(tilemapXml, "orientation");
        map.renderorder = GetAttributeValue(tilemapXml, "renderorder");
        map.width = long.Parse(GetAttributeValue(tilemapXml, "width"));
        map.height = long.Parse(GetAttributeValue(tilemapXml, "height"));
        map.tilewidth = int.Parse(GetAttributeValue(tilemapXml, "tilewidth"));
        map.tileheight = int.Parse(GetAttributeValue(tilemapXml, "tileheight"));
        map.nextobjectid = GetAttributeLong(tilemapXml, "nextobjectid");
        return map;
    }

    private long GetAttributeLong(System.Xml.XmlNode tilemapXml, string attribute, long defaultValue=0)
    {
        long output = defaultValue;
        long.TryParse(GetAttributeValue(tilemapXml, "nextobjectid"), out output);
        return output;
    }

    private string GetAttributeValue(System.Xml.XmlNode tilemapXml, string attribute, string defaultValue = null)
    {
        var attr = tilemapXml.Attributes[attribute];
        if (attr != null)
        {
            return attr.Value;
        }
        else
        {
            return defaultValue;
        }
    }

    private TiledObjectGroup[] ParseGroups(System.Xml.XmlNode tilemapXml)
    {
        var groupsXml = tilemapXml.SelectNodes("/map/objectgroup");
        var groups = new TiledObjectGroup[groupsXml.Count];
        for (int i = 0; i < groupsXml.Count; i++)
        {
            groups[i] = ParseObjectGroup(groupsXml.Item(i));
        }
        return groups;
    }

    private TiledLayer[] ParseLayers(System.Xml.XmlNode tilemapXml)
    {
        var layersXml = tilemapXml.SelectNodes("/map/layer");
        var layers = new TiledLayer[layersXml.Count];
        for (int i = 0; i < layersXml.Count; i++)
        {
            layers[i] = ParseLayer(layersXml.Item(i));
        }
        return layers;
    }

    private TiledTileset[] ParseTilesets(System.Xml.XmlNode tilemapXml)
    {
        var tilesetsXml = tilemapXml.SelectNodes("/map/tileset");
        var tilesets = new TiledTileset[tilesetsXml.Count];
        for (int i = 0; i < tilesetsXml.Count; i++)
        {
            tilesets[i] = ParseTileset(tilesetsXml.Item(i));
        }
        return tilesets;
    }

    private Property[] ParseProperties(System.Xml.XmlNode propertiesXml)
    {
        if (propertiesXml == null || propertiesXml.ChildNodes.Count == 0)
        {
            return new Property[0];
        }
        else
        {
            Property[] properties = new Property[propertiesXml.ChildNodes.Count];
            for (int i = 0; i < properties.Length; i++)
            {
                var propXml = propertiesXml.ChildNodes.Item(i);
                properties[i] = new Property()
                {
                    Name = propXml.Attributes["name"].Value,
                    Value = propXml.Attributes["value"].Value
                };
            }
            return properties;
        }
    }

    private TiledObjectGroup ParseObjectGroup(System.Xml.XmlNode groupXml)
    {
        var objectGroup = new TiledObjectGroup();
        for (int i = 0; i < groupXml.Attributes.Count; i++)
        {
            var attr = groupXml.Attributes.Item(i);
            switch (attr.Name)
            {
                case "name":
                    objectGroup.Name = attr.Value;
                    break;
                default:
                    Debug.Log(string.Concat(i, " - ", attr.Name, ": ", attr.Value));
                    break;
            }
        }
        objectGroup.Properties = ParseProperties(groupXml.SelectSingleNode("properties"));
        var objectsInGroup = groupXml.SelectNodes("object");
        if (objectsInGroup == null || objectsInGroup.Count == 0)
        {
            objectGroup.Objects = new TiledObject[0];
        }
        else
        {
            objectGroup.Objects = new TiledObject[objectsInGroup.Count];
            for (int j = 0; j < objectsInGroup.Count; j++)
            {
                var objXml = objectsInGroup.Item(j);
                TiledObject tileObject = ParseTileObject(objXml);
                objectGroup.Objects[j] = tileObject;
            }
        }
        return objectGroup;
    }

    private TiledObject ParseTileObject(System.Xml.XmlNode objXml)
    {
        TiledObject tileObject = new TiledObject();
        for (int k = 0; k < objXml.Attributes.Count; k++)
        {
            var attr = objXml.Attributes.Item(k);
            switch (attr.Name)
            {
                case "id":
                    tileObject.id = attr.Value;
                    tileObject.Name = tileObject.id;
                    break;
                case "type":
                    tileObject.type = attr.Value;
                    break;
                case "x":
                    tileObject.x = float.Parse(attr.Value);
                    break;
                case "y":
                    tileObject.y = float.Parse(attr.Value);
                    break;
                case "width":
                    tileObject.width = float.Parse(attr.Value);
                    break;
                case "height":
                    tileObject.height = float.Parse(attr.Value);
                    break;
                default:
                    Debug.Log(string.Concat(k, " - ", attr.Name, ": ", attr.Value));
                    break;
            }
        }
        tileObject.Properties = ParseProperties(objXml.SelectSingleNode("properties"));
        return tileObject;
    }

    private TiledTileset ParseTileset(System.Xml.XmlNode tilesetXml)
    {
        var tileset = new TiledTileset();
        for (int j = 0; j < tilesetXml.Attributes.Count; j++)
        {
            var attr = tilesetXml.Attributes.Item(j);
            switch (attr.Name)
            {
                case "name":
                    tileset.Name = attr.Value;
                    break;
                case "tilewidth":
                    tileset.TileWidth = int.Parse(attr.Value);
                    break;
                case "tileheight":
                    tileset.TileHeight = int.Parse(attr.Value);
                    break;
                case "firstgid":
                    tileset.FirstGID = int.Parse(attr.Value);
                    break;
                default:
                    Debug.Log(string.Concat(j, " - ", attr.Name, ": ", attr.Value));
                    break;
            }
        }
        var xmlImage = tilesetXml.SelectSingleNode("image");
        if (xmlImage != null)
        {
            tileset.Image = new TiledTilesetImage()
            {
                Source = xmlImage.Attributes["source"].Value,
                Width = float.Parse(xmlImage.Attributes["width"].Value),
                Height = float.Parse(xmlImage.Attributes["height"].Value)
            };
        }
        var tileConfigsXml = tilesetXml.SelectNodes("tile");
        tileset.TilesConfig = new TiledTilesetTileConfig[tileConfigsXml.Count];
        for (int i = 0; i < tileConfigsXml.Count; i++)
        {
            tileset.TilesConfig[i] = new TiledTilesetTileConfig()
            {
                Id = long.Parse(tileConfigsXml[i].Attributes["id"].Value),
                Properties = ParseProperties(tileConfigsXml[i].SelectSingleNode("properties"))
            };
        }
        return tileset;
    }

    private TiledLayer ParseLayer(System.Xml.XmlNode layerXml)
    {
        var layer = new TiledLayer();
        for (int j = 0; j < layerXml.Attributes.Count; j++)
        {
            var attr = layerXml.Attributes.Item(j);
            switch (attr.Name)
            {
                case "name":
                    layer.Name = attr.Value;
                    break;
                case "width":
                    layer.Width = int.Parse(attr.Value);
                    break;
                case "height":
                    layer.Height = int.Parse(attr.Value);
                    break;
                case "visible":
                    layer.Visible = int.Parse(attr.Value) == 1;
                    break;
                default:
                    Debug.Log(string.Concat(j, " - ", attr.Name, ": ", attr.Value));
                    break;
            }
        }
        layer.Properties = ParseProperties(layerXml.SelectSingleNode("properties"));
        layer.data = new LayerData();
        var tilesInfo = layerXml.SelectNodes("data/tile/@gid");
        layer.data.tiles = new long[tilesInfo.Count];
        for (int i = 0; i < tilesInfo.Count; i++)
        {
            layer.data.tiles[i] = long.Parse(tilesInfo[i].Value);
        }
        return layer;
    }

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
    }
}
