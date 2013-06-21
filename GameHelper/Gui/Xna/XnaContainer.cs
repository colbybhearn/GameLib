using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameHelper.Gui.Xna
{
    /* Vision
     * Container is base class for XNA Gui Controls
     * These controls should be able to have text and/or a sprite to draw.
     * Some controls should behave like a menu (May need a Menu manager)
     *  This menu system should allow for clickable (tie into the input manager automatically) containers
     *  These clickable menu items should contain a callback that way the game can handle transitions and whatnot
     *  This menu system should work with a GamePad/Keyboard, meaning up/down selection ties to input manager, along with confirmation buttons
     */
    public class XnaContainer
    {
        public bool Enabled { get; set; }

        public string Alias { get; private set; }

        public Rectangle BoundingBox { get; set; }

        public Texture2D Texture { get; set; }
        public Color TextureColor { get; set; }

        public string Text { get; set; }
        public Color TextColor { get; set; }

        protected XnaContainer() { }

        public XnaContainer(string alias, Rectangle boundingBox, Texture2D texture, Color textureColor, string text, Color textColor)
        {
            Alias = alias;
            BoundingBox = boundingBox;
            Texture = texture;
            TextureColor = textureColor;
            Text = text;
            TextColor = textColor;
        }

        public XnaContainer(string alias, Rectangle boundingBox, Texture2D texture, string text)
            : this(alias, boundingBox, texture, Color.White, text, Color.White) { }

        public XnaContainer(string alias, Rectangle boundingBox, Texture2D texture)
            : this(alias, boundingBox, texture, Color.White, String.Empty, Color.White) { }

        public XnaContainer(string alias, Rectangle boundingBox, Texture2D texture, Color textureColor)
            : this(alias, boundingBox, texture, textureColor, String.Empty, Color.White) { }

        public XnaContainer(string alias, Rectangle boundingBox, string text)
            : this(alias, boundingBox, null, Color.White, text, Color.White) { }

        public XnaContainer(string alias, Rectangle boundingBox, string text, Color textColor)
            : this(alias, boundingBox, null, Color.White, text, textColor) { }


        public virtual void Draw(SpriteBatch sb, SpriteFont sf)
        {
            if (Texture != null)
                sb.Draw(Texture, BoundingBox, Color.White);

            if(String.IsNullOrEmpty(Text) == false)
            {
                Vector2 textPos = new Vector2(BoundingBox.Center.X, BoundingBox.Center.Y);
                textPos -= sf.MeasureString(Text)/2;
                sb.DrawString(sf, Text, textPos, Color.White);
            }
        }

        public virtual void DebugDraw(SpriteBatch sb, SpriteFont sf)
        {
            throw new Exception("XnaContainer.DebugDraw() is not implemented yet");
        }
    }
}
