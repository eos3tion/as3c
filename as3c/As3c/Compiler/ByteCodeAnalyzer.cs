/*
Copyright(C) 2007 Joa Ebert

As3c is an ActionScript 3 bytecode compiler for the AVM2.

As3c  is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

As3c is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using System.Collections.Generic;
using System.Text;
using SwfLibrary.Types;
using As3c.Common;
using SwfLibrary.Abc;
using SwfLibrary.Abc.Utils;
using SwfLibrary.Abc.Constants;
using System.Collections;
using as3c.As3c.Common;

namespace As3c.Compiler
{
    //TODO merge 3 loops into 1...
    public class ByteCodeAnalyzer
    {
        private struct ConditionalJump
        {
            public uint pos;
            public S24 offset;
            public int stack;
        }

        public static uint InvalidStack = 0;

        private static uint _flags;

        public static uint Flags
        {
            get { return _flags; }
        }

        private static void ClearFlags()
        {
            _flags = 0;
        }

        private static void RaiseFlag(uint flag)
        {
            _flags |= flag;
        }

        public static U30 CalcLocalCount(List<AVM2Command> instructions)
        {
            ClearFlags();

            int i = 0;
            int n = instructions.Count;

            uint local = 0;
            uint maxLocal = 0;

            while (i < n)
            {
                AVM2Command avm2Command = instructions[i];
                switch (avm2Command.OpCode)
                {
                    case Op.Debug:
                        byte type = (byte)avm2Command.Parameters[0];
                        if (1 == type)
                        {
                            local = (byte)avm2Command.Parameters[2] + 1U;
                        }
                        break;
                    case Op.SetLocal:
                    case Op.GetLocal:
                        U30 register = (U30)avm2Command.Parameters[0];
                        local = register.Value + 1U;
                        break;

                    case Op.SetLocal0:
                    case Op.GetLocal0:
                        local = 1;
                        break;

                    case Op.SetLocal1:
                    case Op.GetLocal1:
                        local = 2;
                        break;

                    case Op.SetLocal2:
                    case Op.GetLocal2:
                        local = 3;
                        break;

                    case Op.SetLocal3:
                    case Op.GetLocal3:
                        local = 4;
                        break;
                }

                if (local > maxLocal)
                {
                    maxLocal = local;
                }
                ++i;
            }

            U30 result = new U30();
            result.Value = maxLocal;

            return result;
        }

        public static U30 CalcScopeDepth(byte[] code)
        {
            ClearFlags();

            uint i = 0;
            uint n = (uint)code.Length;

            int scopeDepth = 0;
            int maxScopeDepth = 0;

            while (i < n)
            {
                AVM2Command cmd = null;

                try
                {
                    cmd = Translator.ToCommand(code[i++]);
                }
                catch (Exception)
                {
                    DebugUtil.DumpOpUntilError(code);
                    throw new Exception(String.Format("Can not translate {0} correct at {1}.", code[i - 1], i - 1));
                }


                if (null == cmd)
                    throw new Exception();

                i += cmd.ReadParameters(code, i);

                switch (cmd.OpCode)
                {
                    case Op.PushScope:
                    case Op.PushWith:
                        scopeDepth++;
                        break;

                    case Op.PopScope:
                        scopeDepth--;
                        break;
                }

                if (scopeDepth > maxScopeDepth)
                    maxScopeDepth = scopeDepth;
            }

            U30 result = new U30();
            result.Value = (uint)maxScopeDepth;

            return result;
        }

        private static bool ContainsJumpFrom(List<ConditionalJump> list, ConditionalJump jump)
        {
            int i = 0;
            int n = list.Count;

            for (; i < n; ++i)
            {
                ConditionalJump jumpToTest = list[i];

                if (jumpToTest.pos == jump.pos)
                    return true;
            }

            return false;
        }

        public static U30 CalcMaxStack(byte[] code)
        {
            ClearFlags();

            uint i = 0;
            uint n = (uint)code.Length;

            int stack = 0;
            int maxStack = 0;

            bool corrupt = false;

            uint j = 0;

            List<ConditionalJump> jumps = new List<ConditionalJump>();
            List<uint> unconditionalJumps = new List<uint>();

            ConditionalJump cj;

            int jumpIndex = 0;

            do
            {
                while (i < n)
                {
                    AVM2Command cmd = null;

                    try
                    {
                        cmd = Translator.ToCommand(code[i++]);
                    }
                    catch (Exception)
                    {
                        DebugUtil.DumpOpUntilError(code);
                        cmd = Translator.ToCommand(code[i-1]);
                        throw new Exception(String.Format("Can not translate {0} correct at {1}.", code[i - 1], i-1));
                    }

                    if (null == cmd)
                        throw new Exception();

                    i += cmd.ReadParameters(code, i);

                    // There are a couple of opcodes marked "incorrect" with a comment.
                    // The explanation is: If the index in the multiname array is a RTQName
                    // there could be a namspace and/or namespace set on the stack as well
                    // that would be popped. 
                    //
                    // We do not take that in account here - in the worst case that a namespace
                    // and namespace set is present it could add +2 to the max sack if the
                    // stack is greater than the one we already have.
                    //
                    // In the calculation of the possible max stack we will therefore only remove
                    // the number of arguments from the current value. If there are no arguments
                    // the opcode will be listed here as incorrect without any following calculation.
                    //
                    // Although this is not a problem for the Flash Player. It is just not very
                    // nice...
                    switch (cmd.OpCode)
                    {
                        case Op.Jump:
                            if (!unconditionalJumps.Contains(i))
                            {
                                unconditionalJumps.Add(i);

                                i = (uint)((int)i + (int)((S24)cmd.Parameters[0]).Value);
                            }
                            else
                            {
                                //LOOP BAAM!
                            }
                            break;

                        case Op.PushByte:
                        case Op.PushDouble:
                        case Op.PushFalse:
                        case Op.PushInt:
                        case Op.PushNamespace:
                        case Op.PushNaN:
                        case Op.PushNull:
                        case Op.PushShort:
                        case Op.PushString:
                        case Op.PushTrue:
                        case Op.PushUInt:
                        case Op.PushUndefined:
                        case Op.Dup:
                        case Op.FindProperty://incorrect
                        case Op.FindPropStrict://incorrect
                        case Op.GetGlobalScope:
                        case Op.GetGlobalSlot:
                        case Op.GetLex:
                        case Op.GetLocal:
                        case Op.GetLocal0:
                        case Op.GetLocal1:
                        case Op.GetLocal2:
                        case Op.GetLocal3:
                        case Op.GetScopeObject:
                        case Op.HasNext2:
                        case Op.NewActivation:
                        case Op.NewCatch:
                        case Op.NewFunction:
                            ++stack;
                            break;

                        case Op.IfFalse:
                        case Op.IfTrue:
                            --stack;

                            cj = new ConditionalJump();

                            cj.offset = (S24)cmd.Parameters[0];
                            cj.pos = i;
                            cj.stack = stack;

                            if(!ContainsJumpFrom(jumps, cj))
                                jumps.Add(cj);
                            break;

                        case Op.Add:
                        case Op.AddInt:
                        case Op.AsTypeLate:
                        case Op.BitAnd:
                        case Op.BitOr:
                        case Op.BitXor:
                        case Op.Divide:
                        case Op.DefaultXmlNamespaceL:
                        case Op.Equals:
                        case Op.GreaterEquals:
                        case Op.GreaterThan:
                        case Op.HasNext:
                        case Op.In:
                        case Op.InstanceOf:
                        case Op.IsTypeLate:
                        case Op.LessEquals:
                        case Op.LessThan:
                        case Op.ShiftLeft:
                        case Op.Modulo:
                        case Op.Multiply:
                        case Op.MultiplyInt:
                        case Op.NextName:
                        case Op.NextValue:
                        case Op.Pop:
                        case Op.PushScope://pop from stack, push to scope stack
                        case Op.PushWith://pop from stack, push to scope stack
                        case Op.ReturnValue:
                        case Op.ShiftRight:
                        case Op.SetLocal:
                        case Op.SetLocal0:
                        case Op.SetLocal1:
                        case Op.SetLocal2:
                        case Op.SetLocal3:
                        case Op.SetGlobalSlot:
                        case Op.StrictEquals:
                        case Op.Subtract:
                        case Op.SubtractInt:
                        case Op.Throw:
                        case Op.ShiftRightUnsigned:
                            --stack;
                            break;

                        case Op.LookupSwitch:
                            --stack;
                            for (int k = 2; k < cmd.ParameterCount; ++k)
                            {
                                cj.offset = (S24)cmd.Parameters[k];
                                cj.pos = i;
                                cj.stack = stack;

                                if (!ContainsJumpFrom(jumps, cj))
                                    jumps.Add(cj);
                            }
                            break;

                        case Op.IfEqual:
                        case Op.IfGreaterEqual:
                        case Op.IfGreaterThan:
                        case Op.IfLessEqual:
                        case Op.IfLessThan:
                        case Op.IfNotEqual:
                        case Op.IfNotGreaterEqual:
                        case Op.IfNotGreaterThan:
                        case Op.IfNotLessEqual:
                        case Op.IfNotLessThan:
                        case Op.IfStrictEqual:
                        case Op.IfStrictNotEqual:
                            stack -= 2;

                            cj = new ConditionalJump();

                            cj.offset = (S24)cmd.Parameters[0];
                            cj.pos = i;
                            cj.stack = stack;

                            if (!ContainsJumpFrom(jumps, cj))
                                jumps.Add(cj);
                            break;

                        case Op.InitProperty:
                        case Op.SetProperty://incorrect
                        case Op.SetSlot:
                        case Op.SetSuper://incorrect
                            stack -= 2;
                            break;

                        case Op.Call:
                        case Op.ConstructSuper:
                            stack -= 1 + (int)((U30)cmd.Parameters[0]);
                            break;

                        case Op.Construct:
                            stack -= (int)((U30)cmd.Parameters[0]); ;
                            break;

                        case Op.NewArray:
                            stack -= (int)((U30)cmd.Parameters[0]) - 1;
                            break;

                        case Op.CallMethod:
                        case Op.CallProperty://incorrect
                        case Op.CallPropLex://incorrect
                        case Op.CallPropVoid://incorrect
                        case Op.CallStatic:
                        case Op.CallSuper://incorrect
                        case Op.CallSuperVoid://incorrect
                        case Op.ConstructProp:
                            stack -= (int)((U30)cmd.Parameters[1]);
                            break;

                        case Op.NewObject:
                            stack -= ((int)((U30)cmd.Parameters[0])) << 1;
                            break;

                        //case Op.DeleteProperty://incorrect
                        //case Op.GetDescendants://incorrect
                        //case Op.GetProperty://incorrect
                        //case Op.GetSuper://incorrect
                        //    break;
                    }

                    if (stack < 0)
                    {
                        RaiseFlag(InvalidStack);
                        corrupt = true;
                        Console.WriteLine("[-] Warning: Stack underflow error at operation {0} (#{1})...", cmd.StringRepresentation, j);
                    }

                    if (stack > maxStack)
                        maxStack = stack;

                    ++j;
                }

                if(jumpIndex < jumps.Count)
                {
                    ConditionalJump nextScan = jumps[jumpIndex++];

                    i = (uint)((int)nextScan.pos + (int)nextScan.offset);

                    stack = nextScan.stack;
                }
                else
                {
                    break;
                }
            } while (true);

            U30 result = new U30();
            result.Value = (uint)maxStack;

            if (corrupt)
                DebugUtil.DumpOpUntilError(code);

            return result;
        }

        public static void CheckInlineFlag(Abc46 abc, MethodBodyInfo body)
        {
            byte[] code = body.Code;

            uint i = 0;
            uint n = (uint)code.Length;

            while (i < n)
            {
                AVM2Command command = Translator.ToCommand(code[i++]);
                i += command.ReadParameters(code, i);

                if (command.OpCode == (byte)Op.FindPropStrict)
                {
                    string name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)command.Parameters[0]).Value]));

                    if (CompilerInline.inlineMarker == name)
                    {
                        body.IsInline = true;
                        return;
                    }
                }
            }
        }
    }
}
