using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpartansLib.Extensions.Internal;

using GArray = Godot.Collections.Array;
using ConnectFlags = Godot.Object.ConnectFlags;
using OpCodes = Mono.Cecil.Cil.OpCodes;

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

        public override void OnReady(ILProcessor il, MemberReference reference)
        {
            var nodeType = typeof(Node);

            var lastParam = ((MethodReference)reference).Parameters.LastOrDefault();
            IEnumerable<Instruction> instanceInsts = null;
            bool insertingInsanceId = lastParam?.ParameterType.FullName == "System.UInt64" && lastParam.Name == "_triggerId";
            if(insertingInsanceId)
            {
                instanceInsts = il.Compose(
                    il.NewObj(typeof(GArray)),
                    il.Dup(),
                    il.LoadTopLocal(),
                    il.CallMethodVirtual(nodeType, nameof(Node.GetInstanceId)),
                    new[] {il.Create(OpCodes.Box, il.Body.Method.Module.TypeSystem.UInt64)},
                    il.CallMethodVirtual(typeof(GArray), nameof(GArray.Add), typeof(object)),
                    il.PushAndAddLocal(typeof(GArray))
                );
                // static <if((var l = reference.Parameters.Last()).Type.FullName == "System.UInt64" && l.Name == "_triggerId"):
                // GArray local1 = new GArray { local0.GetInstanceId() }
            }

            var topInsts = il.Compose(
                il.PushThis(),
                il.PushString(NodePath),
                il.CallMethod(typeof(NodePath), "op_Implicit", typeof(String)),
                il.CallMethod(nodeType.GetMethods().First(m => m.Name == nameof(Node.GetNode) && !m.ContainsGenericParameters)),
                il.PushAndAddLocal(reference.Module.ImportReference(nodeType)),
            // Node local0 = this.GetNode(NodePath.op_Implicit($NodePath))
                il.Branch(true,
                    il.LoadTopLocal(),
                    il.Branch(false,
                        il.Compose(
                            il.LoadTopLocal(),
                            il.PushString(SignalName),
                            il.CallMethodVirtual(nodeType, nameof(Node.HasSignal), typeof(String))
                        ),
            // if(local0 == null || !local0.HasSignal($SignalName))
                        il.Compose(
                            il.PushString($"'{{0}}' does not have a signal {SignalName}."),
                            il.LoadTopLocal(),
                            il.CallMethodVirtual(nodeType, nameof(Node.GetPath)),
                            il.CallMethod(typeof(String), nameof(String.Format), typeof(String), typeof(System.Object)),
                            il.CallMethod(typeof(GD), nameof(GD.PushError), typeof(String))
                        ),
            // GD.PushError($"'{connectNode.GetPath()}' does not have a signal '$SignalName'.");
                        il.ComposeNoWarn(
                            instanceInsts,
                            il.NullCheckAndExecute(
                                il.LoadTopLocal(),
                                il.Compose(
                                    il.PushString(SignalName),
                                    il.PushThis(),
                                    il.PushString(reference.Name),
                                    (insertingInsanceId ? il.LoadTopLocal() : il.LoadNull()),
                                    il.PushIntConstant((int)Flags),
                                    il.CallMethodVirtual(nodeType, nameof(Node.Connect), typeof(String), typeof(Godot.Object), typeof(String), typeof(GArray), typeof(UInt32))
                                )
                            )
                        )
                    )
                )
            );
            // else {
            //      $instanceInsts
            //      local0.Connect(SignalName, this, $reference.Name, local1, $Flags)
            //  }
        }
    }
}