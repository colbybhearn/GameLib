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

    [DataContract]
    public abstract class ButtonBinding
    { 
        [DataMember]
        public ButtonEvent ButtonEvent { get; set; }

        [IgnoreDataMember]
        public KeyBindingDelegate Callback { get; set; }

        [DataMember]
        public string Alias { get; set; }

        protected ButtonBinding() { }

        protected ButtonBinding(string alias, ButtonEvent kevent)
        {
            Alias = alias;
            ButtonEvent = kevent;
        }

        protected ButtonBinding(string alias, ButtonEvent kevent, KeyBindingDelegate kdel)
            : this(alias, kevent)
        {
            Callback = kdel;
        }

        protected void CallDelegate()
        {
            if (Callback == null)
                return;
            Callback();
        }

        public abstract void Check(InputState state);

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
