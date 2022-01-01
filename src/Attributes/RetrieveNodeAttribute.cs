using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpartansLib.Common;
using SpartansLib.Extensions.Internal;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RetrieveNodeAttribute : SpartansLibAttribute
    {
        public string Name;
        public bool Required;

        public RetrieveNodeAttribute(string name = null, bool required = true)
        {
            Name = name;
            Required = required;
        }

        public override void OnReady(ILProcessor il, MemberReference reference)
        {
            if (Name == null)
                Name = FilterName(reference.Name);

            var nodeRegistryType = typeof(NodeRegistry);
            var nodeRegistryRef = reference.Module.ImportReference(nodeRegistryType);

            var insts = il.Compose(
                il.PushString(Name),
                il.CallMethod(nodeRegistryRef, nameof(NodeRegistry.GetOrNull), reference.Module.TypeSystem.String),
                il.PushAndAddLocal(typeof(Node)),
                !Required
                ? new Instruction[] {}
                : il.Branch(false,
                    il.LoadTopLocal(),
                    il.Compose(
                        il.PushString($"Registered Node '{Name}' must be registered but could not be found."),
                        il.PushAndAddLocal(il.Body.Method.Module.TypeSystem.String),
                        il.Branch(false,
                            il.LoadProperty(typeof(Engine), nameof(Engine.EditorHint)),
                            il.Compose(
                                il.LoadTopLocal(),
                                il.NewObj(typeof(NullReferenceException), typeof(String)),
                                il.Throw()
                            )
                        ),
                        il.PushIntConstant(1),
                        new[] {il.Create(OpCodes.Newarr, reference.Module.TypeSystem.Object)},
                        il.Dup(),
                        il.PushIntConstant(0),
                        il.LoadTopLocal(),
                        new[] {il.Create(OpCodes.Stelem_Ref)},
                        il.CallMethod(typeof(Logger), nameof(Logger.PushError), typeof(System.Object[])),
                        il.Return()
                    )
                )
            );

            TypeDefinition memberType = null;
            if(reference is FieldReference fr)
                memberType = fr.FieldType.Resolve();
            else if(reference is PropertyReference pr)
                memberType = (reference as PropertyReference)?.PropertyType.Resolve();
            if(memberType == null); // TODO: attribute on wrong declaration

            if(memberType.ContainsGenericParameter
                && memberType.CanBeAssignedTo(typeof(IEnumerable)))
            {
                insts = il.Compose(insts,
                    il.PushThis(),
                    il.PushThis(),
                    il.CallMethod(
                        typeof(ChildExtensions),
                        nameof(ChildExtensions.GetChildren),
                        new [] {memberType.GenericParameters[0]})
                );
                if(memberType.CanBeAssignedTo(typeof(IList<>).MakeGenericRef(memberType.GenericParameters[0])))
                    insts = il.Compose(insts,
                        il.CallMethod(typeof(Enumerable), "ToList", new[] {memberType.GenericParameters[0]})
                    );
                insts = il.Compose(insts, il.SetMemberVariable(reference));
                // <reference> = this.GetChildren<<reference.Type.GenericParameters[0]>>()
                // static <if(reference.Type is IList<reference.Type.GenericParameters[0])>:
                //      <reference> = <reference>.ToList()
            }
            else
                insts = il.Compose(insts,
                    il.PushThis(),
                    il.LoadTopLocal(1),
                    (Required
                        ? il.CastClass(memberType)
                        : il.IsInstance(memberType)
                    ),
                    il.SetMemberVariable(reference)
            );
            // static <if(!Required)>: <reference> = (<reference.Type>)local0
            // static <else>: <reference> = local0 as <reference.Type>

            if(Required)
            {
                var warningInsts = il.Branch(false,
                    il.Compose(il.PushThis(), il.LoadMemberVariable(reference)),
                    il.Compose(
                        il.PushString($"Registered Node '{Name}' can not be assigned to '{memberType.FullName}'."),
                        il.CallMethod(typeof(GD), nameof(GD.PushWarning), typeof(String))
                    )
                );
            // static <if(!Required)>: if(<reference> == null)
            //      GD.PushWarning($"'{<Path>}' can not be assigned to '{<reference.Type.FullName}'");
                il.Prepend(il.Compose(insts, warningInsts));
                return;
            }

            il.Prepend(insts);

            // var val = NodeRegistry.GetOrNull(Name);
            // if (val == null)
            // {
            //     if (!Required) return;
            //     var err = $"Node '{Name}' must be registered but could not be found.";
            //     if (!Engine.EditorHint) throw new NullReferenceException(err);
            //     Logger.PushError(err);
            //     return;
            // }

            // var varInfo = new VariableMemberInfo(memberInfo);
            // if (varInfo.VariableType.ContainsGenericParameters
            //     && typeof(IEnumerable<>)
            //         .MakeGenericType(varInfo.VariableType.GenericTypeArguments[0])
            //         .IsAssignableFrom(varInfo.VariableType))
            // {
            //     var assignable = val.GetChildren<Node>().Where(n => varInfo.VariableType.GenericTypeArguments[0].IsInstanceOfType(n));
            //     varInfo.SetValue(node, varInfo.VariableType == typeof(IList<>)
            //         .MakeGenericType(varInfo.VariableType.GenericTypeArguments[0])
            //         ? assignable.ToList()
            //         : assignable);
            // }
            // else if (varInfo.VariableType.IsInstanceOfType(val))
            //     varInfo.SetValue(node, val);
            // else
            //     GD.PushWarning($"{val} not of type {varInfo.VariableType.FullName}");
        }
    }
}