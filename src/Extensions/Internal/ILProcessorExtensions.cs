using System.ComponentModel;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace SpartansLib.Extensions.Internal
{
    public static class ILProcessorExtensions
    {
        public static IEnumerable<Instruction> Compose(this ILProcessor il, params IEnumerable<Instruction>[] instructionsArray)
        {
            var composed = new List<Instruction>();
            foreach (var instructions in instructionsArray)
            {
                if (instructions == null)
                {
                    Console.WriteLine("Warning: null instruction found");
                    continue;
                }
                composed.AddRange(instructions);
            }

            return composed;
        }

        public static IEnumerable<Instruction> ComposeNoWarn(this ILProcessor il, params IEnumerable<Instruction>[] instructionsArray)
        {
            var composed = new List<Instruction>();
            foreach (var instructions in instructionsArray)
            {
                if (instructions == null)
                    continue;
                composed.AddRange(instructions);
            }

            return composed;
        }

        public static MethodReference GetAndValidateMethod(
            this TypeReference declaringType,
            string name,
            Type[] argTypes = null,
            TypeReference[] genericTypes = null,
            BindingFlags? flags = null
        )
        {
            if(argTypes?.Length == 0) argTypes = null;

            var declaringTypeDef = declaringType.Resolve();
            var method = declaringTypeDef.Methods.FirstOrDefault(m =>
            {
                if(genericTypes != null)
                {
                    if(!m.ContainsGenericParameter)
                        return false;
                    var genericArgsCount = m.GenericParameters
                        .Count(a => a.IsGenericParameter);
                    if((genericTypes?.Length ?? 0) != genericArgsCount)
                        return false;
                }

                if(argTypes != null)
                {
                    if(argTypes.Length != m.Parameters.Count) return false;
                    if(m.Parameters.Any(p => p.ParameterType.IsGenericParameter))
                    {
                        for(var index = 0; index < m.Parameters.Count; index++)
                        {
                            var param = m.Parameters[index];
                            if(param.ParameterType.IsGenericParameter) continue;
                            if(param.ParameterType.FullName != argTypes[index].FullName)
                                return false;
                        }
                    }
                    else if(!m.Parameters.Select(p => p.ParameterType.FullName)
                        .SequenceEqual(argTypes.Select(t => t.FullName))
                    )
                        return false;
                }

                return m.Name == name;
            });

            if(method == null)
                throw new ArgumentException($@"Could not find method '{name}'{
                    (argTypes != null || genericTypes != null ? " with " : "")
                }{
                    (argTypes != null ? $@"{nameof(argTypes)} of ({
                    string.Join(", ", argTypes.Select(t => t.FullName))
                })" : "")}{
                    (argTypes != null && genericTypes != null ? " and " : "")
                }{
                    (genericTypes != null ? $@"{nameof(genericTypes)} of ({
                    string.Join(", ", genericTypes.Select(t => t.FullName))
                })" : "")} in {declaringType.FullName}", nameof(name));

            if(genericTypes != null)
            {
                var index = 0;
                foreach(var genType in genericTypes)
                    method.GenericParameters[index++] = new GenericParameter(genType);
            }

            return method;
        }

        public static MethodReference GetAndValidateMethod(
            this TypeReference declaringType,
            string name,
            TypeReference[] argTypes = null,
            TypeReference[] genericTypes = null,
            BindingFlags? flags = null
        )
        {
            if(argTypes?.Length == 0) argTypes = null;

            var declaringTypeDef = declaringType.Resolve();
            var method = declaringTypeDef.Methods.FirstOrDefault(m =>
            {
                if(genericTypes != null)
                {
                    if(!m.ContainsGenericParameter)
                        return false;
                    var genericArgsCount = m.GenericParameters
                        .Count(a => a.IsGenericParameter);
                    if((genericTypes?.Length ?? 0) != genericArgsCount)
                        return false;
                }

                if(argTypes != null)
                {
                    if(argTypes.Length != m.Parameters.Count) return false;
                    if(m.Parameters.Any(p => p.ParameterType.IsGenericParameter))
                    {
                        for(var index = 0; index < m.Parameters.Count; index++)
                        {
                            var param = m.Parameters[index];
                            if(param.ParameterType.IsGenericParameter) continue;
                            if(param.ParameterType.FullName != argTypes[index].FullName)
                                return false;
                        }
                    }
                    else if(!m.Parameters.Select(p => p.ParameterType.FullName)
                        .SequenceEqual(argTypes.Select(t => t.FullName))
                    )
                        return false;
                }

                return m.Name == name;
            });

            if(method == null)
                throw new ArgumentException($@"Could not find method '{name}'{
                    (argTypes != null || genericTypes != null ? " with " : "")
                }{
                    (argTypes != null ? $@"{nameof(argTypes)} of ({
                    string.Join(", ", argTypes.Select(t => t.FullName))
                })" : "")}{
                    (argTypes != null && genericTypes != null ? " and " : "")
                }{
                    (genericTypes != null ? $@"{nameof(genericTypes)} of ({
                    string.Join(", ", genericTypes.Select(t => t.FullName))
                })" : "")} in {declaringType.FullName}", nameof(name));

            if(genericTypes != null)
            {
                var index = 0;
                foreach(var genType in genericTypes)
                    method.GenericParameters[index++] = new GenericParameter(genType);
            }

            return method;
        }

        public static MethodInfo GetAndValidateMethod(
            this Type declaringType,
            string name,
            Type[] argTypes = null,
            Type[] genericTypes = null,
            BindingFlags? flags = null
        )
        {
            if(argTypes?.Length == 0) argTypes = null;

            var methods = flags != null
                ? declaringType.GetMethods(flags.Value)
                : declaringType.GetMethods();

            var method = methods.FirstOrDefault(m =>
            {
                if(genericTypes != null)
                {
                    if(!m.ContainsGenericParameters)
                        return false;
                    var genericArgsCount = m.GetGenericArguments()
                        .Count(a => a.IsGenericParameter);
                    if((genericTypes?.Length ?? 0) != genericArgsCount)
                        return false;
                }

                if(argTypes != null)
                {
                    var parameters = m.GetParameters();
                    if(argTypes.Length != parameters.Length) return false;
                    if(parameters.Any(p => p.ParameterType.IsGenericParameter))
                    {
                        for(var index = 0; index < parameters.Length; index++)
                        {
                            var param = parameters[index];
                            if(param.ParameterType.IsGenericParameter) continue;
                            if(param.ParameterType.FullName != argTypes[index].FullName)
                                return false;
                        }
                    }
                    else if(!parameters.Select(p => p.ParameterType.FullName)
                        .SequenceEqual(argTypes.Select(t => t.FullName))
                    )
                        return false;
                }

                return m.Name == name;
            });

            if(method == null)
                throw new ArgumentException($@"Could not find method '{name}'{
                    (argTypes != null || genericTypes != null ? " with " : "")
                }{
                    (argTypes != null ? $@"{nameof(argTypes)} of ({
                    string.Join(", ", argTypes.Select(t => t.FullName))
                })" : "")}{
                    (argTypes != null && genericTypes != null ? " and " : "")
                }{
                    (genericTypes != null ? $@"{nameof(genericTypes)} of ({
                    string.Join(", ", genericTypes.Select(t => t.FullName))
                })" : "")} in {declaringType.FullName}", nameof(name));

            return genericTypes != null
                ? method.MakeGenericMethod(genericTypes)
                : method;
        }

        public static MethodInfo GetAndValidateMethod(
            this Type declaringType,
            string name,
            params Type[] argTypes
        )
            => declaringType.GetAndValidateMethod(name, argTypes, null);

        public static MethodInfo GetAndValidateDeclaredMethod(
            this Type declaringType,
            string name,
            Type[] argTypes = null,
            Type[] genericTypes = null
        )
            => declaringType.GetAndValidateMethod(name, argTypes, genericTypes, BindingFlags.DeclaredOnly | BindingFlags.Public);

        public static MethodInfo GetAndValidateInstanceMethod(
            this Type declaringType,
            string name,
            Type[] argTypes = null,
            Type[] genericTypes = null
        )
            => declaringType.GetAndValidateMethod(name, argTypes, genericTypes, BindingFlags.Instance | BindingFlags.Public);

        public static MethodInfo GetAndValidateStaticMethod(
            this Type declaringType,
            string name,
            Type[] argTypes = null,
            Type[] genericTypes = null
        )
            => declaringType.GetAndValidateMethod(name, argTypes, genericTypes, BindingFlags.Static | BindingFlags.Public);

        public static IEnumerable<Instruction> Nop(this ILProcessor IL)
            => new[] {IL.Create(OpCodes.Nop)};

        public static IEnumerable<Instruction> PushThis(this ILProcessor IL)
            => new[] {IL.Create(OpCodes.Ldarg_0)};

        public static IEnumerable<Instruction> PushString(this ILProcessor IL, string arg)
            => new[] {IL.Create(OpCodes.Ldstr, arg)};

        public static IEnumerable<Instruction> PushIntConstant(this ILProcessor IL, int constant)
        {
            switch (constant)
            {
                case -1:
                    return new[] {IL.Create(OpCodes.Ldc_I4_M1)};
                case 0:
                    return new[] {IL.Create(OpCodes.Ldc_I4_0)};
                case 1:
                    return new[] {IL.Create(OpCodes.Ldc_I4_1)};
                case 2:
                    return new[] {IL.Create(OpCodes.Ldc_I4_2)};
                case 3:
                    return new[] {IL.Create(OpCodes.Ldc_I4_3)};
                case 4:
                    return new[] {IL.Create(OpCodes.Ldc_I4_4)};
                case 5:
                    return new[] {IL.Create(OpCodes.Ldc_I4_5)};
                case 6:
                    return new[] {IL.Create(OpCodes.Ldc_I4_6)};
                case 7:
                    return new[] {IL.Create(OpCodes.Ldc_I4_7)};
                case 8:
                    return new[] {IL.Create(OpCodes.Ldc_I4_8)};
            }

            return new[] {IL.Create(OpCodes.Ldc_I4, constant)};
        }

        public static IEnumerable<Instruction> PushIntConstant(this ILProcessor IL, uint constant)
        {
            switch (constant)
            {
                case 0:
                    return new[] {IL.Create(OpCodes.Ldc_I4_0)};
                case 1:
                    return new[] {IL.Create(OpCodes.Ldc_I4_1)};
                case 2:
                    return new[] {IL.Create(OpCodes.Ldc_I4_2)};
                case 3:
                    return new[] {IL.Create(OpCodes.Ldc_I4_3)};
                case 4:
                    return new[] {IL.Create(OpCodes.Ldc_I4_4)};
                case 5:
                    return new[] {IL.Create(OpCodes.Ldc_I4_5)};
                case 6:
                    return new[] {IL.Create(OpCodes.Ldc_I4_6)};
                case 7:
                    return new[] {IL.Create(OpCodes.Ldc_I4_7)};
                case 8:
                    return new[] {IL.Create(OpCodes.Ldc_I4_8)};
            }

            return new[] {IL.Create(OpCodes.Ldc_I4, constant)};
        }

        public static IEnumerable<Instruction> PushArgument(this ILProcessor IL, int index)
        {
            switch (index)
            {
                case 0:
                    return IL.PushThis();
                case 1:
                    return new[] {IL.Create(OpCodes.Ldarg_1)};
                case 2:
                    return new[] { IL.Create(OpCodes.Ldarg_2) };
                case 3:
                    return new[] {IL.Create(OpCodes.Ldarg_3)};
            }

            return new[] { IL.Create(OpCodes.Ldarg, index) };
        }

        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, MethodReference method)
            => new[] {IL.Create(OpCodes.Call, IL.Body.Method.Module.ImportReference(method))};

        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, TypeReference declaringType, string methodName, params TypeReference[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateMethod(methodName, argTypes));
        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, TypeReference declaringType, string methodName, params Type[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateMethod(methodName, argTypes));

        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, MethodBase method)
            => IL.CallMethod(IL.Body.Method.Module.ImportReference(method));
        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, Type declaringType, string methodName, params Type[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateMethod(methodName, argTypes));
        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, Type declaringType, string methodName, Type[] genericTypes, params Type[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateMethod(methodName, argTypes, genericTypes));

        public static IEnumerable<Instruction> CallStaticMethod(this ILProcessor IL, Type declaringType, string methodName, params Type[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateMethod(methodName, argTypes));
        public static IEnumerable<Instruction> CallStaticMethod(this ILProcessor IL, Type declaringType, string methodName, Type[] genericTypes, params Type[] argTypes)
            => IL.CallMethod(declaringType.GetAndValidateStaticMethod(methodName, argTypes, genericTypes));

        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, Type declaringType, string methodName, TypeReference[] genericTypes, params Type[] argTypes)
        {
            var methodRef = IL.Body.Method.Module.ImportReference(declaringType.GetAndValidateMethod(methodName, argTypes));
            if(genericTypes.Length != methodRef.GenericParameters.Count)
                throw new ArgumentException($"'{methodRef.FullName}' can not support {nameof(genericTypes)} ({string.Join(", ", genericTypes.Select(t => t.FullName))})", nameof(genericTypes));
            methodRef = new GenericInstanceMethod(methodRef);
            for(var index = 0; index < genericTypes.Length; index++)
                ((GenericInstanceMethod)methodRef).GenericArguments.Add(IL.Body.Method.Module.ImportReference(genericTypes[index]));
            return IL.CallMethod(methodRef);
        }

        public static IEnumerable<Instruction> CallMethod(this ILProcessor IL, TypeReference declaringType, string methodName, TypeReference[] genericTypes, params TypeReference[] argTypes)
        {
            var methodRef = declaringType.GetAndValidateMethod(methodName, argTypes, genericTypes);
            // if(genericTypes.Length != methodRef.GenericParameters.Count)
            //     throw new ArgumentException($"'{methodRef.FullName}' can not support {nameof(genericTypes)} ({string.Join(", ", genericTypes.Select(t => t.FullName))})", nameof(genericTypes));
            // for(var index = 0; index < methodRef.GenericParameters.Count; index++)
            //     methodRef.GenericParameters[index] = new GenericParameter(genericTypes[index]);
            return IL.CallMethod(methodRef);
        }

        public static IEnumerable<Instruction> CallMethodVirtual(this ILProcessor IL, MethodReference method)
            => new[] {IL.Create(OpCodes.Callvirt, IL.Body.Method.Module.ImportReference(method))};

        public static IEnumerable<Instruction> CallMethodVirtual(this ILProcessor IL, MethodBase method)
            => IL.CallMethodVirtual(IL.Body.Method.Module.ImportReference(method));
        public static IEnumerable<Instruction> CallMethodVirtual(this ILProcessor IL, Type declaringType, string methodName, params Type[] argTypes)
            => IL.CallMethodVirtual(declaringType.GetAndValidateMethod(methodName, argTypes));
        public static IEnumerable<Instruction> CallMethodVirtual(this ILProcessor IL, Type declaringType, string methodName, Type[] genericTypes, params Type[] argTypes)
            => IL.CallMethodVirtual(declaringType.GetAndValidateMethod(methodName, argTypes).MakeGenericMethod(genericTypes));

        public static IEnumerable<Instruction> CallMethodVirtual(this ILProcessor IL, Type declaringType, string methodName, TypeReference[] genericTypes, params Type[] argTypes)
        {
            var methodRef = IL.Body.Method.Module.ImportReference(declaringType.GetAndValidateMethod(methodName, argTypes));
            if(genericTypes.Length != methodRef.GenericParameters.Count)
                throw new ArgumentException($"'{methodRef.FullName}' can not support {nameof(genericTypes)} ({string.Join(", ", genericTypes.Select(t => t.FullName))})", nameof(genericTypes));
            for(var index = 0; index < methodRef.GenericParameters.Count; index++)
                methodRef.GenericParameters[index] = new GenericParameter(genericTypes[index]);
            return IL.CallMethodVirtual(methodRef);
        }

        public static IEnumerable<Instruction> NewObj(this ILProcessor IL, MethodReference ctorMethod)
            => new[] { IL.Create(OpCodes.Newobj, IL.Body.Method.Module.ImportReference(ctorMethod)) };
        public static IEnumerable<Instruction> NewObj(this ILProcessor IL, MethodBase ctorMethod)
            => IL.NewObj(IL.Body.Method.Module.ImportReference(ctorMethod));
        public static IEnumerable<Instruction> NewObj(this ILProcessor IL, Type createdType, params Type[] argTypes)
            => IL.NewObj(createdType.GetConstructor(argTypes));

        public static IEnumerable<Instruction> Dup(this ILProcessor IL)
            => new[] {IL.Create(OpCodes.Dup)};

        public static IEnumerable<Instruction> LoadNull(this ILProcessor IL)
            => new[] { IL.Create(OpCodes.Ldnull) };

        public static IEnumerable<Instruction> LoadMemberVariable(this ILProcessor IL, MemberReference member)
        {
            if(member is FieldReference fr)
                return IL.LoadField(fr.Resolve());
            if(member is PropertyReference pr)
                return IL.LoadProperty(pr.Resolve());
            return IL.CallMethod((member as MethodReference)?.Resolve() ?? throw new ArgumentException());
        }

        public static IEnumerable<Instruction> SetMemberVariable(this ILProcessor IL, MemberReference member)
        {
            if(member is FieldReference fr)
                return IL.SetField(fr.Resolve());
            if(member is PropertyReference pr)
                return IL.SetProperty(pr.Resolve());
            return IL.CallMethod((member as MethodReference)?.Resolve() ?? throw new ArgumentException());
        }

        public static IEnumerable<Instruction> LoadType(this ILProcessor IL, TypeReference type)
            => new[] {IL.Create(OpCodes.Ldtoken, IL.Body.Method.Module.ImportReference(type))};

        public static IEnumerable<Instruction> LoadFieldRef(this ILProcessor IL, FieldReference field)
            => new[] {IL.Create(OpCodes.Ldtoken, IL.Body.Method.Module.ImportReference(field))};

        public static IEnumerable<Instruction> LoadMethodRef(this ILProcessor IL, MethodReference method)
            => new[] {IL.Create(OpCodes.Ldtoken, IL.Body.Method.Module.ImportReference(method))};

        public static IEnumerable<Instruction> LoadField(this ILProcessor IL, FieldDefinition field)
            => new[] {IL.Create(OpCodes.Ldfld, IL.Body.Method.Module.ImportReference(field))};

        public static IEnumerable<Instruction> SetField(this ILProcessor IL, FieldDefinition field)
            => new[] {IL.Create(OpCodes.Stfld, IL.Body.Method.Module.ImportReference(field))};

        public static IEnumerable<Instruction> LoadProperty(this ILProcessor IL, PropertyDefinition property)
            => IL.CallMethod(property.GetMethod);
        public static IEnumerable<Instruction> LoadProperty(this ILProcessor IL, PropertyInfo property)
            => IL.CallMethod(property.GetGetMethod());
        public static IEnumerable<Instruction> LoadProperty(this ILProcessor IL, Type type, string propertyName)
            => IL.LoadProperty(type.GetProperty(propertyName));

        public static IEnumerable<Instruction> SetProperty(this ILProcessor IL, PropertyDefinition property)
            => IL.CallMethod(property.SetMethod);

        public static IEnumerable<Instruction> Return(this ILProcessor IL)
            => new[] { IL.Create(OpCodes.Ret) };

        public static IEnumerable<Instruction> IsInstance(this ILProcessor IL, TypeReference type)
            => new[] {IL.Create(OpCodes.Isinst, IL.Body.Method.Module.ImportReference(type))};

        public static IEnumerable<Instruction> IsInstance(this ILProcessor IL, Type type)
            => IL.IsInstance(IL.Body.Method.Module.ImportReference(type));

        public static IEnumerable<Instruction> CastClass(this ILProcessor IL, TypeReference type)
            => new[] {IL.Create(OpCodes.Castclass, IL.Body.Method.Module.ImportReference(type))};

        public static IEnumerable<Instruction> CastClass(this ILProcessor IL, Type type)
            => IL.CastClass(IL.Body.Method.Module.ImportReference(type));

        public static IEnumerable<Instruction> IsNull(this ILProcessor IL) =>
            new[]
            {
                IL.Create(OpCodes.Ldnull),
                IL.Create(OpCodes.Ceq)
            };

        public static IEnumerable<Instruction> Throw(this ILProcessor IL)
            => new[] {IL.Create(OpCodes.Throw)};

        public static IEnumerable<Instruction> Branch(this ILProcessor IL,
            bool condition,
            IEnumerable<Instruction> conditionInstructions,
            IEnumerable<Instruction> bodyInstructions,
            IEnumerable<Instruction> elseInstructions = null)
        {
            var instructions = new List<Instruction>();

            instructions.AddRange(conditionInstructions);

            if (elseInstructions != null)
            {
                var elseBlockBegin = IL.Create(OpCodes.Nop);
                var branchEnd = IL.Create(OpCodes.Nop);
                instructions.Add(IL.Create(condition ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, elseBlockBegin));
                instructions.AddRange(bodyInstructions);
                instructions.Add(IL.Create(OpCodes.Br_S, branchEnd));
                instructions.Add(elseBlockBegin);
                instructions.AddRange(elseInstructions);
                instructions.Add(branchEnd);
            }
            else
            {
                var branchEnd = IL.Create(OpCodes.Nop);
                instructions.Add(IL.Create(condition ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, branchEnd));
                instructions.AddRange(bodyInstructions);
                instructions.Add(branchEnd);
            }

            return instructions;
        }

        public static IEnumerable<Instruction> WhileLoop(this ILProcessor IL,
            bool condition,
            IEnumerable<Instruction> conditionInstructions,
            IEnumerable<Instruction> bodyInstructions,
            bool isDoWhile = false
        )
        {
            var insts = new List<Instruction>();
            var firstInst = IL.Create(OpCodes.Nop);
            insts.Add(firstInst);
            if(!isDoWhile)
            {
                var enumer = conditionInstructions.GetEnumerator();
                enumer.MoveNext();
                insts.Add(IL.Create(OpCodes.Br_S, enumer.Current));
            }
            insts.AddRange(bodyInstructions);
            insts.AddRange(conditionInstructions);
            insts.Add(IL.Create(condition ? OpCodes.Brfalse_S : OpCodes.Brtrue_S, firstInst));
            return insts;
        }

        public static IEnumerable<Instruction> NullCheckAndExecute(this ILProcessor IL,
            IEnumerable<Instruction> loadInstructions,
            IEnumerable<Instruction> executeInstructions
        )
        {
            var insts = new List<Instruction>();
            var exeInst = IL.Create(OpCodes.Nop);
            var endInst = IL.Create(OpCodes.Nop);

            insts.AddRange(loadInstructions);
            insts.Add(IL.Create(OpCodes.Dup));
            insts.Add(IL.Create(OpCodes.Brtrue_S, exeInst));
            insts.Add(IL.Create(OpCodes.Pop));
            insts.Add(IL.Create(OpCodes.Br_S, endInst));
            insts.Add(exeInst);
            insts.AddRange(executeInstructions);
            insts.Add(endInst);

            return insts;
        }

        public static IEnumerable<Instruction> PushTypeWithValue(this ILProcessor IL, TypeReference type, object value)
        {
            var ts = IL.Body.Method.Module.TypeSystem;
            if (value == null) return IL.LoadNull();
            if (type == ts.String) return IL.PushString((string)value);
            if (type == ts.Int32) return IL.PushIntConstant((int) value);
            if (type == ts.Boolean) return IL.PushIntConstant((bool) value ? 1 : 0);
            if (type == ts.TypedReference) return IL.LoadType((TypeReference) value);
            if (type.Resolve().IsEnum) return IL.PushIntConstant((int) value);
            throw new NotImplementedException();
        }

        public static IEnumerable<Instruction> SetLocal(this ILProcessor IL, int index)
        {
            switch(index)
            {
                case 0: return new[] {IL.Create(OpCodes.Stloc_0)};
                case 1: return new[] {IL.Create(OpCodes.Stloc_1)};
                case 2: return new[] {IL.Create(OpCodes.Stloc_2)};
                case 3: return new[] {IL.Create(OpCodes.Stloc_3)};
            }
            return new[] {IL.Create(OpCodes.Stloc, index)};
        }

        private static Dictionary<string, int> _firstIndexInVariableList = new Dictionary<string, int>();
        public static IEnumerable<Instruction> PushAndAddLocal(this ILProcessor IL, TypeReference variableType)
        {
            var varList = IL.Body.Variables;
            varList.Add(new VariableDefinition(IL.Body.Method.Module.ImportReference(variableType)));
            if(!_firstIndexInVariableList.ContainsKey(IL.Body.Method.FullName))
                _firstIndexInVariableList[IL.Body.Method.FullName] = varList.Count-1;
            return new[] {IL.Create(OpCodes.Stloc, varList.Last())};
        }

        public static IEnumerable<Instruction> PushAndAddLocal(this ILProcessor IL, Type variableType)
        {
            return IL.PushAndAddLocal(IL.Body.Method.Module.ImportReference(variableType));
        }

        public static IEnumerable<Instruction> LoadLocal(this ILProcessor IL, int index)
        {
            switch(index)
            {
                case 0: return new[] {IL.Create(OpCodes.Ldloc_0)};
                case 1: return new[] {IL.Create(OpCodes.Ldloc_1)};
                case 2: return new[] {IL.Create(OpCodes.Ldloc_2)};
                case 3: return new[] {IL.Create(OpCodes.Ldloc_3)};
            }
            return new[] {IL.Create(OpCodes.Ldloc, index)};
        }

        public static IEnumerable<Instruction> LoadTopLocal(this ILProcessor IL, int belowTop = 0)
            => IL.LoadLocal(Math.Max(0, IL.Body.Variables.Count-1)-belowTop);

        public static IEnumerable<Instruction> PushTopLocal(this ILProcessor IL, int belowTop = 0)
        {
            return new[] {IL.Create(OpCodes.Stloc, IL.Body.Variables[Math.Max(0, IL.Body.Variables.Count-1)-belowTop])};
        }

        public static IEnumerable<Instruction> Prepend(this ILProcessor IL, IEnumerable<Instruction> insts, bool checkForBaseCall = true)
        {
            // In the general case if checkForBaseCall is true, if the first instructions are a base call, it will inject after the base call instead
            var offset = checkForBaseCall
                    && IL.Body.Instructions[1].OpCode == OpCodes.Ldarg_0
                    && IL.Body.Instructions[2].OpCode == OpCodes.Call
                    && IL.Body.Instructions[2].Operand.ToString() == GetBaseMethod(IL.Body.Method).ToString() ? 3 : 0;
            while (IL.Body.Instructions.Count < 3)
                IL.Emit(OpCodes.Nop);

            // if(_firstIndexInVariableList.TryGetValue(IL.Body.Method.FullName, out var correctionIndex))
            // {
            //     var correctList = IL.Body.Variables.Skip(correctionIndex+1).Reverse();
            //     foreach(var correct in correctList)
            //         IL.Body.Variables[correctionIndex++] = new VariableDefinition(correct.VariableType);
            // }

            var first = IL.Body.Instructions[offset];
            foreach (var instruction in insts)
            {
                IL.InsertBefore(first, instruction);
            }
            return insts;
        }

        private static MethodDefinition GetBaseMethod(MethodDefinition method, TypeDefinition baseType = null)
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
    }
}