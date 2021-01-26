using System;
using Godot;

namespace SpartansLib.Extensions
{
    public static class PackedSceneExtensions
    {
        public static T Instance<T>(
            this PackedScene packed,
            PackedScene.GenEditState state = PackedScene.GenEditState.Disabled
        ) where T : Node
            => (T)packed.Instance(state);
    }
}
