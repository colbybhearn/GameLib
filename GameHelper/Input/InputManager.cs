﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;


/* dynamic input schemes
 * it makes sense to let the game decide what key bindings should actively be checked
 * and what those keys should do
 * so when you change mode, the game knows that and can update the input manager's configuration
 * so how do we tie the keys to the assets?
 * any kind of mode identifier can be used
 * how do we show all the options on the settings form?
 * We need modes of play
 * each mode has an alias
 * when keybindings are added, their added for a specific mode.
 * when keybindings are shown, their shown grouped by mode
 * if we add it all into the input manager, we can toggle groups of keys remotely as enabled or not.
 * This allows the base game to add Camera controls, or other GenericInputGroups
 * Then the specific game can add it's specific SpecificInputGroups
 * 
 */



/* Keymapping
         * Keep a list of possible bindings
         * each binding has a name / purpose, and an assigned key.
         * To set the binding, we need a special screen on which bindings are listed, input is monitored, and changes can be made.
         * save the binding somewhere outside the solution, per game, per user.
         * read in the binding per game, per user.
         * 
         */
namespace GameHelper.Input
{
    public enum InputMode
    {
        Setup,
        Chat,
        Mapped
    }

    public delegate void ChatDelegate(List<Microsoft.Xna.Framework.Input.Keys> pressedKeys);

    public class InputManager
    {
        /* Vision:
         * 
         * KeyMap should be an end-to-end map of possible actions in terms of (alias to key to delegate)
         * the alias, 
         * the key,
         * the callback
         * 
         * Defaults are handled by each specific game creating a default hard-coded keymap
         * hand the default keymap to the input manager.
         * The input manager will attempt to load a saved keymap according to the game name.
         * Any loaded key keybindings will be used to overwrite the default keybindings
         * 
         * Watchlist can be created from the keyMaps' final keybindings.
         * KeyMap can have a Check method that does what InputManagers' Update method does
         * 
         */

        public InputMode Mode { get; set; }

        InputState inputState;
        //KeyboardState lastState = new KeyboardState();
        //KeyboardState currentState = new KeyboardState();
        //public KeyMap keyMap;
        public SortedList<InputMode, Delegate> InputModeDelegates;
        public String game;
        Settings frmSettings;
        ButtonMapCollection keyMaps = new ButtonMapCollection();

        public InputManager(String gameName, ButtonMapCollection defaultKeyMapcollection)
        {
            game = gameName;
            keyMaps = ButtonMapCollection.Load(game, defaultKeyMapcollection);
            Mode = InputMode.Mapped;
            InputModeDelegates = new SortedList<InputMode, Delegate>();
            inputState = new InputState();
        }

        public void DisableAllKeyMaps()
        {
            keyMaps.DisableAllButtonMaps();
        }

        public void EnableKeyMap(string id)
        {
            keyMaps.EnableButtonMap(id);
        }

        public void DisableKeyMap(string id)
        {
            keyMaps.DisableButtonMap(id);
        }

        public void AddInputMode(InputMode m, Delegate d)
        {
            InputModeDelegates.Add(m, d);
        }

        public void Update()
        {
            inputState.Update();

            switch(Mode)
            {
                    /* TODO
                     * InputMode.Setup and InputMode.Chat not updated to use new InputState with GamePadBindings
                     */
                case InputMode.Setup:
                    frmSettings.ProcessKey(inputState.KeyboardState);
                    break;
                case InputMode.Chat:
                    Delegate d;
                    if(InputModeDelegates.TryGetValue(Mode, out d))
                        ((ChatDelegate)d)(GetPressedKeysWithShift(inputState.KeyboardStateLast, inputState.KeyboardState));
                    break;
                case InputMode.Mapped:
                    foreach (ButtonMap map in keyMaps.buttonMaps.Values)
                    {
                        if (map != null)
                            map.Check(inputState);
                    }
                    break;
            }
        }

        //Will always contain shift if shift is held
        private List<Microsoft.Xna.Framework.Input.Keys> GetPressedKeysWithShift(KeyboardState lastState, KeyboardState currentState)
        {
            Keys[] last = lastState.GetPressedKeys();
            Keys[] current = currentState.GetPressedKeys();
            List<Keys> pressed = new List<Keys>();

            foreach(Keys k in current)
                if(last.Contains(k) == false || k.HasFlag(Keys.LeftShift) || k.HasFlag(Keys.RightShift))
                    pressed.Add(k);

            return pressed;
        }
        
        public void Save()
        {
            ButtonMapCollection.Save(keyMaps);
            //KeyMap.SaveKeyMap(keyMap);
        }
        
        public void EditSettings()
        {
            frmSettings = new Settings(keyMaps);
            InputMode lastMode = Mode;
            Mode = InputMode.Setup;            
            System.Windows.Forms.DialogResult dr = frmSettings.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                keyMaps = frmSettings.keyMaps;
                Save();
            }
            frmSettings.Dispose();
            Mode = lastMode;
        }
    }
}
