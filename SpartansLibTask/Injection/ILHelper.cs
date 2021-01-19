using System;
using System.Collections.Generic;
using System.Data;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SpartansLib.Injection
{
    // MIT License
    //
    // Copyright (c) 2020 crym0nster
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in all
    // copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    // SOFTWARE.
    //
    // ReSharper disable once InconsistentNaming
    public class ILHelper
    {
        // ReSharper disable once InconsistentNaming
        public readonly ILProcessor IL;

        public ILHelper(ILProcessor il)
        {
            IL = il;
            _ts = IL.Body.Method.Module.TypeSystem;
        }

        public static IEnumerable<Instruction> Compose(params IEnumerable<Instruction>[] instructionsArray)
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

        public IEnumerable<Instruction> Nop()
            => new[] {IL.Create(OpCodes.Nop)};

        public IEnumerable<Instruction> PushThis()
            => new[] {IL.Create(OpCodes.Ldarg_0)};

        public IEnumerable<Instruction> PushString(string arg)
            => new[] {IL.Create(OpCodes.Ldstr, arg)};

        public IEnumerable<Instruction> PushIntConstant(int constant)
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

        public IEnumerable<Instruction> PushArgument(int index)
        {
            switch (index)
            {
                case 0:
                    return PushThis();
                case 1:
                    return new[] {IL.Create(OpCodes.Ldarg_1)};
                case 2:
                    return new[] { IL.Create(OpCodes.Ldarg_2) };
                case 3:
                    return new[] {IL.Create(OpCodes.Ldarg_3)};
            }

            return new[] { IL.Create(OpCodes.Ldarg, index) };
        }

        public IEnumerable<Instruction> CallMethod(MethodReference method)
            => new[] {IL.Create(OpCodes.Call, method)};

        public IEnumerable<Instruction> CallMethodVirtual(MethodReference method)
            => new[] {IL.Create(OpCodes.Callvirt, method)};

        public IEnumerable<Instruction> NewObj(MethodReference ctorMethod)
            => new[] { IL.Create(OpCodes.Newobj , ctorMethod) };

        public IEnumerable<Instruction> LoadNull()
            => new[] { IL.Create(OpCodes.Ldnull) };

        public IEnumerable<Instruction> LoadType(TypeReference type)
            => new[] {IL.Create(OpCodes.Ldtoken, type)};

        public IEnumerable<Instruction> LoadFieldRef(FieldReference field)
            => new[] {IL.Create(OpCodes.Ldtoken, field)};

        public IEnumerable<Instruction> LoadMethodRef(MethodReference method)
            => new[] {IL.Create(OpCodes.Ldtoken, method)};

        public IEnumerable<Instruction> LoadField(FieldDefinition field)
            => new[] {IL.Create(OpCodes.Ldfld, field)};

        public IEnumerable<Instruction> SetField(FieldDefinition field)
            => new[] {IL.Create(OpCodes.Stfld, field)};

        public IEnumerable<Instruction> LoadProperty(PropertyDefinition property)
            => new[] {IL.Create(OpCodes.Call, property.GetMethod)};

        public IEnumerable<Instruction> SetProperty(PropertyDefinition property)
            => new[] {IL.Create(OpCodes.Call, property.SetMethod)};

        public IEnumerable<Instruction> Return()
            => new[] { IL.Create(OpCodes.Ret) };

        public IEnumerable<Instruction> IsInstance(TypeReference type)
            => new[] {IL.Create(OpCodes.Isinst, type)};

        public IEnumerable<Instruction> IsNull() =>
            new[]
            {
                IL.Create(OpCodes.Ldnull),
                IL.Create(OpCodes.Ceq)
            };

        public IEnumerable<Instruction> Branch(bool condition,
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

        private readonly TypeSystem _ts;
        public IEnumerable<Instruction> PushTypeWithValue(TypeReference type, object value)
        {
            if (value == null) return LoadNull();
            if (type == _ts.String) return PushString((string)value);
            if (type == _ts.Int32) return PushIntConstant((int) value);
            if (type == _ts.Boolean) return PushIntConstant((bool) value ? 1 : 0);
            if (type == _ts.TypedReference) return LoadType((TypeReference) value);
            if (type.Resolve().IsEnum) return PushIntConstant((int) value);
            throw new NotImplementedException();
        }
    }
}