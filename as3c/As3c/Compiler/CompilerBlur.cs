using as3c.As3c.Common;
using As3c.Common;
using SwfLibrary;
using SwfLibrary.Abc;
using SwfLibrary.Abc.Constants;
using SwfLibrary.Abc.Traits;
using SwfLibrary.Abc.Utils;
using SwfLibrary.Swf;
using SwfLibrary.Types;
using SwfLibrary.Types.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace As3c.Compiler
{
    public class CompilerBlur : ICompile
    {
        protected static char[] dotChar = new char[] { '.' };
        protected static char[] hoChar  = new char[] { '_' };
        protected static string[] moChar= new string[] { "::" };
        protected static string[] lineChar = new string[] { "\r\n","\n"};
        protected static char[] dhChar = new char[] { ',',' ','.'};

        //替换字符表;
        protected static string codeString = @"αβγδεζηθικλμνξοπρστυφχψω";
        protected static char[] codes;

        //顺序也重要(先找最多字符);
        private static string[] classDefineByStringChar = new string[] { "::",":", "." };
        protected static string registerClassAliasKey = "public::flash.net::registerClassAlias";
        protected static HashSet<string> filterClasses;

        private static int position=0;
        private static int len = 11;

        protected SwfFormat _swf;


        protected HashSet<string> allSWFClassStringList;

        /// <summary>
        /// 代码内的过滤列表
        /// </summary>
        protected HashSet<string> innerFiltersList;
        /// <summary>
        /// flashplayer和自定义文件里的过滤列表;
        /// </summary>
        protected HashSet<string> globalFiltersList;

        /// <summary>
        /// 正则表达式的过滤列表;
        /// </summary>
        protected HashSet<string> regexFiltersList;

        protected HashSet<String> registerClassAliasMapping;

        protected Dictionary<String, byte[]> mapping; 
        public CompilerBlur(string[] args=null)
        {
           

            globalFiltersList = new HashSet<string>();
            filterClasses = new HashSet<string>();
            filterClasses.Add("RFSystemManager");
            filterClasses.Add("RFPluginUtils");
            filterClasses.Add("RFPluginUtils");
            filterClasses.Add("StringUtil");

#if DEBUG
            codeString="ABCDEFGHIJKLMNOPQRSTUVWXYZ";
#endif
            codes = codeString.ToCharArray();
            len = codes.Length;

            //default;
            globalFiltersList.Add("versionNumber");//air版本更新xml


            regexFiltersList = new HashSet<string>();

            if (args!=null && args.Length > 0)
            {
                defaultBlueFilterGet(args);
            }
            
        }

        protected void defaultBlueFilterGet(string[] args)
        {
            foreach (string path in args)
            {
                if (parseArgs(path))
                {
                    continue;
                }
                if (File.Exists(path) == false)
                {
                    continue;
                }

                FileInfo fileInfo = new FileInfo(path);

                string extension = fileInfo.Extension;
               /* if (extension != ".bf" && extension != ".reg" && extension != ".txt")
                {
                    continue;
                }*/

                string value;
                string[] list;
                string[] tempList;
                switch (extension)
                {
                    case ".bf":
                        FileStream fs = File.OpenRead(path);
                        BinaryReader reader = new BinaryReader(fs);
                        int n = reader.ReadInt32();
                        while (n-- > 0)
                        {
                            value = reader.ReadString();
                            globalFiltersList.Add(value);
                        }
                        reader.Close();

                        break;
                    case ".txt":
                        value = File.ReadAllText(path);
                        list = value.Split(lineChar, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in list)
                        {
                            tempList=item.Split(dhChar, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string item2 in tempList)
                            {
                                globalFiltersList.Add(item2);
                            }
                            
                        }
                        break;
                    case ".cls":
                        value = File.ReadAllText(path);
                        list = value.Split(lineChar, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in list)
                        {
                            tempList = item.Split(dhChar, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string item2 in tempList)
                            {
                                filterClasses.Add(item2);
                            }

                        }
                        break;

                    case ".reg":
                        value = File.ReadAllText(path);
                        list = value.Split(lineChar, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in list)
                        {
                            tempList = item.Split(dhChar, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string item2 in tempList)
                            {
                                regexFiltersList.Add(item2);
                            }
                        }
                        break;
                }

            }
        }

        virtual protected bool parseArgs(string path)
        {
            return false;
        }

        public void Write(Stream fs)
        {
            _swf.Write(fs);
        }

        virtual protected void CompileBegin()
        {
            innerFiltersList = new HashSet<string>();
            registerClassAliasMapping = new HashSet<string>();
            allSWFClassStringList = new HashSet<string>();
        }

        public void Compile(SwfFormat swf)
        {
            _swf = swf;
            CompileBegin();
            int i, j, n, m;

            string name;
            string[] tempName;
            int tempLen;
            for (i = 0, n = _swf.SymbolCount; i < n; ++i)
            {
                SymbolClass symbol = _swf.GetSymbolAt(i);
                SWFSymbol swfSymbol;
                for (j = 0, m = symbol.Symbols.Count; j < m; j++)
                {
                    swfSymbol = symbol.Symbols[j];

                    name = swfSymbol.name;

                    innerFiltersListAdd(name);

                    tempName = name.Split(dotChar);

                    tempLen = tempName.Length;
                    if (tempLen > 1)
                    {
                        int index = name.LastIndexOf('.');
                        string item;
                        string lastTempName;

                        lastTempName = tempName[tempLen - 1];

                        item = "";
                        for (int o = 0,p=tempLen-1; o < p; o++)
                        {
                            string item1 = tempName[o];
                            innerFiltersListAdd(item1);

                            if (item != "")
                            {
                                item = item + "." + item1;
                            }
                            else
                            {
                                item = item1;
                            }
                        }
                        innerFiltersListAdd(item);
                        innerFiltersListAdd(lastTempName);

                    }
                }

            }
            if (null == mapping)
            {
                mapping = new Dictionary<string, byte[]>();
            }
            Abc46 abc;
           
            for (i = 0, n = _swf.AbcCount; i < n; ++i)
            {
                abc = _swf.GetAbcAt(i);

                foreach (InstanceInfo instanceInfo in abc.Instances)
                {
                    ParserInstance(abc,instanceInfo);
                }
                foreach (MethodBodyInfo methodBodyInfo in abc.MethodBodies)
                {
                    ParserBody(abc, methodBodyInfo);
                }

            }


            for (i = 0, n = _swf.AbcCount; i < n; ++i)
            {
                abc = _swf.GetAbcAt(i);

                DoBlurABC(abc, innerFiltersList);
            }

            compileComplete();
        }

        virtual protected void compileComplete()
        {

        }

       

        protected void innerFiltersListAdd(string name)
        {
            if (globalFiltersList.Contains(name))
            {
                return;
            }

            innerFiltersList.Add(name);
        }

        protected void DoBlurABC(Abc46 abc, HashSet<string> innerFiltersList)
        {
            List<StringInfo> stringTable = abc.ConstantPool.StringTable;
            int len = stringTable.Count;

            StringInfo info = null;
            string value=null;

            byte[] filterValue;
            for (int i = 0; i < len; i++) 
            {
                info = stringTable[i];
                value = info.ToString();

       
                filterValue = getFilterCode(value);
                if (filterValue !=null)
                {
                    info.Data = filterValue;
                }
            }

        }


        protected byte[] getFilterCode(string key)
        {

            byte[] filterValue;
            if (mapping.TryGetValue(key, out filterValue))
            {
                return filterValue;
            }

            if (allSWFClassStringList.Contains(key) == false)
            {
                return null;
            }


            int len = key.Length;
            if (len < 2)
            {
                return null;
            }

            if (key.IndexOf('_') > 0)
            {
                //对于有下划线且不在第一个的名称不做混淆;
                return null;
            }

            if (char.IsNumber(key[len - 1]))
            {
                //对于最后一个字符为数字的不做混淆;
                return null;
            }


            if (innerFiltersList.Contains(key) || globalFiltersList.Contains(key))
            {
                //在过滤列表中的不做混淆;
                return null;
            }

            foreach (string item in regexFiltersList)
            {
                if (Regex.IsMatch(key, item))
                {
                    return null;
                }
            }

            filterValue = getCode(key);
            mapping.Add(key, filterValue);

            return filterValue;
        }


       /// <summary>
       /// 取得一个乱码;
       /// </summary>
       /// <param name="key">其实只用于测试的参数</param>
       /// <returns></returns>
        virtual public byte[] getCode(string key)
        {
            StringBuilder sb = new StringBuilder();

            int temp = 0;
            int index = position;
            do
            {
                temp = index;
                index = temp / len;
                sb.Append(codes[temp % len]);
            } while (index > 0);

            position++;

            string value = sb.ToString();
            int n = value.Length;
            byte[] bytes = new byte[n];

            for (int i = 0; i < n; ++i)
            {
                bytes[i] = (byte)value[i];
            }

            return bytes;
        }


        virtual protected void ParserInstance(Abc46 abc, InstanceInfo instanceInfo)
        {
            int index;
            int len;
            U30 u30 = instanceInfo.Name;

            string fullClassName = NameUtil.ResolveMultiname(abc, u30);
            string[] temp = fullClassName.Split(moChar, StringSplitOptions.RemoveEmptyEntries);
            string className = fullClassName;
            if (temp.Length > 0)
            {
                className = temp[temp.Length - 1];
            }

            string lowCaseName= className.ToLower();
            len = className.Length;

            if (filterClasses.Contains(className))
            {
                getRegisterClassAlias(instanceInfo.Name, abc);
            }
            else
            {

                //只要类名后缀为VO的统一加入不混列表中(跟注册了别名类一个样);
                index = lowCaseName.LastIndexOf("vo");
                if (len > 2 && len == index + 2)
                {
                    getRegisterClassAlias(instanceInfo.Name, abc);

                }
                else
                {

                    //只要类名后缀为Define的统一加入不混列表中(跟注册了别名类一个样);
                    index = lowCaseName.LastIndexOf("define");
                    if (len > 6 && len == index + 6)
                    {
                        getRegisterClassAlias(instanceInfo.Name, abc);
                    }
                    else
                    {
                        index = lowCaseName.LastIndexOf("util");
                        if (index > 2)
                        {
                            getRegisterClassAlias(instanceInfo.Name, abc);
                        }
                    }
                }
            }


            GetInstanceALLTraits(fullClassName, abc);
        }

        protected void GetInstanceALLTraits(string fullClassName, Abc46 abc) { 

            ABCGetInstanceProperty instanceProperty = NameUtil.GetInstancePropertys(abc, fullClassName, _swf, false );
            if (instanceProperty == null)
            {
                return;
            }
            abc = instanceProperty.abc;
            string superFullName = NameUtil.ResolveMultiname(abc, instanceProperty.instanceInfo.SuperName);

            if (superFullName != "public::Object")
            {
                GetInstanceALLTraits(superFullName, abc);
            }

            string[] result = fullClassName.Split(moChar, StringSplitOptions.RemoveEmptyEntries);

            //只取类名;
            string className = result[result.Length - 1];

            HashSet<string> addToStringList= allSWFClassStringList;
            if (filterClasses.Contains(className))
            {
                addToStringList = innerFiltersList;
            }

            foreach (string item in result)
            {
                addToStringList.Add(item);
            }

            foreach (string property in instanceProperty.propertys)
            {
                addToStringList.Add(property);
            }
        }

        protected void ParserBody(Abc46 abc, MethodBodyInfo body)
        {
            AVM2Command command;
            AVM2Command preCommand=null;
            bool registerClassAliasCommand=false;
            U30 u30;
            string value;

            byte[] _code = body.Code;

            uint i = 0;
            uint n = (uint)_code.Length;


            List<StringInfo> stringTable = abc.ConstantPool.StringTable;


            HashSet<int> ObjectLocal = new HashSet<int>();
            while (i < n)
            {
                command = Translator.ToCommand(_code[i]);
                command.baseLocation = i++;                
                if (null == command)
                {
                    throw new Exception("Unknown opcode detected.");
                }
                byte OpCode = command.OpCode;

                i += command.ReadParameters(_code, i);
                command.endLocation = i;

                if (null!=preCommand && preCommand.OpCode == Op.Coerce && ( OpCode== Op.SetLocal|| OpCode == Op.SetLocal0 || OpCode == Op.SetLocal1 || OpCode == Op.SetLocal2 || OpCode == Op.SetLocal3))
                {
                    u30 = (U30)preCommand.Parameters[0];
                    string name = NameUtil.ResolveMultiname(abc, u30);
                    if (name == "public::Object")
                    {
                        switch (OpCode)
                        {
                            case Op.SetLocal:
                                if (command.ParameterCount >= 2)
                                {
                                    ObjectLocal.Add((int)((U30)command.Parameters[1]).Value);
                                }
                                else
                                {
                                    ObjectLocal.Add(0);
                                }
                                break;
                            case Op.SetLocal1:
                                ObjectLocal.Add(-1);
                                break;
                            case Op.SetLocal2:
                                ObjectLocal.Add(-2);
                                break;
                            case Op.SetLocal3:
                                ObjectLocal.Add(-3);
                                break;
                            case Op.SetLocal0:
                                ObjectLocal.Add(-4);
                                break;
                        }
                    }
                }
                else if (OpCode==Op.GetProperty)//.操作
                {
                    byte pOpCode = preCommand.OpCode;
                    int idx = -5;
                    switch (pOpCode)
                    {
                        case Op.GetLocal:
                            if (command.ParameterCount >= 1)
                            {
                                idx = (int)((U30)command.Parameters[0]).Value;
                            }
                            else
                            {
                                idx = 0;
                            }
                           break;
                        case Op.GetLocal1:
                            idx = -1;
                            break;
                        case Op.GetLocal2:
                            idx = -2;
                            break;
                        case Op.GetLocal3:
                            idx = -3;
                            break;
                        case Op.GetLocal0:
                            idx = -4;
                            break;
                    }
                    if (idx != -5 && ObjectLocal.Contains(idx))
                    {
                        //说明是使用var a:Object=xxx;  使用了a.bbb,没有使用a["bbb"]处理，需要将 bbb也加入不可混淆的字符串中
                        u30 = (U30)command.Parameters[0];
                        value = abc.ConstantPool.StringTable[(int)(abc.ConstantPool.MultinameTable[(int)u30.Value].Data[0].Value)].ToString();//NameUtil.ResolveMultiname(abc, u30);
                        Console.WriteLine("getProperty"+value);
                        innerFiltersListAdd(value);
                    }
                }



                if (registerClassAliasCommand)
                {
                    registerClassAliasCommand = false;

                    if (OpCode == Op.FindPropStrict || OpCode == Op.GetLex)
                    {
                        u30 = (U30)command.Parameters[0];

                        getRegisterClassAlias(u30, abc);
                    }
                }               
                if (OpCode == Op.PushString)
                {
                    //registerClassAlias("ddsss",B);会变成如下指令
                    //5d 01 
                    //_as3_findpropstrict flash.net::registerClassAlias
                    //2c 09 
                    //_as3_pushstring "ddsss"
                    if (null != preCommand && preCommand.OpCode == Op.FindPropStrict)
                    {
                        u30 = (U30)preCommand.Parameters[0];

                        if (NameUtil.ResolveMultiname(abc, u30) == registerClassAliasKey)
                        {
                            registerClassAliasCommand = true;
                        }
                    }

                    u30 = (U30)command.Parameters[0];
                    value = stringTable[(int)u30.Value].ToString();
                    addPushString(value);
                }else if (OpCode == Op.DebugFile)
                {
                    u30 = (U30)command.Parameters[0];
                    value = stringTable[(int)u30.Value].ToString();
                    innerFiltersListAdd(value);
                }

                preCommand = command;
            }

        }

        
        protected void addPushString(string value)
        {
            int index;
            int len;
            foreach (string item in classDefineByStringChar)
            {
                len = item.Length;
                index = value.LastIndexOf(item);

                if (index > 1 && index != value.Length - 1)
                {
                    string mabeNameSpace = value.Substring(0, index);
                    innerFiltersListAdd(mabeNameSpace);
                    mabeNameSpace = value.Substring(index + len);
                    innerFiltersListAdd(mabeNameSpace);
                    break;
                }
            }

            innerFiltersListAdd(value);
        }


        protected void getRegisterClassAlias(U30 u30, Abc46 abc)
        {
            string fullName;

            fullName = NameUtil.ResolveMultiname(abc, u30);

            if (registerClassAliasMapping.Contains(fullName))
            {
                return;
            }
            //即便没有属性，也将类名加入到过滤列表
            string[] result = fullName.Split(moChar, StringSplitOptions.RemoveEmptyEntries);

            if (result.Length > 0)
            {
                innerFiltersListAdd(result[result.Length - 1]);
            }

            ABCGetInstanceProperty instanceProperty = NameUtil.GetInstancePropertys(abc,fullName,_swf);

            if(instanceProperty == null)
            {
                return;
            }
            registerClassAliasMapping.Add(fullName);
            abc = instanceProperty.abc;
            InstanceInfo instanceInfo = instanceProperty.instanceInfo;


            string superFullName=NameUtil.ResolveMultiname(abc, instanceInfo.SuperName);

            if(superFullName != "public::Object")
            {
                getRegisterClassAlias(instanceInfo.SuperName, abc);
            }
           
           

            List<string> propertys = instanceProperty.propertys;
#if DEBUG
            Console.WriteLine("{0} \t {1}", fullName, propertys.Count);
#endif

            foreach (string property in propertys)
            {
                innerFiltersListAdd(property);
            }

        }
    }
}
