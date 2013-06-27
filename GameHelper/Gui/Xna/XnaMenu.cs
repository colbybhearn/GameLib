using GameHelper.Input;
using System;
using System.Collections.Generic;

namespace GameHelper.Gui.Xna
{
    class XnaMenu : XnaItemCollection
    {
        public XnaMenuItem FocusedItem { get; private set; }

        public InputCollection Inputs { get; private set; }

        public XnaMenu(string alias, InputCollection inputs)
            : base(alias)
        {
            Inputs = inputs;
        }

        public XnaMenu(string alias, InputCollection inputs, List<XnaMenuItem> xnaMenuItems)
            : this(alias, inputs)
        {
            XnaItems.AddRange(xnaMenuItems);
        }

        public override void AddItem(XnaItem xnaItem)
        {
            if (xnaItem is XnaMenuItem == false)
                throw new Exception("XnaMenu only accepts XnaMenuItems, not XnaItems.");

            base.AddItem(xnaItem);
        }

        protected override void onEnableChange(bool isEnabled)
        {
            Inputs.SetAllMapsState(isEnabled);
        }

        #region Input Callbacks
        public void AnalogCallback(double x, double y)
        {
            foreach (XnaMenuItem item in XnaItems)
            {
                if (item.Check(x, y))
                {
                    item.Focused = true;
                    FocusedItem = item;
                    break;
                }
            }
        }

        public void MoveUpCallback()
        {
            int focus = 1;
            if (FocusedItem != null)
                focus = FocusedItem.Id;

            focus--;

            SetFocus(focus);
        }

        public void MoveDownCallback()
        {
            int focus = -1;
            if (FocusedItem != null)
                focus = FocusedItem.Id;

            focus++;

            SetFocus(focus);
        }

        protected void SetFocus(int focus)
        {
            if (focus <= 0)
                focus += XnaItems.Count;

            FocusedItem = XnaItems.Find(item => (item as XnaMenuItem).Id == focus) as XnaMenuItem;

            FocusedItem.Focused = true;
        }

        public void UseCallback()
        {
            if (FocusedItem != null)
                FocusedItem.Callback();
        }
        #endregion
    }
}
