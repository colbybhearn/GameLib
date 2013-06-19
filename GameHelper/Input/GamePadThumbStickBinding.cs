using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GameHelper.Input
{
    public enum ThumbStick
    {
        [Description("Left Thumbstick")]    Left,
        [Description("Right THumbstick")]   Right,
    }

    [DataContract]
    public class GamePadThumbStickBinding : AnalogBinding
    {
        [DataMember]
        public ThumbStick Stick { get; set; }

        public GamePadThumbStickBinding(string alias, ThumbStick tStick, AnalogEvent aEvent, AnalogData aData)
            : this(alias, tStick, aEvent, aData, null) { }

        public GamePadThumbStickBinding(string alias, ThumbStick tStick, AnalogEvent aEvent, AnalogData aData, AnalogBindingDelegate aDel)
            : base(alias, aEvent, aData, aDel)
        {
            Stick = tStick;
        }

        public override void Check(InputState state)
        {
            Vector2 stickPosition;
            Vector2 stickPositionLast;

            switch (Stick)
            {
                case ThumbStick.Left:
                    stickPosition = state.GamePadState.ThumbSticks.Left;
                    stickPositionLast = state.GamePadStateLast.ThumbSticks.Left;
                    break;
                case ThumbStick.Right:
                    stickPosition = state.GamePadState.ThumbSticks.Right;
                    stickPositionLast = state.GamePadStateLast.ThumbSticks.Right;
                    break;
                default:
                    stickPosition = Vector2.Zero;
                    stickPositionLast = Vector2.Zero;
                    break;
            }

            base.Check(
                stickPosition.X, stickPosition.Y,
                stickPositionLast.X, stickPositionLast.Y
                );
        }
    }
}
