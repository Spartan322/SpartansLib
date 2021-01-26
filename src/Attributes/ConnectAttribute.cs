using System;
using System.Reflection;
using Godot;
using ConnectFlags = Godot.Object.ConnectFlags;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConnectAttribute : SpartansLibAttribute
    {
        public string NodePath;
        public string SignalName;
        public ConnectFlags Flags;

        public ConnectAttribute(string signalName, string nodePath, ConnectFlags flags = 0)
        {
            NodePath = nodePath;
            SignalName = signalName;
            Flags = flags;
        }

        public override void OnReady<T>(T node, MemberInfo memberInfo)
        {
            var connectNode = node.GetNode(NodePath);
            if (connectNode == null) return;
            if (!connectNode.HasSignal(SignalName))
            {
                GD.PushError($"'{connectNode.GetPath()}' does not have a signal '{SignalName}'");
                return;
            }
            connectNode.Connect(SignalName, node, memberInfo.Name, null, (uint)Flags);
        }
    }
}