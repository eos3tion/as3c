using SwfLibrary.Abc;
using SwfLibrary.Abc.Utils;
using SwfLibrary.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace As3c.Compiler
{
    /// <summary>
    /// 君游编译用
    /// </summary>
    public class CompilerBlurJY : CompilerBlur
    {
        /// <summary>
        /// 用于生成字典的后缀
        /// </summary>
        protected const string EXTENION_DIC = ".dic";

        /// <summary>
        /// 用于生成innerList的后缀
        /// </summary>
        protected const string EXTENION_LIST = ".lst";

        /// <summary>
        /// 用于生成Main的ClassAlias
        /// </summary>
        protected const string EXTENION_CLASSALIAS = ".ali";

        /// <summary>
        /// allSWFClassStringList
        /// </summary>
        protected const string EXTENION_CLASS = ".scl";

        public CompilerBlurJY(string[] args=null) : base(args) { }


        protected override void ParserInstance(Abc46 abc, InstanceInfo instanceInfo)
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

            string lowCaseName = className.ToLower();
            len = className.Length;
            String flag = "<NO>";
            if (filterClasses.Contains(className))
            {
                getRegisterClassAlias(instanceInfo.Name, abc);
                flag = "filterClasses";
            }
            else
            {

                //只要类名后缀为VO的统一加入不混列表中(跟注册了别名类一个样);
                index = lowCaseName.LastIndexOf("vo");
                if (len > 2 && len == index + 2)
                {
                    getRegisterClassAlias(instanceInfo.Name, abc);
                    flag = "vo";
                }
                else
                {

                    //只要类名后缀为Define的统一加入不混列表中(跟注册了别名类一个样);
                    index = lowCaseName.LastIndexOf("define");
                    if (len > 6 && len == index + 6)
                    {
                        getRegisterClassAlias(instanceInfo.Name, abc);
                        flag = "define";
                    }
                    else
                    {
                        index = lowCaseName.LastIndexOf("util");
                        if (index > 2)
                        {
                            getRegisterClassAlias(instanceInfo.Name, abc);
                            flag = "util";
                        }
                        else
                        {
                            //处理配置数据
                            index = lowCaseName.LastIndexOf("cfg");
                            if (len > 3 && len == index + 3)
                            {
                                getRegisterClassAlias(instanceInfo.Name, abc);
                            }
                            flag = "cfg";
                        }
                    }
                    index = lowCaseName.LastIndexOf("yyhd");
                    if (len>4&&len == index + 4)
                    {
                        getRegisterClassAlias(instanceInfo.Name, abc);
                        flag = "yyhd";
                    }
                    
                }
            }
           // Console.WriteLine(flag + ":" + lowCaseName);

            GetInstanceALLTraits(fullClassName, abc);
        }

        public override byte[] getCode(string str)
        {
            int slen = str.Length;
           
            uint seed = 131; // 31 131 1313 13131 131313 etc..
            // BKDR Hash Function
            uint hash = 0;
            // AP Hash Function
            uint hash1 = 0;
            int i;
            byte cha;
            for (i = 0; i < slen; i++)
            {
                cha = (byte)str[i];
                hash = hash * seed + (cha);
                if ((i & 1) == 0)
                {
                    hash1 ^= ((hash1 << 7) ^ (cha) ^ (hash >> 3));
                }
                else
                {
                    hash1 ^= (~((hash1 << 11) ^ (cha) ^ (hash >> 5)));
                }
            }

            byte[] bytes = new byte[6];
            bytes[0]=(byte)(hash>>24 & 0xff);
            bytes[1]=(byte)(hash>>16 & 0xff);
            bytes[2]=(byte)(hash>>8 & 0xff);
            bytes[3]=(byte)(hash & 0xff);
            bytes[4] = (byte)(hash1 >> 24 & 0xff);
            bytes[5] = (byte)(hash1 >> 16 & 0xff);
            //bytes[6] = (byte)(hash1 >> 8 & 0xff);
            //bytes[7] = (byte)(hash1 & 0xff);
            byte[] bytes64 = System.Text.Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes));
            return bytes64;
        }
    }

    /// <summary>
    /// 混淆程序主文件
    /// </summary>
    public class CompilerBlurJYMain : CompilerBlurJY
    {
        private string _dicPath;

        private string _listPath;

        private string _aliPath;

        private string _sclPath;

        public CompilerBlurJYMain(string[] args=null):base(args)
        {
           
        }

        /// <summary>
        /// 将以原始字符串为key<br/>
        /// 混淆后字符串的byte为Value的字典写入文件
        /// </summary>
        protected void writeFile(String path,object obj)
        {
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, obj);
            fs.Close();
        }

        override protected bool parseArgs(string path)
        {
            string extension = Path.GetExtension(path);
            if (extension == EXTENION_DIC)
            {
                _dicPath = path;
                return true;
            }
            if (extension == EXTENION_LIST)
            {
                _listPath = path;
                return true;
            }
            if (extension == EXTENION_CLASSALIAS)
            {
                _aliPath = path;
                return true;
            }
            if (extension == EXTENION_CLASS)
            {
                _sclPath = path;
                return true;
            }
            return false;
        }

        override protected void compileComplete()
        {
            if (null != _dicPath)
            {
                writeFile(_dicPath,mapping);
            }
            if (null != _listPath)
            {
                writeFile(_listPath,innerFiltersList);
            }
            if (null != _aliPath)
            {
                writeFile(_aliPath, registerClassAliasMapping);
            }
            if (null != _sclPath)
            {
                writeFile(_sclPath, allSWFClassStringList);
            }
        }


       
    }

    /// <summary>
    /// 混淆插件
    /// </summary>
    public class CompilerBlurJYPlugin : CompilerBlurJY
    {

        public CompilerBlurJYPlugin(string[] args)
            : base(args)
        {

        }
  

        override protected bool parseArgs(string path)
        {
            if (File.Exists(path) == false)
            {
                return true;
            }

            FileInfo fileInfo = new FileInfo(path);
            string extension = fileInfo.Extension;
            if (extension == EXTENION_DIC)
            {
                mapping = (Dictionary<String, byte[]>)readFile(path);
                return true;
            }
            if (extension == EXTENION_LIST)
            {
                _mainInnerList = (HashSet<string>)readFile(path);
                return true;
            }
            if (extension == EXTENION_CLASSALIAS)
            {
                _mainClassAlias = (HashSet<string>)readFile(path);
                return true;
            }
            if (extension == EXTENION_CLASS)
            {
                _mainSWFClass = (HashSet<string>)readFile(path);
                return true;
            }
            return false;
        }

        protected object readFile(string path)
        {
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read);
            IFormatter formatter = new BinaryFormatter();
            object obj = formatter.Deserialize(fs);
            fs.Close();
            return obj;
        }

        protected override void CompileBegin()
        {
            base.CompileBegin();
            if (null != _mainInnerList)
            {
                foreach (string str in _mainInnerList)
                {
                    innerFiltersList.Add(str);
                }
            }

            if (null != _mainClassAlias)
            {
                foreach (string str in _mainClassAlias)
                {
                    registerClassAliasMapping.Add(str);
                }
            }

            if (null != _mainSWFClass)
            {
                foreach (string str in _mainSWFClass)
                {
                    allSWFClassStringList.Add(str);
                }
            }
        }

        private HashSet<string> _mainSWFClass;

        private HashSet<string> _mainClassAlias;

        private HashSet<string> _mainInnerList;
    }
   

}
