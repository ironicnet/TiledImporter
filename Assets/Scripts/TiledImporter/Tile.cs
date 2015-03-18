using UnityEngine;
using System.Linq;
using System.Collections;

namespace Ironicnet.TiledImporter
{
    public class Tile : MonoBehaviour
    {

        public SpriteRenderer Renderer;
        public int X;
        public int Y;
        public float ZOrder;
        public int WidthPx;
        public float WidthUnit;
        public int HeightPx;
        public float HeightUnit;
        public float DepthUnit;
        public int Colspan
        {
            get;
            protected set;
        }
        public float Rowspan { get; protected set; }
        public TiledTilesetTileConfig config
        {
            get;
            protected set;
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void SetSprite(Sprite sprite)
        {
            Renderer.sprite = sprite;
        }

        internal void SetConfig(TiledTilesetTileConfig tileConfig)
        {
            config = tileConfig;
            UpdateConfigProperties();

            BoxCollider collider = this.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(((WidthUnit*Colspan)/2)-WidthUnit/2, 0,0);
                collider.size = new Vector3(WidthUnit * Colspan, HeightUnit * Rowspan, DepthUnit);
            }
        }

        private void UpdateConfigProperties()
        {
            if (config != null)
            {
                Colspan = int.Parse(config.Properties.First(p => p.Name == "colspan").Value);
            }
            else
            {
                Colspan = 1;
            }
            Rowspan = 1;
        }

    }
}