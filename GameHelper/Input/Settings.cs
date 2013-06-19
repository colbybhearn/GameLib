using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace GameHelper.Input
{
    public partial class Settings : Form
    {
        public InputCollection keyMaps;
        public Settings(InputCollection kmc)
        {
            InitializeComponent();
            keyMaps = kmc;
            AddKeyBindingGuiControls();
        }

        /// <summary>
        /// Adds controls (custom tab pages containing keybindingControls) for displaying each keybinding
        /// </summary>
        public void AddKeyBindingGuiControls()
        {
            for (int kmi = 0; kmi < keyMaps.inputMaps.Count; kmi++)
            {
                FlowLayoutTabPage fltp = new FlowLayoutTabPage();
                tcControlGroups.TabPages.Add(fltp);
                InputMap im = keyMaps.inputMaps.Values[kmi];
                if (im is ButtonMap == false)
                    continue;
                ButtonMap bm = im as ButtonMap;
                fltp.Text = bm.Alias;
                foreach (Input.KeyBinding kb in bm.ButtonBindings.Values)
                {
                    KeyBindingControl kbc = new KeyBindingControl(kb);
                    
                    fltp.AddControl(kbc);
                }
            }
        }

        /// <summary>
        /// Processes the current keystate to set a keybinding.
        /// </summary>
        /// <param name="currentState"></param>
        internal void ProcessKey(Microsoft.Xna.Framework.Input.KeyboardState currentState)
        {
            foreach (TabPage tp in tcControlGroups.Controls)
            {
                // if the tab page is one of the custom tab pages (should always be the case)
                if (tp is FlowLayoutTabPage)
                {
                    // make it so
                    FlowLayoutTabPage fltp = tp as FlowLayoutTabPage;
                    // in case this tab page contains the active keybinding control needing to set its binding.
                    fltp.ProcessKey(currentState);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
