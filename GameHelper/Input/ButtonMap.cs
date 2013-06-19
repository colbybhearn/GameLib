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
    public class ButtonMap : InputMap
    {
        [DataMember]
        public SortedList<string, ButtonBinding> ButtonBindings { get; private set; }

        public ButtonMap(string alias)
            : base(alias)
        {
            ButtonBindings = new SortedList<string, ButtonBinding>();
        }

        public ButtonMap(string alias, List<ButtonBinding> defaultBindings)
            : this(alias)
        {
            foreach (ButtonBinding bb in defaultBindings)
                ButtonBindings.Add(bb.Alias, bb);
        }

        public ButtonMap(ButtonMap other)
            : this(other.Alias)
        {
            ButtonBindings = new SortedList<string, ButtonBinding>(other.ButtonBindings);
        }

        public void AddBinding(ButtonBinding bb)
        {
            ButtonBindings.Add(bb.Alias, bb);
        }

        public override void Check(InputState state)
        {
            if (Enabled == false)
                return; 

            foreach (ButtonBinding bb in ButtonBindings.Values)
                bb.Check(state);
        }

        internal override void Load(InputMap savedMap)
        {
            if (savedMap is ButtonMap == false)
                throw new Exception("ButtonMap attempting to load type other than ButtonMap");

            ButtonMap savedButtonMap = savedMap as ButtonMap;

            foreach (ButtonBinding saved in savedButtonMap.ButtonBindings.Values)
            {
                if (this.ButtonBindings.ContainsKey(saved.Alias))
                {
                    ButtonBinding binding = ButtonBindings[saved.Alias];

                    if(binding.GetType().Equals(saved) == false)
                        throw new Exception("Error loading Button Bindings: Saved binding is not a same type as default binding");
                        

                    if (binding is KeyBinding)
                    {
                        KeyBinding kb = binding as KeyBinding;
                        KeyBinding other = saved as KeyBinding;
                        kb.Key = other.Key;
                        kb.Modifiers = other.Modifiers;
                    }
                    else if (binding is GamePadButtonBinding)
                    {
                        GamePadButtonBinding gb = binding as GamePadButtonBinding;
                        GamePadButtonBinding other = saved as GamePadButtonBinding;
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
