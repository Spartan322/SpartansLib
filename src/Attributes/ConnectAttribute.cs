using System;
using System.Reflection;
using Godot;
using GArray = Godot.Collections.Array;
using ConnectFlags = Godot.Object.ConnectFlags;
using System.Linq;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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
            GArray binds = null;
            var lastParam = ((MethodInfo)memberInfo).GetParameters().LastOrDefault();
            if (lastParam?.ParameterType == typeof(ulong) && lastParam.Name == "_triggerId")
                binds = new GArray { connectNode.GetInstanceId() };
            connectNode.Connect(SignalName, node, memberInfo.Name, binds, (uint)Flags);
        }
    }
}