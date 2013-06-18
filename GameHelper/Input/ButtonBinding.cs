using System.Runtime.Serialization;

namespace GameHelper.Input
{
    [DataContract]
    public abstract class ButtonBinding
    { 
        [DataMember]
        public KeyEvent KeyEvent { get; set; }

        [IgnoreDataMember]
        public KeyBindingDelegate Callback { get; set; }

        [DataMember]
        public string Alias { get; set; }

        protected ButtonBinding() { }

        protected ButtonBinding(string alias, KeyEvent kevent)
        {
            Alias = alias;
            KeyEvent = kevent;
        }

        protected ButtonBinding(string alias, KeyEvent kevent, KeyBindingDelegate kdel)
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

            switch (KeyEvent)
            {
                case KeyEvent.Released:
                    if (!(wasButtonDown && isButtonUp))
                        passCheck = false;
                    break;
                case KeyEvent.Pressed:
                    if (!(wasButtonUp && isButtonDown))
                        passCheck = false;
                    break;
                case KeyEvent.Down:
                    if (!(isButtonDown))
                        passCheck = false;
                    break;
                case KeyEvent.Up:
                    if (!(isButtonUp))
                        passCheck = false;
                    break;
            }

            if (passCheck)
                CallDelegate();
        }
    }
}
