using System;
using System.Collections.Generic;
using System.Text;
using SwfLibrary;
using SwfLibrary.Abc;
using As3c.Common;
using System.Collections;
using SwfLibrary.Types;
using SwfLibrary.Abc.Constants;
using SwfLibrary.Abc.Utils;
using as3c.As3c.Common;
using System.IO;
using As3c.Compiler.Exceptions;
using as3c.As3c.Compiler.Utils;
using As3c.Compiler.Utils;

namespace As3c.Compiler
{
    public class CompilerInline :ICompile
    {
        protected const string asmWrapper = "public::com.youbt.asm::__asm";
        protected const string opKeyword = "public::com.youbt.asm::Op";
        public const string inlineMarker = "public::com.youbt.asm::__inline";

        protected SwfFormat _swf;
        protected List<AVM2Command> injects = new List<AVM2Command>();

        public CompilerInline()
        {
        }
        public void Write(Stream fs)
        {
            _swf.Write(fs);
        }

        public void Compile(SwfFormat swf)
        {
            _swf = swf;

            HashSet<int> hash = new HashSet<int>();
            ClassInfo classInfo;
            ScriptInfo scriptInfo;
            for (int i = 0, n = _swf.AbcCount; i < n; ++i)
            {
                Abc46 abc = _swf.GetAbcAt(i);
                int j, m;

                hash.Clear();

                /*ArrayList str = new ArrayList();
                for (j = 0, m = abc.Classes.Count; j < m; j++)
                {
                    classInfo = abc.Classes[j];
                    string name=NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)abc.Instances[j].Name]));
                    if (j ==12)
                    {
                        str.Add(name);
                    }
                 }*/

                for (j = 0, m = abc.Classes.Count; j < m; j++)
                {
                    classInfo = abc.Classes[j];
                    string name=NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)abc.Instances[j].Name]));
                     hash.Add((int)classInfo.CInit);
                }

                for (j = 0, m = abc.Scripts.Count; j < m; j++)
                {
                    scriptInfo = abc.Scripts[j];
                    hash.Add((int)scriptInfo.Init.Value);
                }

                MethodBodyInfo body;
                for (j = 0, m = abc.MethodBodies.Count; j < m; ++j)
                {
                    body = abc.MethodBodies[j];
                    ParserBody(abc, body);
                    DecoderInner(abc, body, hash.Contains(j));
                    RewriteBody(abc, body);
                }
            }
        }

        protected void ParserBody(Abc46 abc, MethodBodyInfo body)
        {
            Dictionary<uint, AVM2Command> opcodePositions = new Dictionary<uint, AVM2Command>();

            AVM2Command command;
            HashSet<byte> jumpOpcodes = Op.getJumpOpcodesLookup();
            List<AVM2Command> instructions = body.Instructions;
            List<JumpTargetData> jumpTargets = body.JumpTargets;

            byte[] _code = body.Code;

            uint i = 0;
            uint n = (uint)_code.Length;

            while (i < n)
            {
                command = Translator.ToCommand(_code[i]);

                command.baseLocation = i;
                opcodePositions.Add(i++, command);
                if (null == command)
                {
                    throw new Exception("Unknown opcode detected.");
                }

                i += command.ReadParameters(_code, i);
                command.endLocation = i;

                if (jumpOpcodes.Contains(command.OpCode))
                {
                    jumpTargets.Add(new JumpTargetData(command));
                }
                instructions.Add(command);
            }

            #region 引用跳转的指命所指的指命对像;
            foreach (ExceptionInfo ei in body.Exceptions)
            {
                ei.FromCommand = opcodePositions[ei.From.Value];
                ei.ToCommand = opcodePositions[ei.To.Value];
                ei.TargetCommand = opcodePositions[ei.Target.Value];
            }


            uint targetPos;
            uint len;
            AVM2Command target;
            AVM2Command jumper;
            foreach (JumpTargetData jumpTargetData in jumpTargets)
            {
                jumper = jumpTargetData.JumpCommand;
                if (jumper.OpCode == Op.LookupSwitch)
                {
                    targetPos = (uint)(jumper.baseLocation + ((S24)jumper.Parameters[0]).Value);

                    if (!opcodePositions.ContainsKey(targetPos))
                    {
                        target = AVM2Command.END_OF_BODY;
                    }
                    else
                    {
                        target = opcodePositions[targetPos];
                    }

                    jumpTargetData.TargetCommand = target;


                    len = ((U30)jumper.Parameters[1]).Value;
                    for (int j = 0; j <= len; ++j)
                    {
                        targetPos = (uint)(jumper.baseLocation + ((S24)jumper.Parameters[j + 2]).Value);
                        if (!opcodePositions.ContainsKey(targetPos))
                        {
                            target = AVM2Command.END_OF_BODY;
                        }
                        else
                        {
                            target = opcodePositions[targetPos];
                        }
                        jumpTargetData.addTarget(target);
                    }
                }
                else
                {
                    targetPos = (uint)(jumper.endLocation + ((S24)jumper.Parameters[0]).Value);
                    if (!opcodePositions.ContainsKey(targetPos))
                    {
                        target = AVM2Command.END_OF_BODY;
                    }
                    else
                    {
                        target = opcodePositions[targetPos];
                    }

                    jumpTargetData.TargetCommand = target;
                }
            }
            #endregion
        }

        protected void DecoderInner(Abc46 abc, MethodBodyInfo body, bool isIniter = false)
        {
            Dictionary<string, AVM2Command> labels = new Dictionary<string, AVM2Command>();
            List<AVM2Command> instructions = body.Instructions;
            List<JumpTargetData> jumpTargets = body.JumpTargets;
            List<Instruction> tempInstructions = new List<Instruction>();
            List<AVM2Command> newInstructions = new List<AVM2Command>();


            AVM2Command command;
            AVM2Command inlineCommand;
            Instruction instruction;

            bool patchBody = false;
            string name;
            int len = instructions.Count;
            int i = 0;

            while (i < len)
            {
                command = instructions[i++];
                if (command.OpCode == Op.FindPropStrict)
                {
                    name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)command.Parameters[0]).Value]));

                    if (asmWrapper == name)
                    {
                        #region asmWrapper;
#if DEBUG
                        Console.WriteLine("[i] Parsing inline block in method {0}", body.Method.Value);
#endif

                        bool parse = true;
                        patchBody = true;
                        while (parse && i < len)
                        {
                            inlineCommand = instructions[i++];
                            switch (inlineCommand.OpCode)
                            {
                                case Op.Debug:
                                case Op.DebugFile:
                                case Op.DebugLine:
                                    newInstructions.Add(inlineCommand);
                                    break;
                                case Op.PushString:
                                    name = ((StringInfo)abc.ConstantPool.StringTable[(int)((U30)inlineCommand.Parameters[0])]).ToString();
                                    if (name.IndexOf('.') != 0 || name.IndexOf(':') != (name.Length - 1))
                                    {
                                        newInstructions.Add(inlineCommand);
                                        continue;
                                    }

                                    name = name.Substring(0, name.Length - 1);
                                    AVM2Command label = Translator.ToCommand(0x09);
                                    newInstructions.Add(label);
                                    labels.Add(name, label);

                                    break;
                                case Op.FindPropStrict:
                                case Op.GetLex:
                                    #region innerOp
                                    name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)inlineCommand.Parameters[0]).Value]));
                                    if (opKeyword == name)
                                    {

                                        //下一个节点为getProperty;
                                        if (inlineCommand.OpCode == Op.FindPropStrict) i++;
                                    }
                                    else
                                    {
                                        throw new Exception("Malformed inline block. GetLex call with invalid parameters");
                                    }


                                    List<AVM2Command> args = new List<AVM2Command>();
                                    AVM2Command userCommand = null;
                                    uint argc;
                                    while (i < len)
                                    {
                                        AVM2Command arg = instructions[i++];

                                        if (Op.CallProperty == arg.OpCode)
                                        {
                                            name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)arg.Parameters[0]).Value]));
                                            userCommand = Translator.ToCommand(name, true);
                                            argc = ((U30)arg.Parameters[1]).Value;

                                            if (null == userCommand)
                                            {
                                                throw new Exception(String.Format("Unknown command {0}.", name));
                                            }

                                            break;
                                        }
                                        args.Add(arg);
                                    }

                                    if (null == userCommand)
                                    {
                                        throw new Exception("Malformed inline block.");
                                    }

                                    //对label的内联特殊处理;
                                    if (userCommand.OpCode == Op.Label)
                                    {
                                        name = abc.ConstantPool.StringTable[(int)((U30)(args[0].Parameters[0])).Value].ToString();
                                        if (String.IsNullOrEmpty(name))
                                        {
                                            newInstructions.Add(inlineCommand);
                                            continue;
                                        }
                                        labels.Add(name, userCommand);
                                        newInstructions.Add(userCommand);
                                        continue;
                                    }

                                    instruction = new Instruction(abc, userCommand, args);
                                    tempInstructions.Add(instruction);
                                    newInstructions.Add(userCommand);
                                    #endregion innerOp
                                    break;

                                case Op.CallProperty:
                                    name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)inlineCommand.Parameters[0]).Value]));
                                    if (asmWrapper == name)
                                    {
                                        AVM2Command pop = instructions[i++];
                                        if (Op.Pop != pop.OpCode)
                                        {
                                            throw new Exception("应该为一个Pop标签才对");
                                        }
                                        parse = false;
                                    }
                                    else
                                    {
                                        newInstructions.Add(inlineCommand);
                                    }
                                    break;

                                case Op.CallPropVoid:
                                    name = NameUtil.ResolveMultiname(abc, (MultinameInfo)(abc.ConstantPool.MultinameTable[(int)((U30)inlineCommand.Parameters[0]).Value]));
                                    if (asmWrapper == name)
                                    {
                                        parse = false;
                                    }
                                    else
                                    {
                                        throw new Exception("Malformed inline block. Method calls are not accepted ...");
                                    }
                                    break;

                            }
                        }
                        #endregion asmWrapper
                    }
                    else
                    {
                        newInstructions.Add(command);
                    }

                }
                else
                {
                    newInstructions.Add(command);
                }
            }
            //isIniter = false;
            if (isIniter && newInstructions.Count>2)
            {

                int atIndex = 2;
                if ((newInstructions[0] as AVM2Command).OpCode == 0xf1)
                {
                    atIndex = 4;
                }

                injects.Clear();
                //jump;
                AVM2Command create = Translator.ToCommand(0x10);
                create.Parameters.Add((S24)10);
                injects.Add(create);
                JumpTargetData jump = new JumpTargetData(create);

                //createobject;
                create = Translator.ToCommand(0x56);
                U30 u30 = new U30();
                u30.Value = 10000000;
                create.Parameters.Add(u30);
                injects.Add(create);
                //setlocal0;
                injects.Add(Translator.ToCommand(0xd4));
                //inject;
                newInstructions.InsertRange(atIndex, injects);

                jump.TargetCommand = newInstructions[atIndex+injects.Count];
                jumpTargets.Add(jump);

                body.newInstructions = newInstructions;
            }

            if (patchBody)
            {
                foreach (Instruction item in tempInstructions)
                {
                    item.ParseCommand(abc, labels, jumpTargets);
                }
                body.newInstructions = newInstructions;
            }
        }

        protected void RewriteBody(Abc46 abc, MethodBodyInfo body)
        {
            Dictionary<uint, AVM2Command> opcodePositions = new Dictionary<uint, AVM2Command>();
            List<AVM2Command> instructions;
            List<JumpTargetData> jumpTargets = body.JumpTargets;

            MemoryStream buffer = new MemoryStream();
            BinaryWriter codeWriter = new BinaryWriter(buffer);
            BinaryReader codeReader = new BinaryReader(buffer);

            List<AVM2Command> newInstructions = body.newInstructions;
            if (newInstructions == null)
            {
                instructions = body.Instructions;
            }
            else
            {
                instructions = newInstructions;
            }

            int n = instructions.Count;
            foreach (AVM2Command command in instructions)
            {
                command.baseLocation = (uint)buffer.Position;

                codeWriter.Write((byte)command.OpCode);
                command.WriteParameters(codeWriter);

                command.endLocation = (uint)buffer.Position;
            }


            foreach (JumpTargetData jumpTargetData in jumpTargets)
            {
                resolveJumpTarget(jumpTargetData, codeWriter, codeReader);
            }

            buffer.Seek(0, SeekOrigin.Begin);

            byte[] code = codeReader.ReadBytes((int)buffer.Length);

            body.Code = code;

            foreach (ExceptionInfo ei in body.Exceptions)
            {
                ei.From = (U30)ei.FromCommand.baseLocation;
                ei.To = (U30)ei.ToCommand.baseLocation; 
                ei.Target = (U30)ei.TargetCommand.baseLocation;
            }

            if (jumpTargets.Count>0)
            {
                //TODO
            }
        }

        protected void resolveJumpTarget(JumpTargetData jumpTargetData, BinaryWriter codeWriter, BinaryReader codeReader)
        {
            AVM2Command jumpCommand = jumpTargetData.JumpCommand;
            AVM2Command targetCommand = jumpTargetData.TargetCommand;
            if (targetCommand == null)
            {
                throw new Exception("未知jump位置");
            }
            if (targetCommand == AVM2Command.END_OF_BODY)
            {
#if DEBUG
                Console.WriteLine("jump to end");
#endif
                return;
            }

            uint targetPos;
            int newTargetPos;
            int oldTargetPos;
            uint jumpPointer;
            uint targetPointer;

            if (jumpCommand.OpCode == Op.LookupSwitch)
            {
                oldTargetPos = ((S24)jumpCommand.Parameters[0]).Value;
                jumpPointer = jumpCommand.baseLocation;
                targetPointer = targetCommand.baseLocation;

                targetPos = (uint)(jumpPointer + oldTargetPos);

                if (targetPos != targetPointer)
                {
                    codeWriter.BaseStream.Position = jumpPointer + 1;
                    newTargetPos = (int)(targetPointer - jumpPointer);
#if DEBUG
                    Console.WriteLine("relocate lookupSwitch position from {0} to {1}", oldTargetPos, newTargetPos);
#endif
                    Primitives.WriteS24(codeWriter, newTargetPos);
                }

                int index;
                int len = jumpTargetData.ExtraCommands.Count;
                for (int i = 0; i < len; ++i)
                {
                    targetCommand = jumpTargetData.ExtraCommands[i];
                    targetPointer = targetCommand.baseLocation;

                    oldTargetPos = ((S24)jumpCommand.Parameters[i + 2]).Value;
                    targetPos = (uint)(jumpPointer + oldTargetPos);

                    if (targetPos != targetPointer)
                    {
                        codeWriter.BaseStream.Position = jumpPointer + 1;

                        index = i;
                        Primitives.ReadS24(codeReader);
                        Primitives.ReadU30(codeReader);
                        while (index-- > 0)
                        {
                            Primitives.ReadS24(codeReader);
                        }

                        newTargetPos = (int)(targetPointer - jumpPointer);
#if DEBUG
                        Console.WriteLine("relocate lookupSwitch position from {0} to {1}", oldTargetPos, newTargetPos);
#endif
                        Primitives.WriteS24(codeWriter, newTargetPos);
                    }


                }

            }
            else
            {
                targetPointer = targetCommand.baseLocation;
                jumpPointer = jumpCommand.endLocation;

                oldTargetPos = ((S24)jumpCommand.Parameters[0]).Value;
                targetPos = (uint)(jumpPointer + oldTargetPos);

                if (targetPos != targetPointer)
                {
                    codeWriter.BaseStream.Position = jumpCommand.baseLocation+1;

                    newTargetPos = (int)(targetPointer - jumpPointer);
#if DEBUG
                    Console.WriteLine("relocate jump position from {0} to {1}", oldTargetPos, newTargetPos);
#endif
                    Primitives.WriteS24(codeWriter, newTargetPos);
                }

            }

        }

    }
}
