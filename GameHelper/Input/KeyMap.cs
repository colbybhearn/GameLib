using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GameHelper.Input
{
    public enum KeyEvent
    {
        [Description("While Not Pressed")]  Up, // happens to be up right now           
        [Description("While Pressed")]      Down, // happens to be down right now
        [Description("On Press")]           Pressed, // just pressed since last update
        [Description("On Release")]         Released, // just released since last update
    }

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

    /// <summary>
    /// Stores a set of keys, mapped to a set of possible bindings
    /// </summary>
    [DataContract]
    public class KeyMap
    {
        public bool Enabled;

        [DataMember]
        public string Alias;
        [DataMember]
        public SortedList<string, KeyBinding> KeyBindings { get; private set; }

        public KeyMap()
        {
        }

        public KeyMap(string alias)
        {
            Alias = alias;
            KeyBindings = new SortedList<string, KeyBinding>();
        }

        public KeyMap(string alias, List<KeyBinding> defaultBindings)
        {
            Alias = alias;
            KeyBindings = new SortedList<string, KeyBinding>();
            foreach (KeyBinding kb in defaultBindings)
                KeyBindings.Add(kb.Alias, kb);
        }

        public KeyMap(KeyMap other)
        {
            Alias = other.Alias;
            KeyBindings = new SortedList<string, KeyBinding>(other.KeyBindings);
        }

        public void AddKeyBinding(KeyBinding kb)
        {
            KeyBindings.Add(kb.Alias, kb);
        }

        public void Check(KeyboardState last, KeyboardState current)
        {
            if (Enabled == false)
                return; 

            foreach (KeyBinding kb in KeyBindings.Values)
                kb.Check(last, current);
        }

        internal void LoadOverrides(KeyMap savedkm)
        {
            foreach (KeyBinding saved in savedkm.KeyBindings.Values)
            {
                // if the game can use that preference,
                if (this.KeyBindings.ContainsKey(saved.Alias))
                {
                    // get the default for that input, 
                    KeyBinding kb = KeyBindings[saved.Alias];
                    // and totally override it with the saved preference
                    kb.Key = saved.Key;
                    kb.Modifiers = saved.Modifiers;
                }
            }
        }
    }
}
