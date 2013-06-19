using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameHelper.Input
{
    [DataContract]
    public abstract class InputBinding
    {
        [DataMember]
        public string Alias { get; set; }

        protected InputBinding() { }

        protected InputBinding(string alias)
        {
            Alias = alias;
        }

        public abstract void Check(InputState state);
    }
}
