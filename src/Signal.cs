using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpartansLib.Extensions;

namespace SpartansLib
{
    public struct SignalInstance<T>
        where T : Godot.Object
    {
        public readonly Signal Signal;
        public readonly T Object;

        public SignalInstance(T obj, Signal sig)
        {
            Object = obj;
            Signal = sig;
        }

        public T Emit(params object[] args)
        {
            Object.EmitSignal(Signal, args);
            return Object;
        }

        public Error Connect<T2>(T2 target, string method, object[] binds, uint flags)
            where T2 : Godot.Object
            => Object.Connect(Signal, target, method, binds, flags);

        public Error Connect<T2>(T2 target, string method, params object[] binds)
            where T2 : Godot.Object
            => Connect(target, method, binds, 0);
    }

    public class Signal
    {
        public readonly string Name;
        private readonly List<Type> ArgumentTypes;
        private Signal(string name, params Type[] argTypes)
        {
            Name = name;
            ArgumentTypes = argTypes.ToList();
        }

        public Type this[int index] => ArgumentTypes[index];

        public int ArgCount => ArgumentTypes?.Count ?? 0;
        public static implicit operator Signal(string str) => new Signal(str);
        //public static implicit operator string(Signal obj) => obj.Name;

        public SignalInstance<T> For<T>(T obj)
            where T : Godot.Object
            => new SignalInstance<T>(obj, this);

        public static Signal MakeCustom(string name, params Type[] argTypes)
            => new Signal(name, argTypes);

        public static Signal MakeCustom<T>(string name)
            => new Signal(name, typeof(T));

        public static Signal MakeCustom<T, T2>(string name)
            => new Signal(name, typeof(T), typeof(T2));

        public static Signal MakeCustom<T, T2, T3>(string name)
            => new Signal(name, typeof(T), typeof(T2), typeof(T3));

        public static Signal MakeCustom<T, T2, T3, T4>(string name)
            => new Signal(name, typeof(T), typeof(T2), typeof(T3), typeof(T4));

        public static Signal MakeCustom<T, T2, T3, T4, T5>(string name)
            => new Signal(name, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public static readonly Signal ScriptChanged = "script_changed";
        public static readonly Signal Ready = "ready";
        public static readonly Signal Renamed = "renamed";
        public static readonly Signal TreeEntered = "tree_entered";
        public static readonly Signal TreeExited = "tree_exited";
        public static readonly Signal TreeExiting = "tree_exiting";
        public static readonly Signal VisibilityChanged = "visibility_changed";
        public static readonly Signal Draw = "draw";
        public static readonly Signal Hide = "hide";
        public static readonly Signal ItemRectChanged = "item_rect_changed";
        public static readonly Signal FocusEntered = "focus_entered";
        public static readonly Signal FocusExited = "focus_exited";
        public static readonly Signal GuiInput = new Signal("gui_input", typeof(InputEvent));
        public static readonly Signal MinimumSizeChanged = "minimum_size_changed";
        public static readonly Signal ModalClosed = "modal_closed";
        public static readonly Signal MouseEntered = "mouse_entered";
        public static readonly Signal MouseExited = "mouse_exited";
        public static readonly Signal Resized = "resized";
        public static readonly Signal SizeFlagsChanged = "size_flags_changed";
        public static readonly Signal ButtonDown = "button_down";
        public static readonly Signal ButtonUp = "button_up";
        public static readonly Signal Pressed = "pressed";
        public static readonly Signal Toggled = new Signal("toggled", typeof(bool));
        public static readonly Signal TextChangeRejected = "text_change_rejected";
        public static readonly Signal TextChanged = new Signal("text_changed", typeof(string));
        public static readonly Signal TextEntered = new Signal("text_entered", typeof(string));
        public static readonly Signal MetaClicked = new Signal("meta_clicked", typeof(object));
        public static readonly Signal MetaHoverEnded = new Signal("meta_hover_ended", typeof(object));
        public static readonly Signal MetaHoverStarted = new Signal("meta_hover_started", typeof(object));
    }
}
