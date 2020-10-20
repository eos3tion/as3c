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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using as3c.As3c.Common;
using as3c.As3c.Compiler.Utils;
using As3c.Common;
using As3c.Compiler.Exceptions;
using SwfLibrary.Abc;
using SwfLibrary.Abc.Constants;
using SwfLibrary.Abc.Utils;
using SwfLibrary.Types;

namespace As3c.Compiler.Utils
{
    public class Instruction
    {
        protected ParserInformation _debugInfo;
        protected AVM2Command _cmd;
        /// <summary>
        ///  参数列表;
        /// </summary>
        protected List<string> _arguments;

        public Instruction(Abc46 abc, AVM2Command command, List<AVM2Command> arguments)
        {
            _cmd = command;
            _arguments = new List<string>();

            for (int i = 0; i < arguments.Count; ++i)
            {
                AVM2Command argument = arguments[i];

                switch (argument.OpCode)
                {
                    case Op.PushByte:
                        _arguments.Add(((byte)argument.Parameters[0]).ToString());
                        break;

                    case Op.PushShort:
                        _arguments.Add(((U30)argument.Parameters[0]).Value.ToString());
                        break;

                    case Op.PushDouble:
                        _arguments.Add(((double)(abc.ConstantPool.DoubleTable[(int)((U30)argument.Parameters[0])])).ToString());
                        break;

                    case Op.PushInt:
                        _arguments.Add(((S32)(abc.ConstantPool.IntTable[(int)((U30)argument.Parameters[0])])).Value.ToString());
                        Console.WriteLine("My arguments are now: {0}", _arguments);
                        break;

                    case Op.PushUInt:
                        _arguments.Add(((U32)(abc.ConstantPool.DoubleTable[(int)((U30)argument.Parameters[0])])).Value.ToString());
                        break;

                    case Op.PushString:
                        _arguments.Add(((StringInfo)(abc.ConstantPool.StringTable[(int)((U30)argument.Parameters[0])])).ToString());
                        break;

                    case Op.GetLocal0:
                        if ((byte)Op.GetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.GetLocal0;
                        else if ((byte)Op.SetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.SetLocal0;
                        else
                            _arguments.Add("0");
                        break;

                    case Op.GetLocal1:
                        if ((byte)Op.GetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.GetLocal1;
                        else if ((byte)Op.SetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.SetLocal1;
                        else
                            _arguments.Add("1");
                        break;

                    case Op.GetLocal2:
                        if ((byte)Op.GetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.GetLocal2;
                        else if ((byte)Op.SetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.SetLocal2;
                        else
                            _arguments.Add("2");
                        break;

                    case Op.GetLocal3:
                        if ((byte)Op.GetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.GetLocal3;
                        else if ((byte)Op.SetLocal == _cmd.OpCode)
                            _cmd.OpCode = (byte)Op.SetLocal3;
                        else
                            _arguments.Add("3");
                        break;

                    case Op.GetLocal:
                        _arguments.Add(((int)((U30)argument.Parameters[0])).ToString());
                        break;

                    default:
#if DEBUG
                        Console.WriteLine("[-] Parameter type {0} not handled ...", argument.StringRepresentation);
#endif
                        break;
                }
            }
        }


        public AVM2Command ParseCommand(Abc46 abc, Dictionary<string, AVM2Command> labels, List<JumpTargetData> jumpTargets)
        {
            ArrayList parameters = _cmd.Parameters;
            parameters.Clear();

            byte code=_cmd.OpCode;

            ///如果是个跳转指令的话;
            if (Op.getJumpOpcodesLookup().Contains(code))
            {
                string labelId = null;

                try
                {
                    labelId = Arguments[0];
                }
                catch (Exception)
                {
                    Console.WriteLine("[-] WARNING: Jumping to an unknown label");
                }

                AVM2Command target = null;
                if (labels.ContainsKey(labelId))
                {
                    target = labels[labelId];
                }


                if (code == Op.LookupSwitch)
                {
                    throw new Exception("未实现lookupSwitch的opcode");
                }

                jumpTargets.Add(new JumpTargetData(_cmd, target));
                parameters.Add((S24)10);
                return _cmd;
            }




            switch (_cmd.OpCode)
            {
                case Op.PushShort:
                    parameters.Add((U30)Convert.ToInt32(Arguments[0]));
                    break;

                case Op.AsType:
                case Op.Coerce:
                case Op.DeleteProperty:
                case Op.FindProperty:
                case Op.FindPropStrict:
                case Op.GetDescendants:
                case Op.GetLex:
                case Op.GetProperty:
                case Op.GetSuper:
                case Op.InitProperty:
                case Op.IsType:
                case Op.SetProperty:
                case Op.PushNamespace:
                     parameters.Add(NameUtil.GetMultiname(abc, Arguments[0]));
                    break;

                case Op.NewArray:
                case Op.NewObject:
                    parameters.Add((U30)Convert.ToUInt32(Arguments[0]));
                    break;
                case Op.CallProperty:
                case Op.CallPropLex:
                case Op.CallPropVoid:
                case Op.CallSuper:
                case Op.CallSuperVoid:
                case Op.ConstructProp:
                    parameters.Add(NameUtil.GetMultiname(abc, Arguments[0]));
                    parameters.Add((U30)Convert.ToUInt32(Arguments[1]));
                    break;

                case Op.NewClass:
                    parameters.Add(NameUtil.GetClass(abc, Arguments[0]));
                    break;

                case Op.PushDouble:
                    parameters.Add((U30)abc.ConstantPool.ResolveDouble(Convert.ToDouble(Arguments[0].Replace('.', ','))));
                    break;

                case Op.PushInt:
                    parameters.Add((U30)abc.ConstantPool.ResolveInt((S32)Convert.ToInt32(Arguments[0])));
                    break;

                case Op.PushUInt:
                    parameters.Add((U30)abc.ConstantPool.ResolveUInt((U32)Convert.ToUInt32(Arguments[0])));
                    break;

                case Op.DebugFile:
                case Op.PushString:
                    if (Arguments[0].StartsWith("\"") && Arguments[0].EndsWith("\""))//TODO fix ugly hack
                    {
                        parameters.Add((U30)abc.ConstantPool.ResolveString(Arguments[0].Substring(1, Arguments[0].Length - 2)));
                    }
                    else
                    {
                        parameters.Add((U30)abc.ConstantPool.ResolveString(Arguments[0]));
                    }
                    break;

                default:
                    if (0 < Command.ParameterCount)
                    {
                        try
                        {
                            foreach (string argument in Arguments)
                            {
                                 parameters.Add((byte)Convert.ToByte(argument));
                            }
                        }
                        catch (Exception)
                        {
                            throw new InstructionException(InstructionException.Type.UnknownType, DebugInfo);
                        }
                    }
                    break;
            }

            return _cmd;
        }


        public Instruction(string command, ParserInformation debugInfo)
        {
            _debugInfo = debugInfo;
            
            char[] separators = { ' ' };

            string[] tokens = command.Split(separators,2);

            AVM2Command cmd = Translator.ToCommand(tokens[0]);

            if (cmd == null)
            {
                throw new InstructionException(InstructionException.Type.InvalidSyntax, _debugInfo);
            }

            if (tokens.Length == 1 && cmd.ParameterCount > 0)
            {
                throw new InstructionException(InstructionException.Type.NotEnoughArguments, _debugInfo);
            }
            else
            {
                if (tokens.Length == 2)
                {
                    separators[0] = ',';
                    string[] args = tokens[1].Split(separators, 0xff);

                    if (args.Length > cmd.ParameterCount)
                    {
                        throw new InstructionException(InstructionException.Type.TooManyArguments, _debugInfo);
                    }
                    else if (args.Length < cmd.ParameterCount)
                    {
                        throw new InstructionException(InstructionException.Type.NotEnoughArguments, _debugInfo);
                    }

                    _arguments = new List<string>();

                    if ( cmd.ParameterCount != 0 )
                    {
                        for (int i = 0; i < args.Length; ++i)
                        {
                            _arguments.Add(args[i].Trim());
                        }
                    }
                }
            }

            _cmd = cmd; 
        }

        public List<string> Arguments { get { return _arguments; } }

        public AVM2Command Command { get { return _cmd; } }

        public ParserInformation DebugInfo { get { return _debugInfo; } }
    }
}
