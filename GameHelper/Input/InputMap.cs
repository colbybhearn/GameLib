using System.Runtime.Serialization;

namespace GameHelper.Input
{
    [DataContract]
    public abstract class InputMap
    {
        public bool Enabled;

        [DataMember]
        public string Alias;

        protected InputMap() { }

        protected InputMap(string alias)
        {
            Alias = alias;
        }

        public abstract void Check(InputState state);

        internal abstract void Load(InputMap savedMap);
    }
}
