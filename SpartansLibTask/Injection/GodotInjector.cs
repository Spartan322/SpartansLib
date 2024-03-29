using System.Reflection.Emit;
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
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Codes = System.Reflection.Emit.OpCodes;

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

        private Assembly _spartansLibAssembly;
        private Assembly _godotSharpAssembly;

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
            _spartansLibAssembly = Assembly.LoadFrom(_spartansLibModuleAssemblyPath);
            _godotSharpAssembly = Assembly.LoadFrom(_godotMainAssemblyDir + "GodotSharp.dll");

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

            var baseReady = GetBaseMethod(methodDef, nodeClass.BaseType.Resolve());

            var il = methodDef.Body.GetILProcessor();
            if (baseReady != null)
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, _targetModule.ImportReference(baseReady));
            }
            il.Emit(OpCodes.Ret);

            nodeClass.Methods.Add(methodDef);

            return methodDef;
        }

        private MethodDefinition GetBaseMethod(MethodDefinition method, TypeDefinition baseType = null)
        {
            if (baseType == null)
                baseType = method.DeclaringType.BaseType.Resolve();
            MethodDefinition result = null;
            while (baseType.FullName != "System.Object")
            {
                result = baseType.Methods.FirstOrDefault(m => m.Name == method.Name && m.Parameters.SequenceEqual(method.Parameters));
                if (result != null) break;
                baseType = baseType.BaseType.Resolve();
            }
            return result;
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

        private MethodReference ImportMethodFor(Type type, string name, params Type[] argTypes)
            => _targetModule.ImportReference(type.GetMethod(
                name,
                BindingFlags.Public | BindingFlags.Instance,
                null,
                argTypes,
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

        private delegate Attribute AttributeInstanceFactory(object[] args);
        private Dictionary<MethodReference, AttributeInstanceFactory> _customAttributeFactories
            = new Dictionary<MethodReference, AttributeInstanceFactory>();
        private AttributeInstanceFactory GetOrCreateCustomAttributeObjectFactory(CustomAttribute attribute)
        {
            if(_customAttributeFactories.TryGetValue(attribute.Constructor, out var factory))
                return factory;

            var attributeType = _spartansLibAssembly.GetType(attribute.AttributeType.FullName);
            var method = new DynamicMethod("CreateInstance", attributeType, new Type[] { typeof(object[]) });
            ConstructorInfo ctorInfo;
            int index;
            var gen = method.GetILGenerator();
            if(attribute.Constructor.Parameters.Count > 0)
            {
                var argTypes = new Type[attribute.Constructor.Parameters.Count];
                index = 0;
                foreach (var caArg in attribute.Constructor.Parameters)
                    argTypes[index] = GenOpCodeAndArgTypeFor(gen, caArg.ParameterType.FullName, index++);

                ctorInfo = attributeType.GetConstructor(argTypes);
            }
            else
                ctorInfo = attributeType.GetConstructor(Type.EmptyTypes);

            gen.Emit(Codes.Newobj, ctorInfo);

            index = 0;
            var lastIndex = attribute.Fields.Count + attribute.Properties.Count;
            var constructArgTypes = new Type[
                + attribute.Fields.Count
                + attribute.Properties.Count];
            foreach (var field in attributeType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance))
            {
                GenOpCodeAndArgTypeFor(gen, field.FieldType.FullName, index++);
                gen.Emit(Codes.Stfld, attributeType.GetField(field.Name));
                if(index < lastIndex) gen.Emit(Codes.Dup);
            }
            foreach (var property in attributeType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance))
            {
                GenOpCodeAndArgTypeFor(gen, property.PropertyType.FullName, index++);
                var reflectedProperty = attributeType.GetProperty(property.Name);
                if(reflectedProperty.CanWrite)
                    gen.Emit(Codes.Callvirt, reflectedProperty.SetMethod);
                else
                    gen.Emit(Codes.Stfld, attributeType.GetField($"<{reflectedProperty.Name}>k__BackingField"));
                if(index < lastIndex) gen.Emit(Codes.Dup);
            }

            gen.Emit(Codes.Ret);
            return _customAttributeFactories[attribute.Constructor] = (AttributeInstanceFactory)method.CreateDelegate(typeof(AttributeInstanceFactory));
        }

        private Type _GetArgType(string fullname)
        {
            switch(fullname)
            {
                case "System."+nameof(Boolean): return typeof(Boolean);
                case "System."+nameof(String): return typeof(String);
                case "System."+nameof(Int16): return typeof(Int16);
                case "System."+nameof(Int32): return typeof(Int32);
                case "System."+nameof(Int64): return typeof(Int64);
                case "System."+nameof(Char): return typeof(Char);
                case "System."+nameof(Byte): return typeof(Byte);
                case "System."+nameof(Double): return typeof(Double);
                case "System."+nameof(Single): return typeof(Single);
                case "System."+nameof(Type): return typeof(Type);
                case "System."+nameof(Object): return typeof(Object);
                default: // Only supposed to be possible with enums and arrays
                    if(fullname.EndsWith("[]")) // Is Array
                        return _GetArgType(fullname.Remove(fullname.Length-2, 2)).MakeArrayType();
                    return _spartansLibAssembly.GetType(fullname);
            }
        }

        private Type GenOpCodeAndArgTypeFor(
            ILGenerator gen,
            string argTypeFullName,
            int index
            //out Type argType
        )
        {
            Type argType;
            gen.Emit(Codes.Ldarg_0); // args
            switch(index)
            {
                case 0: gen.Emit(Codes.Ldc_I4_0); break;
                case 1: gen.Emit(Codes.Ldc_I4_1); break;
                case 2: gen.Emit(Codes.Ldc_I4_2); break;
                case 3: gen.Emit(Codes.Ldc_I4_3); break;
                case 4: gen.Emit(Codes.Ldc_I4_4); break;
                case 5: gen.Emit(Codes.Ldc_I4_5); break;
                case 6: gen.Emit(Codes.Ldc_I4_6); break;
                case 7: gen.Emit(Codes.Ldc_I4_7); break;
                case 8: gen.Emit(Codes.Ldc_I4_8); break;
                default: gen.Emit(Codes.Ldc_I4, index); break;
            }
            gen.Emit(Codes.Ldelem_Ref);
            switch(argTypeFullName)
            {
                case "System."+nameof(Boolean):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Boolean));
                    break;
                case "System."+nameof(String):
                    gen.Emit(Codes.Castclass, argType = typeof(String));
                    break;
                case "System."+nameof(Int16):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Int16));
                    break;
                case "System."+nameof(Int32):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Int32));
                    break;
                case "System."+nameof(Int64):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Int64));
                    break;
                case "System."+nameof(Char):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Char));
                    break;
                case "System."+nameof(Byte):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Byte));
                    break;
                case "System."+nameof(Double):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Double));
                    break;
                case "System."+nameof(Single):
                    gen.Emit(Codes.Unbox_Any, argType = typeof(Single));
                    break;
                case "System."+nameof(Type):
                    gen.Emit(Codes.Castclass, argType = typeof(Type));
                    break;
                case "System."+nameof(Object): argType = typeof(Object); break;
                default: // Only supposed to be possible with enums and arrays
                    if(argTypeFullName.EndsWith("[]")) // Is Array
                    {
                        gen.Emit(Codes.Castclass, argType = _GetArgType(argTypeFullName).MakeArrayType());
                        break;
                    }
                    gen.Emit(Codes.Unbox_Any, typeof(Int32));
                    argType = Type.GetType(argTypeFullName) ?? _godotSharpAssembly.GetType(argTypeFullName) ?? _spartansLibAssembly.GetType(argTypeFullName);
                    break;
            }
            if(argType == null) throw new Exception($"{argTypeFullName} threw a null");
            return argType;
        }

        private Attribute CreateInjectedCustomAttributeObject(CustomAttribute attribute)
        {
            if(!CanBeAssignedTo(
                attribute.AttributeType.Resolve(),
                "SpartansLib.Attributes.SpartansLibAttribute"))
                return null;
            var factory = GetOrCreateCustomAttributeObjectFactory(attribute);
            if(!attribute.HasConstructorArguments && !attribute.HasFields && !attribute.HasProperties)
                return factory(Array.Empty<object>());
            var args = new object[
                attribute.ConstructorArguments.Count
                    + attribute.Fields.Count
                    + attribute.Properties.Count];
            var index = 0;
            foreach(var arg in attribute.ConstructorArguments)
                args[index++] = arg.Value;
            foreach(var arg in attribute.Fields)
                args[index++] = arg.Argument.Value;
            foreach(var arg in attribute.Properties)
                args[index++] = arg.Argument.Value;
            return factory(args);

            IEnumerable<TypeReference> GetHierachy(TypeDefinition type)
            {
                TypeReference next;
                do yield return next = type.BaseType;
                while(next != type.Module.TypeSystem.Object);
            }

            bool CanBeAssignedTo(TypeDefinition derived, string baseFullName)
                => GetHierachy(derived).Any(type => type.FullName == baseFullName);
        }

        private delegate void Invoker(Attribute attrib, params object[] args);
        private Dictionary<string, Invoker> _namesToInvokers = new Dictionary<string, Invoker>();
        private void InvokeModifierMethod(Attribute attrib, string methodName, params object[] args)
        {
            attrib.GetType().InvokeMember(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, attrib, args);
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
                    typeof(string), typeof(BindingFlags));
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
                        new[] {ilh.IL.Create(OpCodes.Ldc_I4, 52)},
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
                    var attribObj = CreateInjectedCustomAttributeObject(ca);
                    if(attribObj == null) continue;

                    if (TryGetAttributeMethod(ca, nodeClass, "OnConstructor", out var onCtorMethodImp))
                    {
                        InvokeModifierMethod(attribObj, onCtorMethodImp.Name, cilh.IL, nodeClass);
                        // InsertInstructionsIntoExistingMethod(cilh.IL, ILHelper.Compose(
                        //     ComposeForCustomAttribute(ca, cilh),
                        //     cilh.PushThis(),
                        //     ComposeMemberReferenceInstruction(nodeClass, nodeClass, cilh),
                        //     cilh.CallMethodVirtual(onCtorMethodImp),
                        //     cilh.Nop()
                        // ));
                    }

                    if (!TryGetAttributeMethod(ca, nodeClass, "OnReady", out var onReadyMethodImp))
                        continue;
                    readyMethod = GetOrCreateReadyMethod(nodeClass);
                    rilh = new ILHelper(readyMethod.Body.GetILProcessor());
                    InvokeModifierMethod(attribObj, onReadyMethodImp.Name, rilh.IL, nodeClass);
                    // InsertInstructionsIntoExistingMethod(rilh.IL, ILHelper.Compose(
                    //     ComposeForCustomAttribute(ca, rilh),
                    //     rilh.PushThis(),
                    //     ComposeMemberReferenceInstruction(nodeClass, nodeClass, rilh),
                    //     rilh.CallMethodVirtual(onReadyMethodImp),
                    //     rilh.Nop()
                    // ), true);
                }
            }

            foreach (var memberDef in members)
            {
                Console.WriteLine(memberDef.FullName);
                var attrs = GetFieldAttributes(memberDef, _spartanLibsAttributeTypeDef);
                if (attrs == null || attrs.Count < 1)
                {
                    Console.WriteLine($"No member attributes found for '{memberDef.Name}'.");
                    continue;
                }
                Console.WriteLine($"Member attribute{(attrs.Count > 1 ? "s" : "")} {string.Join(", ", attrs.Select(a => $"'{a.Constructor.DeclaringType.FullName}'"))} found for '{memberDef.Name}'.");

                if (readyMethod == null) readyMethod = GetOrCreateReadyMethod(nodeClass);
                if (rilh == null) rilh = new ILHelper(readyMethod.Body.GetILProcessor());

                if (memberDef is PropertyDefinition propDef && propDef.SetMethod == null)
                {
                    Console.WriteLine("Member does not contain a setter property, creating one.");
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
                    var attribObj = CreateInjectedCustomAttributeObject(ca);
                    if(attribObj == null) continue;

                    if (TryGetAttributeMethod(ca, nodeClass, "OnConstructor", out var onCtorMethodImp))
                    {
                        InvokeModifierMethod(attribObj, onCtorMethodImp.Name, cilh.IL, memberDef);
                        // InsertInstructionsIntoExistingMethod(cilh.IL, ILHelper.Compose(
                        //     ComposeForCustomAttribute(ca, cilh),
                        //     cilh.PushThis(),
                        //     ComposeMemberReferenceInstruction(nodeClass, memberDef, cilh),
                        //     cilh.CallMethodVirtual(onCtorMethodImp),
                        //     cilh.Nop()
                        // ));
                    }

                    if (!TryGetAttributeMethod(ca, nodeClass, "OnReady", out var onReadyMethodImp))
                        continue;
                    InvokeModifierMethod(attribObj, onReadyMethodImp.Name, rilh.IL, memberDef);
                //     InsertInstructionsIntoExistingMethod(rilh.IL,
                // ILHelper.Compose(
                //     ComposeForCustomAttribute(ca, rilh),
                //             rilh.PushThis(),
                //             ComposeMemberReferenceInstruction(nodeClass, memberDef, rilh),
                //             rilh.CallMethodVirtual(onReadyMethodImp),
                //             rilh.Nop()), true);
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

        private void InsertInstructionsIntoExistingMethod(ILProcessor il, IEnumerable<Instruction> instructions, bool checkForBaseCall = false)
        {
            // In the general case if checkForBaseCall is true, if the first instructions are a base call, it will inject after the base call instead
            var offset = checkForBaseCall
                    && il.Body.Instructions[1].OpCode == OpCodes.Ldarg_0
                    && il.Body.Instructions[2].OpCode == OpCodes.Call
                    && il.Body.Instructions[2].Operand.ToString() == GetBaseMethod(il.Body.Method).ToString() ? 3 : 0;
            while (il.Body.Instructions.Count < 3)
                il.Emit(OpCodes.Nop);
            foreach (var instruction in instructions.Reverse())
            {
                var first = il.Body.Instructions[offset];
                il.InsertBefore(first, instruction);
            }
        }

        private void InsertInstructionsIntoNewMethod(ILProcessor il, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                il.Append(instruction);
        }

        private TaskLoggingHelper logger;
        public void Inject(TaskLoggingHelper log)
        {
            logger = log;
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