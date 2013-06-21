using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameHelper.Gui.Xna
{
    class XnaItemCollection
    {
        public bool Enabled
        {
            get
            {
                return Enabled;
            }
            set
            {
                onEnableChange(value);
                Enabled = value;
            }
        }

        public string Alias { get; set; }

        public List<XnaItem> XnaItems { get; private set; }

        protected XnaItemCollection() { }

        public XnaItemCollection(string alias)
        {
            XnaItems = new List<XnaItem>();
        }

        public XnaItemCollection(string alias, List<XnaItem> xnaItems)
            : this(alias)
        {
            XnaItems.AddRange(xnaItems);
        }

        public virtual void AddItem(XnaItem xnaItem)
        {
            XnaItems.Add(xnaItem);
        }

        public virtual void Draw(SpriteBatch sb, SpriteFont sf)
        {
            foreach (XnaItem item in XnaItems)
                item.Draw(sb, sf);
        }

        protected virtual void onEnableChange(bool isEnabled) { }
    }
}
