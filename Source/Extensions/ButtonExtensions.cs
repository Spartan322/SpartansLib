using System;
using Godot;

namespace SpartansLib.Extensions
{
    public static class ButtonExtensions
    {
        public static bool SetTextFor(this Button button, bool value, string untoggled = "<", string toggled = ">")
        {
            if (!value) button.Text = toggled;
            else button.Text = untoggled;
            return button.Pressed;
        }

        public static bool SetToggleTextFor(this Button button, bool toggleValue, string untoggled = "<", string toggled = ">")
        {
            toggleValue = !toggleValue;
            if (toggleValue) button.Text = toggled;
            else button.Text = untoggled;
            return toggleValue;
        }
    }
}
