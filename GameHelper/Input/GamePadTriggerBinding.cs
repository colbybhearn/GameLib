using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameHelper.Input
{
    public enum Trigger
    {
        Left,
        Right,
    }

    [DataContract]
    public class GamePadTriggerBinding : AnalogBinding
    {
        /* Note
         * Trigger only uses the X component, not a Y component.
         */

        [DataMember]
        public Trigger Trigger { get; set; }

        public GamePadTriggerBinding(string alias, Trigger trigger, AnalogEvent aEvent, AnalogData aData)
            : this(alias, trigger, aEvent, aData, null) { }

        public GamePadTriggerBinding(string alias, Trigger trigger, AnalogEvent aEvent, AnalogData aData, AnalogBindingDelegate aDel)
            : base(alias, aEvent, aData, aDel)
        {
            Trigger = trigger;
        }

        public override void Check(InputState state)
        {
            double position;
            double positionLast;

            switch (Trigger)
            {
                case Trigger.Left:
                    position = state.GamePadState.Triggers.Left;
                    positionLast = state.GamePadStateLast.Triggers.Left;
                    break;
                case Trigger.Right:
                    position = state.GamePadState.Triggers.Right;
                    positionLast = state.GamePadStateLast.Triggers.Right;
                    break;
                default:
                    position = 0;
                    positionLast = 0;
                    break;
            }

            base.Check(
                position, 0,
                positionLast, 0
                );
        }
    }
}
