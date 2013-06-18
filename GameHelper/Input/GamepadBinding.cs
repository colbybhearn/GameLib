using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace GameHelper.Input
{
    [DataContract]
    class GamePadBinding : ButtonBinding
    {
        [DataMember]
        public Buttons Button { get; set; }
        
        GamePadBinding() { }

        public GamePadBinding(string alias, Buttons b, KeyEvent kevent)
            : this(alias, b, kevent, null) { }

        public GamePadBinding(string alias, Buttons b, KeyEvent kevent, KeyBindingDelegate kdel)
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
