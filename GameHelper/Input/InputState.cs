using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace GameHelper.Input
{
    public class InputState
    {
        /* Vision
         * This class handles keeping the current state of all input devices up to date and knows about the last state of them
         * This way the generic Input Handler classes can take an InputState into a Check funciton, thus being able to force the use
         * of a check function via virtual function in the ButtonBinding class for buttons, and whatever the other class shall be for
         * joysticks and mouse input. WOO
         */
        public GamePadState GamePadState { get; private set; }
        public GamePadState GamePadStateLast { get; private set; }

        public GamePadDeadZone DeadZone { get; set; }

        public KeyboardState KeyboardState { get; private set; }
        public KeyboardState KeyboardStateLast { get; private set; }

        public PlayerIndex PlayerIndex { get; private set; }

        public InputState()
            : this(PlayerIndex.One) { }

        public InputState(PlayerIndex p)
        {
            GamePadState = new GamePadState();
            GamePadStateLast = new GamePadState();

            DeadZone = GamePadDeadZone.Circular;

            KeyboardState = new KeyboardState();
            KeyboardStateLast = new KeyboardState();

            PlayerIndex = p;
        }

        public void Update()
        {
            GamePadStateLast = GamePadState;
            GamePadState = GamePad.GetState(PlayerIndex, DeadZone);

            KeyboardStateLast = KeyboardState;
            KeyboardState = Keyboard.GetState(PlayerIndex);
        }
    }
}
