using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;
using System.Runtime.Serialization;

namespace GameHelper.Input
{
    [DataContract]
    public class KeyBinding
    {
        [DataMember]
        public Keys Key { get; set; }
        
        public KeyEvent KeyEvent { get; set; }

        [XmlIgnore]
        public KeyBindingDelegate Callback { get; set; }

        [DataMember]
        public KeyModifier Modifiers { get; set; }

        [DataMember]
        public string Alias { get; set; }

        /*[DataMember]
        public bool Ctrl { get; set; }
        [DataMember]
        public bool Shift { get; set; }
        [DataMember]
        public bool Alt { get; set; }*/

        public KeyBinding() { }

        public KeyBinding(string alias, Keys k, KeyModifier mods, KeyEvent kevent)
        {
            Alias = alias;
            Key = k;
            Modifiers = mods;
            KeyEvent = kevent;
        }

        public KeyBinding(string alias, Keys k, KeyModifier mods, KeyEvent kevent, KeyBindingDelegate kdel)
            : this(alias, k, mods, kevent)
        {
            Callback = kdel;
        }

        public KeyBinding(string alias, Keys k, KeyEvent kevent)
            : this(alias, k, KeyModifier.None, kevent) { }

        public KeyBinding(string alias, Keys k, KeyEvent kevent, KeyBindingDelegate kdel)
            : this(alias, k, KeyModifier.None, kevent, kdel) { }

        private void CallDelegate()
        {
            if (Callback == null)
                return;
            Callback();
        }

        public void Check(KeyboardState last, KeyboardState curr)
        {
            if (last == null)
                return;
            // if the modifier keys don't match what we're looking for
            if (!matchMods(curr))
                // then it's not a match
                return;
            //based on the type of event
            switch (KeyEvent)
            {
                case KeyEvent.Released:
                    if (!(keyDown(Key, last) && keyUp(Key, curr)))
                        return;
                    break;
                case KeyEvent.Pressed:
                    if (!(keyUp(Key, last) && keyDown(Key, curr)))
                        return;
                    break;
                case KeyEvent.Down:
                    if (!(keyDown(Key, curr)))
                        return;
                    break;
                case KeyEvent.Up:
                    if (!(keyUp(Key, curr)))
                        return;
                    break;
                default:
                    return;
            }
            // we made it through! Do something freakin' awesome
            CallDelegate();
        }

        private bool matchMods(KeyboardState curr)
        {
            // Don't care!
            if (Modifiers.HasFlag(KeyModifier.Any))
                return true;

            // if this binding should have no modifiers then make sure all of the modifier keys are up
            if (Modifiers.HasFlag(KeyModifier.None))
            {
                if (curr.IsKeyDown(Keys.LeftControl) || curr.IsKeyDown(Keys.RightControl))
                    return false;
                if (curr.IsKeyDown(Keys.LeftShift) || curr.IsKeyDown(Keys.RightShift))
                    return false;
                if (curr.IsKeyDown(Keys.LeftShift) || curr.IsKeyDown(Keys.RightShift))
                    return false;
            }

            // if this binding uses the Control key,
            if (Modifiers.HasFlag(KeyModifier.Control))
                // and neither of the Left Control or Right Control keys are down,
                if (!(curr.IsKeyDown(Keys.LeftControl) || curr.IsKeyDown(Keys.RightControl)))
                    // then it's not a match
                    return false;

            // if this binding uses the Shift key,
            if (Modifiers.HasFlag(KeyModifier.Shift))
                // and neither of the Left Shift and Right Shift keys are down,
                if (!(curr.IsKeyDown(Keys.LeftShift) || curr.IsKeyDown(Keys.RightShift)))
                    // then it's not a match
                    return false;

            // if this binding uses the Alt key,
            if (Modifiers.HasFlag(KeyModifier.Alt))
                // and neither of the Left Alt and Right Alt keys are down,
                if (!(curr.IsKeyDown(Keys.LeftAlt) || curr.IsKeyDown(Keys.RightAlt)))
                    // then it's not a match
                    return false;

            // all disqualifications have been exhausted, modifiers match this binding
            return true;
        }
        public bool keyDown(Keys key, KeyboardState ks)
        {
            return ks.IsKeyDown(key);
        }
        public bool keyUp(Keys key, KeyboardState ks)
        {
            return ks.IsKeyUp(key);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Modifiers.HasFlag(KeyModifier.Control))
                sb.Append("Ctrl + ");
            if (Modifiers.HasFlag(KeyModifier.Shift))
                sb.Append("Shift + ");
            if (Modifiers.HasFlag(KeyModifier.Alt))
                sb.Append("Alt + ");
            sb.Append(Key.ToString());
            return sb.ToString();
        }
    }
}
