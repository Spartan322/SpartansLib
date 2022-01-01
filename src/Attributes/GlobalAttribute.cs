using System;
using System.Collections.Generic;
using Godot;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpartansLib.Extensions.Internal;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GlobalAttribute : SpartansLibAttribute
    {
        public string Name;
        public bool? Singular = null;

        public GlobalAttribute(string name = null)
        {
            Name = name;
        }

        public GlobalAttribute(string name, bool singular) : this(name)
        {
            Singular = singular;
        }

        public override void OnConstructor(ILProcessor il, MemberReference reference)
        {
            if (Name == null)
                Name = reference.Name;

            IEnumerable<Instruction> insts;

            //var counter = 2;
            //var currentName = string.Format(Name, "");
            switch (Singular)
            {
                case false:
                    insts = il.Compose(
                        il.PushIntConstant(2),
                        il.PushAndAddLocal(typeof(Int32)),
                        il.PushString(Name),
                        il.PushAndAddLocal(typeof(String)),
                        il.WhileLoop(
                            false,
                            il.Compose(
                                il.LoadTopLocal(),
                                il.CallMethod(typeof(NodeRegistry), nameof(NodeRegistry.IsRegistered), typeof(String))
                            ),
                            il.Compose(
                                il.PushString(Name),
                                il.LoadTopLocal(1),
                                il.Dup(),
                                il.PushIntConstant(1),
                                new[] {il.Create(OpCodes.Add)},
                                il.PushTopLocal(1),
                                new[] {il.Create(OpCodes.Box)},
                                il.CallMethod(typeof(String), nameof(String.Format), typeof(String), typeof(System.Object)),
                                il.PushTopLocal()
                            )
                        ),
                        il.PushThis(),
                        il.LoadTopLocal(),
                        il.LoadNull(),
                        il.CallMethod(
                            il.Body.Method.Module.ImportReference(typeof(NodeRegistry)),
                            nameof(NodeRegistry.Register),
                            new[] {(TypeReference)reference},
                            (TypeReference)reference, il.Body.Method.Module.TypeSystem.String)
                            );
                    return;
                case true:
                    insts = il.Branch(true,
                        il.Compose(
                            il.PushString(Name),
                            il.CallMethod(typeof(NodeRegistry), nameof(NodeRegistry.IsRegistered), typeof(String))
                        ),
                        il.Compose(
                            il.PushString($"'{Name}' is already registered in '{nameof(NodeRegistry)}'."),
                            il.CallMethod(typeof(GD), nameof(GD.PushError), typeof(String))
                        )
                    );
                    // if (NodeRegistry.IsRegistered(Name))
                    // {
                    //     GD.PushError($"{Name} already register in {nameof(NodeRegistry)}");
                    //     return;
                    // }
                    break;
                default:
                    insts = il.Branch(true,
                        il.Compose(
                            il.PushString(Name),
                            il.CallMethod(typeof(NodeRegistry), nameof(NodeRegistry.IsRegistered), typeof(String))
                        ),
                        il.Compose(
                            il.PushThis(),
                            il.PushString(Name),
                            il.LoadNull(),
                            il.CallMethod(
                                il.Body.Method.Module.ImportReference(typeof(NodeRegistry)),
                                nameof(NodeRegistry.RegisterOrReplace),
                                new[] {(TypeReference)reference},
                                (TypeReference)reference, il.Body.Method.Module.TypeSystem.String),
                            new[] {il.Create(OpCodes.Pop)}
                        )
                    );
                    // if (NodeRegistry.IsRegistered(Name))
                    // {
                    //     node.RegisterOrReplace(Name);
                    //     return;
                    // }
                    break;
            }
            insts = il.Compose(insts,
                il.PushThis(),
                il.PushString(Name),
                il.LoadNull(),
                il.CallMethod(
                    il.Body.Method.Module.ImportReference(typeof(NodeRegistry)),
                    nameof(NodeRegistry.Register),
                    new[] {(TypeReference)reference},
                    (TypeReference)reference, il.Body.Method.Module.TypeSystem.String)
            );
            // node.Register(currentName);
        }
    }
}
