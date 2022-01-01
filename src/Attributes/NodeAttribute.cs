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
    public class NodeAttribute : SpartansLibAttribute
    {
        public string Path;
        public bool Required;
        public string PathName;

        public NodeAttribute(string path = null, bool required = true)
        {
            Path = path;
            Required = required;
        }

        public override void OnReady(ILProcessor il, MemberReference reference)
        {
            var pathNameMemberName = $"{reference.Name}Path";
            var tType = il.Body.Method.DeclaringType.Resolve();
            var pathNameCheck = ((IEnumerable<MemberReference>)tType.Fields).Concat(tType.Properties);
            if (PathName == null && pathNameCheck.Any(memRef => memRef.Name == pathNameMemberName))
                PathName = pathNameMemberName;
            MemberReference pathNameMember = null;
            if (PathName != null)
            {
                pathNameMember = pathNameCheck.FirstOrDefault(memRef => memRef.Name == PathName);
                if (pathNameMember != null
                    && !(pathNameMember is FieldReference
                    || pathNameMember is PropertyReference))
                {
                    PathName = null;
                    // string path = null;
                    // path = pathNameFound is FieldReference fref
                    //     ? fref.
                    //     :
                    //new VariableMemberInfo(pathNameFound).GetValue(node)?.ToString();
                    // if (path != null)
                    //     Path = path;
                }
            }
            if (Path == null)
                Path = FilterName(reference.Name);

            var nodeType = typeof(Node);
            var nodeRef = reference.Module.ImportReference(nodeType);

            TypeDefinition memberType = null;
            if(reference is FieldReference fr)
                memberType = fr.FieldType.Resolve();
            else if(reference is PropertyReference pr)
                memberType = (reference as PropertyReference)?.PropertyType.Resolve();
            if(memberType == null); // TODO: attribute on wrong declaration

            var insts = il.Compose(
                il.PushThis(),
                pathNameMember != null
                    ? il.Compose(il.PushThis(), il.LoadMemberVariable(pathNameMember))
                    : il.PushString(Path),
                il.CallMethod(nodeType, nameof(Node.GetNodeOrNull),  new[] {memberType}, typeof(NodePath)),
                il.PushAndAddLocal(memberType),
            // Node local0 = this.GetNode(Path)
                !Required
                    ? new Instruction[] {}
                    : il.Branch(false, // if condition is 0/null and false, doesn't branch
                        il.LoadTopLocal(),
            // if(local0 == null)
            // static <if(!Required)>: return
                        il.Compose(
                            pathNameMember != null
                                ? il.Compose(
                                    il.PushString("Node '"),
                                    il.PushThis(),
                                    il.LoadMemberVariable(pathNameMember),
                                    il.PushString($"' of '{memberType.FullName}' is required but could not be found."),
                                    il.CallStaticMethod(typeof(String), nameof(String.Concat), typeof(String), typeof(String), typeof(String))
                                )
                                : il.PushString($"Node '{Path}' of '{memberType.FullName}' is required but could not be found."),
                            il.PushAndAddLocal(il.Body.Method.Module.TypeSystem.String),
            // var local1 = $"Node '{Path}' is required but could not be found."
                            il.Branch(false,
                                il.LoadProperty(typeof(Engine), nameof(Engine.EditorHint)),
                                il.Compose(
                                    il.LoadTopLocal(),
                                    il.NewObj(typeof(NullReferenceException), typeof(String)),
                                    il.Throw()
                                )
                            ),
            // if (!Engine.EditorHint) throw new NullReferenceException(err)
                        il.PushIntConstant(1),
                        new[] {il.Create(OpCodes.Newarr, reference.Module.TypeSystem.Object)},
                        il.Dup(),
                        il.PushIntConstant(0),
                        il.LoadTopLocal(),
                        new[] {il.Create(OpCodes.Stelem_Ref)},
                        il.CallMethod(typeof(Logger), nameof(Logger.PushError), typeof(System.Object[]))
            // Logger.PushError(local1)
            // return
                    )
                )
            );

            // if(Required)
            // {
            //     insts = il.Compose(insts, il.Branch(false,
            //         il.Compose(il.PushThis(), il.LoadMemberVariable(reference)),
            //         il.Compose(
            //             pathNameMember != null
            //                 ? il.Compose(
            //                     il.PushString("'"),
            //                     il.PushThis(),
            //                     il.LoadMemberVariable(pathNameMember),
            //                     il.PushString($"' can not be assigned to '{memberType.FullName}'."),
            //                     il.CallMethod(typeof(String), "Concat", new[] {typeof(String), typeof(String), typeof(String)})
            //                 )
            //                 : il.PushString($"'{Path}' can not be assigned to '{memberType.FullName}'."),
            //             il.CallMethod(typeof(GD), nameof(GD.PushWarning), typeof(String))
            //         )
            //     ));
            // // static <if(!Required)>: if(<reference> == null)
            // //      GD.PushWarning($"'{<Path>}' can not be assigned to '{<reference.Type.FullName}'");
            // }

            if(memberType.ContainsGenericParameter
                && memberType.CanBeAssignedTo(typeof(IEnumerable)))
            {
                insts = il.Compose(insts,
                    il.PushThis(),
                    il.Dup(),
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

            il.Prepend(insts);

            // var val = node.GetNode(Path);
            // if (val == null)
            // {
            //     if (!Required) return;
            //     var err = $"Node '{Path}' is required but could not be found.";
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
