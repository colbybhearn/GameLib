using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace GameHelper.Input
{
    [Flags]
    public enum KeyModifier
    {
        [Description("No modifier allowed")]        None    = 1,
        [Description("Requires alt modifier")]      Alt     = 2,
        [Description("Requires control modifier")]  Control = 4,
        [Description("Requires shift modifier")]    Shift   = 8,
        [Description("Any modifier allowed")]       Any     = 15,
    }

    public delegate void KeyBindingDelegate();

    [DataContract]
    public class KeyBinding : ButtonBinding
    {
        [DataMember]
        public Keys Key { get; set; }

        [DataMember]
        public KeyModifier Modifiers { get; set; }

        public KeyBinding() { }

        public KeyBinding(string alias, Keys k, KeyModifier mods, ButtonEvent kevent)
            : this(alias, k, mods, kevent, null) { }

        public KeyBinding(string alias, Keys k, KeyModifier mods, ButtonEvent kevent, KeyBindingDelegate kdel)
            : base(alias, kevent, kdel)
        {
            Key = k;
            Modifiers = mods;
        }

        public KeyBinding(string alias, Keys k, ButtonEvent kevent)
            : this(alias, k, KeyModifier.None, kevent, null) { }

        public KeyBinding(string alias, Keys k, ButtonEvent kevent, KeyBindingDelegate kdel)
            : this(alias, k, KeyModifier.None, kevent, kdel) { }

        public override void Check(InputState state)
        {
            if (!matchMods(state))
                return;

            base.Check(
                state.KeyboardState.IsKeyDown(Key),
                state.KeyboardStateLast.IsKeyDown(Key)
                );
        }

        /*public void Check(KeyboardState last, KeyboardState curr)
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
        }*/

        private bool matchMods(InputState state)
        {
            // Don't care!
            if (Modifiers.HasFlag(KeyModifier.Any))
                return true;

            // if this binding should have no modifiers then make sure all of the modifier keys are up
            if (Modifiers.HasFlag(KeyModifier.None))
            {
                if (state.KeyboardState.IsKeyDown(Keys.LeftControl) || state.KeyboardState.IsKeyDown(Keys.RightControl))
                    return false;
                if (state.KeyboardState.IsKeyDown(Keys.LeftShift) || state.KeyboardState.IsKeyDown(Keys.RightShift))
                    return false;
                if (state.KeyboardState.IsKeyDown(Keys.LeftShift) || state.KeyboardState.IsKeyDown(Keys.RightShift))
                    return false;
            }

            // if this binding uses the Control key,
            if (Modifiers.HasFlag(KeyModifier.Control))
                // and neither of the Left Control or Right Control keys are down,
                if (!(state.KeyboardState.IsKeyDown(Keys.LeftControl) || state.KeyboardState.IsKeyDown(Keys.RightControl)))
                    // then it's not a match
                    return false;

            // if this binding uses the Shift key,
            if (Modifiers.HasFlag(KeyModifier.Shift))
                // and neither of the Left Shift and Right Shift keys are down,
                if (!(state.KeyboardState.IsKeyDown(Keys.LeftShift) || state.KeyboardState.IsKeyDown(Keys.RightShift)))
                    // then it's not a match
                    return false;

            // if this binding uses the Alt key,
            if (Modifiers.HasFlag(KeyModifier.Alt))
                // and neither of the Left Alt and Right Alt keys are down,
                if (!(state.KeyboardState.IsKeyDown(Keys.LeftAlt) || state.KeyboardState.IsKeyDown(Keys.RightAlt)))
                    // then it's not a match
                    return false;

            // all disqualifications have been exhausted, modifiers match this binding
            return true;
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
