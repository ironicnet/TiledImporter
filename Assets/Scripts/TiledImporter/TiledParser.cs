using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ironicnet.TiledImporter
{
    public class TiledParser
    {
        public Tiled TiledFile;

        public Tiled Parse(string filename)
        {

            System.Xml.XmlDocument tilemapXml = new System.Xml.XmlDocument();
            tilemapXml.Load(filename);

            TiledFile = new Tiled();
            TiledFile.Map = ParseTileMap(tilemapXml.SelectSingleNode("map"));
            return TiledFile;
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

        private long GetAttributeLong(System.Xml.XmlNode tilemapXml, string attribute, long defaultValue = 0)
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
                        break;
                    case "name":
                        tileObject.Name = attr.Value;
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
    }
}
