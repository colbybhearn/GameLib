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

    /// <summary>
    /// Stores a set of keys, mapped to a set of possible bindings
    /// </summary>
    [DataContract]
    public class ButtonMap
    {
        public bool Enabled;

        [DataMember]
        public string Alias;
        [DataMember]
        public SortedList<string, ButtonBinding> ButtonBindings { get; private set; }

        public ButtonMap()
        {
        }

        public ButtonMap(string alias)
        {
            Alias = alias;
            ButtonBindings = new SortedList<string, ButtonBinding>();
        }

        public ButtonMap(string alias, List<KeyBinding> defaultBindings)
        {
            Alias = alias;
            ButtonBindings = new SortedList<string, ButtonBinding>();
            foreach (KeyBinding kb in defaultBindings)
                ButtonBindings.Add(kb.Alias, kb);
        }

        public ButtonMap(ButtonMap other)
        {
            Alias = other.Alias;
            ButtonBindings = new SortedList<string, ButtonBinding>(other.ButtonBindings);
        }

        public void AddBinding(ButtonBinding bb)
        {
            ButtonBindings.Add(bb.Alias, bb);
        }

        public void Check(InputState state)
        {
            if (Enabled == false)
                return; 

            foreach (KeyBinding kb in ButtonBindings.Values)
                kb.Check(state);
        }

        internal void LoadOverrides(ButtonMap savedkm)
        {
            foreach (ButtonBinding saved in savedkm.ButtonBindings.Values)
            {
                // if the game can use that preference,
                if (this.ButtonBindings.ContainsKey(saved.Alias))
                {
                    // get the default for that input, 
                    ButtonBinding binding = ButtonBindings[saved.Alias];
                    // and totally override it with the saved preference
                    if (binding is KeyBinding)
                    {
                        if (saved is KeyBinding == false)
                            throw new Exception("Error loading Button Bindings: Saved binding is not a KeyBinding");

                        KeyBinding kb = binding as KeyBinding;
                        KeyBinding other = saved as KeyBinding;
                        kb.Key = other.Key;
                        kb.Modifiers = other.Modifiers;
                    }
                    else if (binding is GamePadBinding)
                    {
                        if (saved is GamePadBinding == false)
                            throw new Exception("Error loading Button Bindings: Saved binding is not a GamePadBinding");

                        GamePadBinding gb = binding as GamePadBinding;
                        GamePadBinding other = saved as GamePadBinding;
                        gb.Button = other.Button;
                    }
                    else
                    {
                        throw new Exception("Unknown ButtonBinding type being loaded. Did you add a new type and forget to handle loading?");
                    }
                }
            }
        }
    }
}
