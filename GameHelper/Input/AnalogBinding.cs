using System.ComponentModel;
using System.Runtime.Serialization;

namespace GameHelper.Input
{
    public enum AnalogEvent
    {
        [Description("On Move")]    Moved,
        [Description("Always")]     Always,
    }

    public enum AnalogData
    {
        [Description("Report Delta")]       Delta,
        [Description("Report Absolute")]    Absolute,
    }

    public delegate void AnalogBindingDelegate(double xValue, double yValue);

    [DataContract]
    public abstract class AnalogBinding : InputBinding
    {
        [DataMember]
        public AnalogEvent AnalogEvent { get; set; }

        [DataMember]
        public AnalogData AnalogData { get; set; }

        [IgnoreDataMember]
        public AnalogBindingDelegate Callback { get; set; }

        protected AnalogBinding(string alias, AnalogEvent aEvent, AnalogData aData)
            : base(alias)
        {
            AnalogEvent = aEvent;
            AnalogData = aData;
        }

        protected AnalogBinding(string alias, AnalogEvent aEvent, AnalogData aData, AnalogBindingDelegate aDel)
            : this(alias, aEvent, aData)
        {
            Callback = aDel;
        }

        protected void CallDelegate(double x, double y)
        {
            if (Callback == null)
                return;
            Callback(x, y);
        }

        protected void Check(double x, double y, double xLast, double yLast)
        {
            double xDelta = x - xLast;
            double yDelta = y - yLast;

            bool passCheck = true;

            switch (AnalogEvent)
            {
                case AnalogEvent.Moved:
                    if (xDelta == 0 && yDelta == 0)
                        passCheck = false;
                    break;
                case AnalogEvent.Always:
                    break;
            }

            if (passCheck)
            {
                switch (AnalogData)
                {
                    case AnalogData.Absolute:
                        CallDelegate(x, y);
                        break;
                    case AnalogData.Delta:
                        CallDelegate(xDelta, yDelta);
                        break;
                }
            }
        }
    }
}
