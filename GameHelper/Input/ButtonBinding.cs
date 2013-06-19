using System.ComponentModel;
using System.Runtime.Serialization;

namespace GameHelper.Input
{
    
    public enum ButtonEvent
    {
        [Description("While Not Pressed")]  Up, // happens to be up right now           
        [Description("While Pressed")]      Down, // happens to be down right now
        [Description("On Press")]           Pressed, // just pressed since last update
        [Description("On Release")]         Released, // just released since last update
    }

    public delegate void ButtonBindingDelegate();

    [DataContract]
    public abstract class ButtonBinding : InputBinding
    { 
        [DataMember]
        public ButtonEvent ButtonEvent { get; set; }

        [IgnoreDataMember]
        public ButtonBindingDelegate Callback { get; set; }

        protected ButtonBinding(string alias, ButtonEvent bEvent)
            : base(alias)
        {
            ButtonEvent = bEvent;
        }

        protected ButtonBinding(string alias, ButtonEvent bEvent, ButtonBindingDelegate bDel)
            : this(alias, bEvent)
        {
            Callback = bDel;
        }

        protected void CallDelegate()
        {
            if (Callback == null)
                return;
            Callback();
        }

        // Still a bit messy ... but better
        protected void Check(bool isButtonDown, bool wasButtonDown)
        {
            bool isButtonUp = !isButtonDown; // Cleaner code
            bool wasButtonUp = !wasButtonDown;

            bool passCheck = true;

            switch (ButtonEvent)
            {
                case ButtonEvent.Released:
                    if (!(wasButtonDown && isButtonUp))
                        passCheck = false;
                    break;
                case ButtonEvent.Pressed:
                    if (!(wasButtonUp && isButtonDown))
                        passCheck = false;
                    break;
                case ButtonEvent.Down:
                    if (!(isButtonDown))
                        passCheck = false;
                    break;
                case ButtonEvent.Up:
                    if (!(isButtonUp))
                        passCheck = false;
                    break;
            }

            if (passCheck)
                CallDelegate();
        }
    }
}
