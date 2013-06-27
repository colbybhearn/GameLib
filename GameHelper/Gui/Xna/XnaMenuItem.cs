using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameHelper.Gui.Xna
{

    class XnaMenuItem : XnaItem
    {
        public int Id { get; private set; }

        public Handlers.voidEH Callback { get; set; }

        public XnaMenuItem(int id, Rectangle boundingBox, Texture2D texture, Color textureColor, string text, Color textColor, Handlers.voidEH del)
            : base(id.ToString(), boundingBox, texture, textureColor, text, textColor)
        {
            Callback = del;
        }

        public XnaMenuItem(int id, Rectangle boundingBox, Texture2D texture,string text, Handlers.voidEH del)
            : this(id, boundingBox, texture, Color.White, text, Color.White, del) { }

        public XnaMenuItem(int id, Rectangle boundingBox, Texture2D texture, Color textureColor, Handlers.voidEH del)
            : this(id, boundingBox, texture, textureColor, String.Empty, Color.White, del) { }

        public XnaMenuItem(int id, Rectangle boundingBox, Texture2D texture, Handlers.voidEH del)
            : this(id, boundingBox, texture, Color.White, String.Empty, Color.White, del) { }

        public XnaMenuItem(int id, Rectangle boundingBox, string text, Color textColor, Handlers.voidEH del)
            : this(id, boundingBox, null, Color.White, text, textColor, del) { }

        public XnaMenuItem(int id, Rectangle boundingBox, string text, Handlers.voidEH del)
            : this(id, boundingBox, null, Color.White, text, Color.White, del) { }

        public bool Check(double x, double y)
        {
            return BoundingBox.Contains((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
