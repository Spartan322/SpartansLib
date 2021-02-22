using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace SpartansLib.Injection
{
    public class GodotInjector : IDisposable
    {
        // private readonly bool _debugChecksEnabled;
        private readonly string _targetModuleAssemblyPath;
        private readonly string _spartansLibModuleAssemblyPath;
        private readonly string _godotMainAssemblyDir;
        private readonly string _godotLinkedAssembliesDir;

        private ModuleDefinition _targetModule;
        private ModuleDefinition _spartansLibModule;
        private TypeDefinition _godotNodeTypeDef;
        // private TypeDefinition _godotNodePathTypeDef;
        // private TypeDefinition _godotExportAttrTypeDef;
        // private TypeDefinition _godotPropertyHintTypeDef;

        // private TypeDefinition _godotUtilsNodeAttrTypeDef;
        // private TypeDefinition _godotUtilsAutoloadAttrTypeDef;
        // private TypeDefinition _godotUtilsExportRenameAttrTypeDef;
        private TypeDefinition _spartanLibsAttributeTypeDef;

        // private MethodReference _nodePathConversionMethodDef;
        // private MethodReference _nodePathIsEmptyMethodDef;
        // private MethodReference _nodeGetNodeMethodDef;
        // private MethodReference _exportAttrCtorDef;
        // private MethodReference _godotPushErrorMethodDef;

        private string _configuration;

        public GodotInjector(
            string targetDllPath,
            string spartansLibDllPath,
            string godotMainAssemblyDir,
            string godotLinkedAssembliesDir,
            string configuration,
            bool debugCheckEnabled
        )
        {
            _targetModuleAssemblyPath = targetDllPath;
            _spartansLibModuleAssemblyPath = spartansLibDllPath;
            _godotMainAssemblyDir = godotMainAssemblyDir;
            _godotLinkedAssembliesDir = godotLinkedAssembliesDir;
            // _debugChecksEnabled = debugCheckEnabled;
            _configuration = configuration;
        }

        private void ReadTargetAssembly()
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(_godotMainAssemblyDir);
            assemblyResolver.AddSearchDirectory(_godotLinkedAssembliesDir);

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = assemblyResolver,
                ReadSymbols = !_configuration.Contains("Release"),
                ReadWrite = true
            };

            _targetModule = ModuleDefinition.ReadModule(_targetModuleAssemblyPath, readerParameters);
            _spartansLibModule = ModuleDefinition.ReadModule(_spartansLibModuleAssemblyPath, readerParameters);

            var godotSharpModule = ModuleDefinition.ReadModule(_godotMainAssemblyDir + "GodotSharp.dll");

            _godotNodeTypeDef = _targetModule.ImportReference(godotSharpModule.GetType("Godot.Node")).Resolve();
            // _nodeGetNodeMethodDef = _targetModule.ImportReference(
            // FindMethodByFullName(_godotNodeTypeDef, "Godot.Node Godot.Node::GetNodeOrNull(Godot.NodePath)"));

            // _godotNodePathTypeDef = _nodeGetNodeMethodDef.Parameters[0].ParameterType.Resolve();

            // _nodePathConversionMethodDef = _targetModule.ImportReference(
            //     FindMethodByFullName(_godotNodePathTypeDef,
            //         "Godot.NodePath Godot.NodePath::op_Implicit(System.String)"));
            //
            // _nodePathIsEmptyMethodDef = _targetModule.ImportReference(
            //     FindMethodByFullName(_godotNodePathTypeDef, "System.Boolean Godot.NodePath::IsEmpty()"));

            // _godotExportAttrTypeDef = GetResolvedType("Godot.ExportAttribute", true);
            // if (_godotExportAttrTypeDef != null)
            // {
            //     _godotPropertyHintTypeDef = GetResolvedType("Godot.PropertyHint", true);
            //     // _exportAttrCtorDef = _targetModule.ImportReference(
            //     //     FindMethodBySimpleName(_godotExportAttrTypeDef, ".ctor"));
            // }

            // _godotUtilsNodeAttrTypeDef = GetResolvedType("SpartansLib.Attributes.NodeAttribute", true);
            // _godotUtilsAutoloadAttrTypeDef = GetResolvedType("SpartansLib.Attributes.SingletonAttribute", true);
            // _godotUtilsExportRenameAttrTypeDef = GetResolvedType("GodotCSUtils.ExportRenameAttribute", true);
            _spartanLibsAttributeTypeDef = GetResolvedType("SpartansLib.Attributes.SpartansLibAttribute", true);
            var gdClassTypeDef = _targetModule.ImportReference(godotSharpModule.GetType("Godot.GD")).Resolve();
            // _godotPushErrorMethodDef = _targetModule.ImportReference(
            //     FindMethodByFullName(gdClassTypeDef, "System.Void Godot.GD::PushError(System.String)"));
        }

        private TypeDefinition GetResolvedType(string typeFullName, bool optional = false)
        {
            var typeDef = _spartansLibModule.GetType(typeFullName);
            if (typeDef != null)
                return typeDef;
            if (optional)
                return null;
            throw new Exception($"cannot locate {typeFullName} type ref");
        }

        public void Dispose() => _targetModule?.Dispose();

        private static bool HasBaseClass(TypeDefinition typeDef, IMetadataTokenProvider baseTypeDef)
        {
            if (typeDef == baseTypeDef)
                return true;
            return typeDef.BaseType != null && HasBaseClass(typeDef.BaseType.Resolve(), baseTypeDef);
        }

        // private static TypeReference GetFieldOrPropertyType(IMemberDefinition member)
        // {
        //     switch (member)
        //     {
        //         case FieldDefinition field:
        //             return field.FieldType;
        //         case PropertyDefinition property:
        //             return property.PropertyType;
        //         default:
        //             throw new Exception("must be field or property");
        //     }
        // }

        private IEnumerable<TypeDefinition> GetNodeClasses(TypeDefinition type = null)
            => type == null
                ? _targetModule.Types.Where(t => HasBaseClass(t, _godotNodeTypeDef))
                : type.NestedTypes.Where(t => HasBaseClass(t, _godotNodeTypeDef));

        private IEnumerable<TypeDefinition> GetAllNodeClasses(TypeDefinition type = null)
        {
            IEnumerable<TypeDefinition> topNodeClasses = GetNodeClasses(type).ToArray();
            return topNodeClasses.Aggregate(topNodeClasses,
                (current, topNode)
                    => current.Concat(GetAllNodeClasses(topNode)));
        }

        // private static MethodDefinition FindMethodByFullName(TypeDefinition typeDef, string fullName)
        //     => typeDef.Methods.FirstOrDefault(m => m.FullName == fullName);

        private static MethodDefinition FindMethodBySimpleName(TypeDefinition typeDef, string simpleName)
            => typeDef.Methods.FirstOrDefault(m => m.Name == simpleName);

        // private static FieldDefinition FindFieldBySimpleName(TypeDefinition typeDef, string simpleName)
        //     => typeDef.Fields.FirstOrDefault(m => m.Name == simpleName);
        //
        // private static PropertyDefinition FindPropertyBySimpleName(TypeDefinition typeDef, string simpleName)
        //     => typeDef.Properties.FirstOrDefault(m => m.Name == simpleName);

        private static List<CustomAttribute> GetFieldAttributes(ICustomAttributeProvider memberDef, MemberReference attrTypeDef)
            => memberDef.CustomAttributes.Where(attribute => attribute.Constructor.DeclaringType.Resolve().BaseType.FullName == attrTypeDef.FullName).ToList();

        private MethodDefinition GetOrCreateReadyMethod(TypeDefinition nodeClass)
        {
            Console.WriteLine($"Searching for ready method in {nodeClass.FullName}");
            var methodDef = FindMethodBySimpleName(nodeClass, "_Ready");
            if (methodDef != null) return methodDef;
            Console.WriteLine("Ready method not found, creating one");
            methodDef = new MethodDefinition("_Ready",
                MethodAttributes.Public
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual, _targetModule.TypeSystem.Void);

            var il = methodDef.Body.GetILProcessor();
            il.Emit(OpCodes.Ret);

            nodeClass.Methods.Add(methodDef);

            return methodDef;
        }

        // private void ProcessAutoloadLinks(TypeDefinition nodeClass)
        // {
        //     var members = new List<IMemberDefinition>();
        //     members.AddRange(nodeClass.Fields);
        //     members.AddRange(nodeClass.Properties);
        //
        //     MethodDefinition readyMethod = null;
        //
        //     foreach (var memberDef in members)
        //     {
        //         var autoloadAttr = GetFieldAttribute(memberDef, _godotUtilsAutoloadAttrTypeDef);
        //         if (autoloadAttr == null)
        //             continue;
        //
        //         var name = (string) autoloadAttr.ConstructorArguments[0].Value ?? memberDef.Name;
        //
        //         //Console.WriteLine($"found Autoload: class - {nodeClass}; member - {memberDef}; name - {name}");
        //
        //         if (readyMethod == null) readyMethod = GetOrCreateReadyMethod(nodeClass);
        //
        //         var il = readyMethod.Body.GetILProcessor();
        //         var instructions = GenerateSetLinkFromPathInstructions(memberDef, $"/root/{name}", il);
        //         InsertInstructionsIntoExistingMethod(il, instructions);
        //     }
        // }

        private static IEnumerable<TypeDefinition> GetAllNestedClassesIn(TypeDefinition type)
        {
            var nestedTypes = type.NestedTypes;
            return nestedTypes.Aggregate<TypeDefinition, IEnumerable<TypeDefinition>>(
                nestedTypes,
                (current, nested)
                    => current.Concat(GetAllNestedClassesIn(nested)));
        }

        private static bool TryGetAttributeMethod(ICustomAttribute attr, TypeReference nodeClass, string name, out MethodReference method)
        {
            var attrType = attr.AttributeType.Resolve();
            var methodDef = attrType.Methods.FirstOrDefault(m => m.Name == name);
            if (methodDef == null)
            {
                method = null;
                return false;
            }
            method = nodeClass.Module.ImportReference(methodDef);

            if (!method.HasGenericParameters) return true;
            var genericInstance = new GenericInstanceMethod(method);
            genericInstance.GenericArguments.Add(nodeClass);
            method = genericInstance;
            return true;
        }

        private MethodReference ImportStaticMethodFor(Type type, string name, Type argType)
            => _targetModule.ImportReference(type.GetMethod(
                name,
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { argType },
                null));


        private MethodReference ImportMethodFor(Type type, string name, Type argType)
            => _targetModule.ImportReference(type.GetMethod(
                name,
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { argType },
                null));

        private IEnumerable<Instruction> ComposeForCustomAttribute(CustomAttribute attribute, ILHelper ilh)
        {
            IEnumerable<Instruction> result = null;
            foreach (var caArg in attribute.ConstructorArguments)
            {
                Console.WriteLine($"Composing load '{caArg.Type.FullName}':'{caArg.Value}' instruction");

                result = ILHelper.Compose(
                    result,
                    ilh.PushTypeWithValue(caArg.Type, caArg.Value));
            }

            result = ILHelper.Compose(
                result,
                ilh.NewObj(_targetModule.ImportReference(attribute.Constructor)));

            return result;
        }

        private MethodReference _typeRuntimeHandle;
        private MethodReference _methodRuntimeHandle;
        private MethodReference _fieldRuntimeHandle;
        private MethodReference _getPropertyMethod;
        private IEnumerable<Instruction> ComposeMemberReferenceInstruction(TypeReference typeRef, IMemberDefinition memberDef, ILHelper ilh)
        {
            // ReSharper disable once InvertIf
            if (_typeRuntimeHandle == null)
            {
                _typeRuntimeHandle = ImportStaticMethodFor(
                    typeof(Type),
                    nameof(Type.GetTypeFromHandle),
                    typeof(RuntimeTypeHandle));
                _methodRuntimeHandle = ImportStaticMethodFor(
                    typeof(MethodBase),
                    nameof(MethodBase.GetMethodFromHandle),
                    typeof(RuntimeMethodHandle));
                _fieldRuntimeHandle = ImportStaticMethodFor(
                    typeof(FieldInfo),
                    nameof(FieldInfo.GetFieldFromHandle),
                    typeof(RuntimeFieldHandle));
                _getPropertyMethod = ImportMethodFor(
                    typeof(Type),
                    nameof(Type.GetProperty),
                    typeof(string));
            }

            return ILHelper.Compose(
                memberDef is TypeDefinition td
                    ? new[] { ilh.LoadType(td), ilh.CallMethod(_typeRuntimeHandle) }
                    : memberDef is MethodDefinition md
                    ? new[] { ilh.LoadMethodRef(md), ilh.CallMethod(_methodRuntimeHandle) }
                    : memberDef is FieldDefinition fd
                    ? new[] { ilh.LoadFieldRef(fd), ilh.CallMethod(_fieldRuntimeHandle) }
                    : new[]
                    {
                        ilh.LoadType(typeRef),
                        ilh.CallMethod(_typeRuntimeHandle),
                        ilh.PushString(memberDef.Name),
                        ilh.CallMethod(_getPropertyMethod)
                    }
            );
        }

        private void ProcessAttributes(TypeDefinition nodeClass, TaskLoggingHelper log)
        {
            Console.WriteLine($"Processing Injection Attributes for '{nodeClass.FullName}'");

            var members = new List<IMemberDefinition>(nodeClass.Fields);
            members.AddRange(nodeClass.Properties);
            members.AddRange(nodeClass.Methods);
            members.AddRange(nodeClass.NestedTypes);

            var ctorMethod = nodeClass.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
            if (ctorMethod == null)
            {
                var msg = $"{nameof(SpartansLib)}.{nameof(Injection)} does not support injecting objects that lack a default constructor. '{nodeClass.FullName}' found lacking a default constructor.";
                if (log == null) throw new InvalidOperationException(msg);
                log.LogWarning(msg);
                return;
            }
            var cilh = new ILHelper(ctorMethod.Body.GetILProcessor());
            MethodDefinition readyMethod = null;
            ILHelper rilh = null;

            Console.WriteLine("Retrieving top level class field attributes");
            var classAttrs = GetFieldAttributes(nodeClass, _spartanLibsAttributeTypeDef);
            if (classAttrs != null)
            {
                foreach (var ca in classAttrs)
                {
                    if (TryGetAttributeMethod(ca, nodeClass, "OnConstructor", out var onCtorMethodImp))
                    {
                        InsertInstructionsIntoExistingMethod(cilh.IL, ILHelper.Compose(
                            ComposeForCustomAttribute(ca, cilh),
                            cilh.PushThis(),
                            ComposeMemberReferenceInstruction(nodeClass, nodeClass, cilh),
                            cilh.CallMethodVirtual(onCtorMethodImp),
                            cilh.Nop()
                        ));
                    }

                    if (!TryGetAttributeMethod(ca, nodeClass, "OnReady", out var onReadyMethodImp))
                        continue;
                    readyMethod = GetOrCreateReadyMethod(nodeClass);
                    rilh = new ILHelper(readyMethod.Body.GetILProcessor());
                    InsertInstructionsIntoExistingMethod(rilh.IL, ILHelper.Compose(
                        ComposeForCustomAttribute(ca, rilh),
                        rilh.PushThis(),
                        ComposeMemberReferenceInstruction(nodeClass, nodeClass, rilh),
                        rilh.CallMethodVirtual(onReadyMethodImp),
                        rilh.Nop()
                    ));
                }
            }

            foreach (var memberDef in members)
            {
                var attrs = GetFieldAttributes(memberDef, _spartanLibsAttributeTypeDef);
                if (attrs == null || attrs.Count < 1)
                {
                    Console.WriteLine($"No member attributes found for '{memberDef.Name}'");
                    continue;
                }
                Console.WriteLine($"Member attribute{(attrs.Count > 1 ? "s" : "")} {string.Join(", ", attrs.Select(a => $"'{a.Constructor.DeclaringType.FullName}'"))} found for '{memberDef.Name}'");

                if (readyMethod == null) readyMethod = GetOrCreateReadyMethod(nodeClass);
                if (rilh == null) rilh = new ILHelper(readyMethod.Body.GetILProcessor());

                if (memberDef is PropertyDefinition propDef && propDef.SetMethod == null)
                {
                    Console.WriteLine("Member does not contain a setter property, creating one");
                    var setMethod = new MethodDefinition(
                        $"set_{propDef.Name}",
                        MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                        _targetModule.TypeSystem.Void);
                    setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None,
                        propDef.PropertyType));
                    nodeClass.Methods.Add(setMethod);
                    var setIlh = new ILHelper(setMethod.Body.GetILProcessor());

                    InsertInstructionsIntoNewMethod(setIlh.IL,
                        ILHelper.Compose(
                            setIlh.PushThis(),
                            setIlh.PushArgument(1),
                            setIlh.SetField(nodeClass.Fields.First(f => f.Name == $"<{propDef.Name}>k__BackingField")),
                            setIlh.Return()
                        ));

                    propDef.SetMethod = setMethod;
                }

                foreach (var ca in attrs)
                {
                    if (TryGetAttributeMethod(ca, nodeClass, "OnConstructor", out var onCtorMethodImp))
                    {
                        InsertInstructionsIntoExistingMethod(cilh.IL, ILHelper.Compose(
                            ComposeForCustomAttribute(ca, cilh),
                            cilh.PushThis(),
                            ComposeMemberReferenceInstruction(nodeClass, memberDef, cilh),
                            cilh.CallMethodVirtual(onCtorMethodImp),
                            cilh.Nop()
                        ));
                    }

                    if (!TryGetAttributeMethod(ca, nodeClass, "OnReady", out var onReadyMethodImp))
                        continue;
                    InsertInstructionsIntoExistingMethod(rilh.IL,
                ILHelper.Compose(
                    ComposeForCustomAttribute(ca, rilh),
                            rilh.PushThis(),
                            ComposeMemberReferenceInstruction(nodeClass, memberDef, rilh),
                            rilh.CallMethodVirtual(onReadyMethodImp),
                            rilh.Nop()));
                }
            }
        }

        // private void ProcessNodeLinks(TypeDefinition nodeClass)
        // {
        //     var members = new List<IMemberDefinition>();
        //     members.AddRange(nodeClass.Fields);
        //     members.AddRange(nodeClass.Properties);
        //
        //     MethodDefinition readyMethod = null;
        //
        //     foreach (var memberDef in members)
        //     {
        //         var nodeAttr = GetFieldAttribute(memberDef, _godotUtilsNodeAttrTypeDef);
        //         if (nodeAttr == null)
        //             continue;
        //
        //         var nodePath = (string) nodeAttr.ConstructorArguments[0].Value ?? memberDef.Name;
        //
        //         //Console.WriteLine($"found Get link: class - {nodeClass}; member - {memberDef}; nodePath - {nodePath}");
        //
        //         if (readyMethod == null) readyMethod = GetOrCreateReadyMethod(nodeClass);
        //
        //         var il = readyMethod.Body.GetILProcessor();
        //         var instructions = GenerateSetLinkFromPathInstructions(memberDef, nodePath, il);
        //         InsertInstructionsIntoExistingMethod(il, instructions);
        //     }
        // }

        // private IEnumerable<Instruction> GenerateSetLinkFromPathInstructions(
        //     IMemberDefinition fieldOrPropDef,
        //     string nodePath,
        //     ILProcessor il
        // )
        // {
        //     var c = new ILHelper(il);
        //
        //     TypeReference memberType;
        //     IEnumerable<Instruction> memberSetInstructions;
        //     IEnumerable<Instruction> memberGetInstructions;
        //
        //     switch (fieldOrPropDef)
        //     {
        //         case FieldDefinition field:
        //             memberType = field.FieldType;
        //             memberSetInstructions = c.SetField(field);
        //             memberGetInstructions = c.LoadField(field);
        //             break;
        //         case PropertyDefinition property:
        //             memberType = property.PropertyType;
        //             memberSetInstructions = c.SetProperty(property);
        //             memberGetInstructions = c.LoadProperty(property);
        //             break;
        //         default:
        //             throw new Exception($"member is {fieldOrPropDef}, but only can be field or prop");
        //     }
        //
        //     IEnumerable<Instruction> checkInstructions;
        //     if (_debugChecksEnabled)
        //     {
        //         var messagePrefix = $"Linking {fieldOrPropDef.DeclaringType.Name}::{fieldOrPropDef.Name} " +
        //                             $"to node path '{nodePath}': ";
        //         var nodeNotFoundMessage = messagePrefix + "Node not found";
        //         var incompatibleTypeMessage = messagePrefix + $"Type of node is incompatible with target";
        //
        //         checkInstructions = c.Branch(true,
        //             ILHelper.Compose(c.PushThis(), memberGetInstructions, c.IsNull()),
        //             c.Branch(true,
        //                 ILHelper.Compose(
        //                     c.PushThis(),
        //                     c.PushString(nodePath),
        //                     c.CallMethod(_nodePathConversionMethodDef),
        //                     c.CallMethod(_nodeGetNodeMethodDef),
        //                     c.IsNull()),
        //                 ILHelper.Compose(c.PushString(nodeNotFoundMessage), c.CallMethod(_godotPushErrorMethodDef)),
        //                 ILHelper.Compose(c.PushString(incompatibleTypeMessage), c.CallMethod(_godotPushErrorMethodDef))
        //             ));
        //     }
        //     else
        //         checkInstructions = new List<Instruction>();
        //
        //
        //     return ILHelper.Compose(
        //         c.PushThis(),
        //         c.PushThis(),
        //         c.PushString(nodePath),
        //         c.CallMethod(_nodePathConversionMethodDef),
        //         c.CallMethod(_nodeGetNodeMethodDef),
        //         c.IsInstance(memberType),
        //         memberSetInstructions,
        //         checkInstructions
        //     );
        // }

        // private void ProcessEditorLinks(TypeDefinition nodeClass)
        // {
        //     var members = new List<IMemberDefinition>();
        //     members.AddRange(nodeClass.Fields);
        //     members.AddRange(nodeClass.Properties);
        //
        //     MethodDefinition readyMethod = null;
        //
        //     foreach (var memberDef in members)
        //     {
        //         if (!HasBaseClass(GetFieldOrPropertyType(memberDef).Resolve(), _godotNodeTypeDef))
        //             continue;
        //
        //         var exportAttr = GetFieldAttribute(memberDef, _godotExportAttrTypeDef);
        //         if (exportAttr == null)
        //             continue;
        //
        //         var exportRenameAttr = GetFieldAttribute(memberDef, _godotUtilsExportRenameAttrTypeDef);
        //         var exportedName = (string) exportRenameAttr?.ConstructorArguments[0].Value ?? memberDef.Name;
        //
        //         //Console.WriteLine($"found Link: class - {nodeClass}; member - {memberDef}; exportedName - {exportedName}");
        //
        //         exportedName = SelectNonUsedNameForEditorLinkField(nodeClass, exportedName);
        //         if (exportedName == null)
        //         {
        //             Console.WriteLine(
        //                 $"cannot select non-used name for editor link: class - {nodeClass}; member - {memberDef}");
        //             continue;
        //         }
        //
        //         memberDef.CustomAttributes.Remove(exportAttr);
        //
        //         var exportHint = (string) exportAttr.ConstructorArguments[1].Value;
        //         if (exportHint == null)
        //         {
        //             var memberType = memberDef is FieldDefinition ? "field" : "property";
        //             exportHint = $"Link to {memberType} '{memberDef.Name}'";
        //         }
        //
        //         if (readyMethod == null) readyMethod = GetOrCreateReadyMethod(nodeClass);
        //
        //         var exportedField = InjectEditorLinkField(nodeClass, exportedName, exportHint);
        //
        //         var il = readyMethod.Body.GetILProcessor();
        //         var instructions = GenerateEditorLinkInstructions(memberDef, exportedField, exportedName, il);
        //         InsertInstructionsIntoExistingMethod(il, instructions);
        //     }
        // }

        // private static string SelectNonUsedNameForEditorLinkField(TypeDefinition nodeClass, string baseName)
        // {
        //     var selectedName = baseName;
        //     for (var attempt = 0; attempt < 100; attempt++)
        //     {
        //         if (FindMethodBySimpleName(nodeClass, selectedName) == null &&
        //             FindFieldBySimpleName(nodeClass, selectedName) == null &&
        //             FindPropertyBySimpleName(nodeClass, selectedName) == null)
        //         {
        //             return selectedName;
        //         }
        //
        //         selectedName += "_";
        //     }
        //
        //     return null;
        // }

        // private IEnumerable<Instruction> GenerateEditorLinkInstructions(
        //     IMemberDefinition fieldOrPropDef,
        //     FieldDefinition exportedFieldDef,
        //     string exportedName,
        //     ILProcessor il
        // )
        // {
        //     var c = new ILHelper(il);
        //
        //     TypeReference memberType;
        //     IEnumerable<Instruction> memberSetInstructions;
        //     IEnumerable<Instruction> memberGetInstructions;
        //
        //     switch (fieldOrPropDef)
        //     {
        //         case FieldDefinition field:
        //             memberType = field.FieldType;
        //             memberSetInstructions = c.SetField(field);
        //             memberGetInstructions = c.LoadField(field);
        //             break;
        //         case PropertyDefinition property:
        //             memberType = property.PropertyType;
        //             memberSetInstructions = c.SetProperty(property);
        //             memberGetInstructions = c.LoadProperty(property);
        //             break;
        //         default:
        //             throw new Exception($"member is {fieldOrPropDef}, but only can be field or prop");
        //     }
        //
        //     IEnumerable<Instruction> nodePathCheckInstructions;
        //     IEnumerable<Instruction> postCheckInstructions;
        //     if (_debugChecksEnabled)
        //     {
        //         var messagePrefix = $"Linking {fieldOrPropDef.DeclaringType.Name}::{fieldOrPropDef.Name} ";
        //         var nodePathNotSetMessage = messagePrefix + "Not set";
        //
        //         var printNotSetMessageInstructions =
        //             ILHelper.Compose(c.PushString(nodePathNotSetMessage), c.CallMethod(_godotPushErrorMethodDef));
        //
        //         nodePathCheckInstructions = c.Branch(false,
        //             ILHelper.Compose(c.PushThis(), c.LoadField(exportedFieldDef), c.IsNull()),
        //             c.Branch(true,
        //                 ILHelper.Compose(
        //                     c.PushThis(),
        //                     c.LoadField(exportedFieldDef),
        //                     c.CallMethod(_nodePathIsEmptyMethodDef)),
        //                 printNotSetMessageInstructions
        //             ),
        //             printNotSetMessageInstructions);
        //
        //
        //
        //         var nodeNotFoundMessage = messagePrefix + "Node not found";
        //         var incompatibleTypeMessage = messagePrefix + $"Type of node is incompatible with target";
        //
        //         postCheckInstructions = c.Branch(true,
        //             ILHelper.Compose(c.PushThis(), memberGetInstructions, c.IsNull()),
        //             c.Branch(true,
        //                 ILHelper.Compose(
        //                     c.PushThis(),
        //                     c.PushThis(),
        //                     c.LoadField(exportedFieldDef),
        //                     c.CallMethod(_nodeGetNodeMethodDef),
        //                     c.IsNull()),
        //                 ILHelper.Compose(c.PushString(nodeNotFoundMessage), c.CallMethod(_godotPushErrorMethodDef)),
        //                 ILHelper.Compose(c.PushString(incompatibleTypeMessage), c.CallMethod(_godotPushErrorMethodDef))
        //             ));
        //     }
        //     else
        //     {
        //         nodePathCheckInstructions = new List<Instruction>();
        //         postCheckInstructions = new List<Instruction>();
        //     }
        //
        //     return ILHelper.Compose(
        //         nodePathCheckInstructions,
        //         c.PushThis(),
        //         c.PushThis(),
        //         c.PushThis(),
        //         c.LoadField(exportedFieldDef),
        //         c.CallMethod(_nodeGetNodeMethodDef),
        //         c.IsInstance(memberType),
        //         memberSetInstructions,
        //         postCheckInstructions
        //     );
        // }

        // private FieldDefinition InjectEditorLinkField(TypeDefinition nodeClass, string exportedName, string exportHint)
        // {
        //     var exportAttr =
        //         new CustomAttribute(_targetModule.ImportReference(_exportAttrCtorDef))
        //         {
        //             ConstructorArguments =
        //             {
        //                 new CustomAttributeArgument(_targetModule.ImportReference(_godotPropertyHintTypeDef), 0),
        //                 new CustomAttributeArgument(_targetModule.TypeSystem.String, exportHint)
        //             }
        //         };
        //
        //     var exportedField = new FieldDefinition(exportedName, FieldAttributes.Public,
        //         _targetModule.ImportReference(_godotNodePathTypeDef))
        //     {
        //         CustomAttributes = {exportAttr}
        //     };
        //
        //     nodeClass.Fields.Add(exportedField);
        //     return exportedField;
        // }

        private static void InsertInstructionsIntoExistingMethod(ILProcessor il, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions.Reverse())
            {
                var first = il.Body.Instructions[0];
                il.InsertBefore(first, instruction);
            }
        }

        private static void InsertInstructionsIntoNewMethod(ILProcessor il, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                il.Append(instruction);
        }

        public void Inject(TaskLoggingHelper log)
        {
            ReadTargetAssembly();

            // var canProcessNodeLinks = _godotUtilsNodeAttrTypeDef != null;
            // var canProcessAutoloadLinks = _godotUtilsAutoloadAttrTypeDef != null;
            // var canProcessEditorLinks = _godotExportAttrTypeDef != null;

            foreach (var nodeClass in GetAllNodeClasses())
            {
                // if (canProcessNodeLinks)
                //     ProcessNodeLinks(nodeClass);
                // if (canProcessEditorLinks)
                //     ProcessEditorLinks(nodeClass);
                // if (canProcessAutoloadLinks)
                //     ProcessAutoloadLinks(nodeClass);
                ProcessAttributes(nodeClass, log);
            }

            var writerParameters = new WriterParameters
            {
                WriteSymbols = !_configuration.Contains("Release")
            };

            _targetModule.Write(writerParameters);
        }
    }
}