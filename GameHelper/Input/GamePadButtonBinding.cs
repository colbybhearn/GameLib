using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace GameHelper.Input
{
    [DataContract]
    class GamePadButtonBinding : ButtonBinding
    {
        [DataMember]
        public Buttons Button { get; set; }

        public GamePadButtonBinding(string alias, Buttons b, ButtonEvent kevent)
            : this(alias, b, kevent, null) { }

        public GamePadButtonBinding(string alias, Buttons b, ButtonEvent kevent, ButtonBindingDelegate kdel)
            : base(alias, kevent, kdel)
        {
            Button = b;
            Callback = kdel;
        }

        public override void Check(InputState state)
        {
            base.Check(
                state.GamePadState.IsButtonDown(Button),
                state.GamePadStateLast.IsButtonDown(Button)
                );
        }

        public override string ToString()
        {
            return Button.ToString();
        }
    }
}
