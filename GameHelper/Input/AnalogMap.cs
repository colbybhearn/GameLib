using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameHelper.Input
{
    [DataContract]
    public class AnalogMap : InputMap
    {
        [DataMember]
        public SortedList<string, AnalogBinding> AnalogBindings { get; private set; }

        public AnalogMap(string alias)
            : base(alias)
        {
            AnalogBindings = new SortedList<string, AnalogBinding>();
        }

        public AnalogMap(string alias, List<AnalogBinding> bindings)
            : this(alias)
        {
            foreach (AnalogBinding ab in bindings)
                AnalogBindings.Add(ab.Alias, ab);
        }

        public AnalogMap(AnalogMap other)
            : this(other.Alias)
        {
            AnalogBindings = new SortedList<string, AnalogBinding>(other.AnalogBindings);
        }

        public void AddBinding(AnalogBinding ab)
        {
            AnalogBindings.Add(ab.Alias, ab);
        }

        public override void Check(InputState state)
        {
            if (Enabled == false)
                return;

            foreach (AnalogBinding ab in AnalogBindings.Values)
                ab.Check(state);
        }

        internal override void Load(InputMap savedMap)
        {
            if (savedMap is AnalogMap == false)
                throw new Exception("AnalogMap attempting to load type other than AnalogMap");

            AnalogMap savedAnalogMap = savedMap as AnalogMap;

            foreach (AnalogBinding saved in savedAnalogMap.AnalogBindings.Values)
            {
                if (AnalogBindings.ContainsKey(saved.Alias))
                {
                    AnalogBinding binding = AnalogBindings[saved.Alias];
                    
                    if(binding.GetType().Equals(saved) == false)
                        throw new Exception("Error loading Button Bindings: Saved binding is not a same type as default binding");

                    if (binding is GamePadThumbStickBinding)
                    {
                        GamePadThumbStickBinding gpjb = binding as GamePadThumbStickBinding;
                        GamePadThumbStickBinding other = saved as GamePadThumbStickBinding;

                    }
                }
            }
        }
    }
}
