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
using SwfLibrary.Abc.Constants;
using SwfLibrary.Exceptions;
using SwfLibrary.Utils;
using System.Collections;
using SwfLibrary.Abc.Traits;
using SwfLibrary.Types;

namespace SwfLibrary.Abc.Utils
{
    /// <summary>
    /// NameUtil is a utility class that helps creating and/or resolving Multiname
    /// structures.
    /// 
    /// TODO: The resolving of Multinames is completly wrong. This means their string
    /// representation is wrong, therefore also the whole syntax gets wrong.
    /// 
    /// Some proper error checking would be nice too.
    /// </summary>
    public class NameUtil
    {
        public static string ResolveMultiname(Abc46 abc, U30 index)
        {
            return ResolveMultiname(abc, (MultinameInfo)abc.ConstantPool.MultinameTable[(int)index.Value]);
        }

        public static string ResolveMultiname(Abc46 abc, int index)
        {
            return ResolveMultiname(abc, (MultinameInfo)abc.ConstantPool.MultinameTable[index]);
        }

        /**
         * HERE STARTS THE SPAGHETTI CODE
         */
        public static string ResolveMultiname(Abc46 abc, MultinameInfo multiName)
        {
            NamespaceInfo ns;
            NamespaceSetInfo nss;
            StringInfo name;
            string result="";

            switch (multiName.Kind)
            {
                case MultinameInfo.RTQName:
                case MultinameInfo.RTQNameA:
                    name = abc.ConstantPool.StringTable[(int)multiName.Data[0].Value];
                    result = name.ToString();
                    break;

                case MultinameInfo.QName:
                case MultinameInfo.QNameA:
                    ns = ((NamespaceInfo)abc.ConstantPool.NamespaceTable[(int)multiName.Data[0].Value]);
                    name = ((StringInfo)abc.ConstantPool.StringTable[(int)multiName.Data[1].Value]);
                    result= getNameSpaceString(abc,ns) + "::" + name.ToString();
                    break;

                case MultinameInfo.RTQNameL:
                case MultinameInfo.RTQNameLA:
                    Console.WriteLine("[-] RTQNameL/RTQameLA is currently not supported.");
                    result="";
                    break;

                case MultinameInfo.Multiname_:
                case MultinameInfo.MultinameA:
                    name = abc.ConstantPool.StringTable[(int)multiName.Data[0].Value];
                    nss = abc.ConstantPool.NamespaceSetTable[(int)multiName.Data[1].Value];
                    result= getNamespaceSetString(abc, nss, true) + "::" + name.ToString();
                    break;

                case MultinameInfo.MultinameL:
                case MultinameInfo.MultinameLA:
                    nss = abc.ConstantPool.NamespaceSetTable[(int)multiName.Data[0].Value];
                    result= getNamespaceSetString(abc, nss);
                    break;

                default:
                    result="*";
                    break;

            }

            return result;
        }

        private static string getNamespaceSetString(Abc46 abc,NamespaceSetInfo nss, bool onlyLast=false)
        {
            ConstantPool pool = abc.ConstantPool;
            NamespaceInfo ns;
            string name;
            string lastName="";
            string set = "";
          
            for (int i = 0, n = nss.NamespaceSet.Count; i < n; ++i)
            {
                U30 nssNs = (U30)nss.NamespaceSet[i];
                ns = pool.NamespaceTable[(int)nssNs.Value];

                name=getNameSpaceString(abc,ns);
                if (name !="")
                {
                    set += name;

                    if (i != n - 1)
                    {
                        set += ", ";
                    }

                    lastName = name;
                }
            }

            if (onlyLast)
            {
                return lastName;
            }

            return "["+set+"]";
        }


        /// <summary>
        /// 取得namespace名称
        /// </summary>
        /// <param name="abc"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        private static string getNameSpaceString(Abc46 abc, NamespaceInfo ns)
        {
            string result = "";
            int index = (int)ns.Name.Value;
            if (index != 0)
            {
                result = abc.ConstantPool.StringTable[index].ToString();
                string kind = getNameSpaceKindString(abc, ns);
                if (result == "")
                {
                    result = kind;
                }
                else
                {
                    result = kind + "::" + result;
                }
            }

            return result;
        }

        /// <summary>
        ///  取得namespace访问符
        /// </summary>
        /// <param name="abc"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        private static string getNameSpaceKindString(Abc46 abc, NamespaceInfo ns)
        {
            string result = "";
            switch (ns.Kind)
            {
                case NamespaceInfo.Namespace:
                    result = "namespace";
                    break;
                case NamespaceInfo.ExplicitNamespace:
                    result = "explicit";
                    break;
                case NamespaceInfo.PrivateNs:
                    result = "private";
                    break;
                case NamespaceInfo.ProtectedNamespace:
                    result = "protected";
                    break;
                case NamespaceInfo.StaticProtectedNs:
                    result = "protected$";
                    break;
                case NamespaceInfo.PackageInternalNs:
                    result = "internal";
                    break;
                case NamespaceInfo.PackageNamespace:
                    result = "public";
                    break;
                default:
                    result = "*";
                    break;
            }

            return result;
        }

        public static string ResolveClass(Abc46 abc, InstanceInfo info)
        {
            return ResolveMultiname(abc, abc.ConstantPool.MultinameTable[(int)info.Name.Value]);
        }

        public static U30 GetMultiname(Abc46 abc, string argument)
        {
            ConstantPool pool = abc.ConstantPool;

            if (argument.StartsWith("#"))
            {
                int index = int.Parse(argument.Substring(1, argument.Length - 1));

                if (index < pool.MultinameTable.Count && index > 0)
                {
                    return (U30)index;
                }

                throw new Exception(String.Format("Invalid multiname {0}", index));
            }

            NamespaceInfo ns;
            NamespaceSetInfo nss;
            StringInfo name;

            bool skipQname = argument.IndexOf("[") == 0;

            if (skipQname)
            {
                //BAD quick dirty hack
                argument = argument.Replace(" ", "");
            }

            string tempName;
            MultinameInfo multiName;
            U30 result = new U30();

            for (int i = 1, n = pool.MultinameTable.Count; i < n; ++i)
            {
                multiName = pool.MultinameTable[i];

                switch (multiName.Kind)
                {
                    #region QName, QNameA

                    case MultinameInfo.QName:
                    case MultinameInfo.QNameA:

                        if (!skipQname)
                        {
                            continue;
                        }
                        ns = pool.NamespaceTable[(int)multiName.Data[0].Value];
                        name = pool.StringTable[(int)multiName.Data[1].Value];

                        tempName = getNameSpaceString(abc,ns) + "::" + name.ToString();

                        if (tempName == argument)
                        {
                            result.Value = (uint)i;
                            return result;
                        }
                        break;

                    #endregion

                    #region MultinameL, MultinameLA

                    case MultinameInfo.MultinameL:
                    case MultinameInfo.MultinameLA:

                        if (!skipQname)
                        {
                            continue;
                        }
                       
                        nss = pool.NamespaceSetTable[(int)multiName.Data[0].Value];
                        tempName=getNamespaceSetString(abc, nss);

                        if (argument == tempName)
                        {
                            result.Value = (uint)i;
                            return result;
                        }
                        break;

                    #endregion

                    default:
                        continue;
                }
            }

            if (skipQname)
            {
                #region Create MultinameL

                // Remove [] from argument
                argument = argument.Substring(1, argument.Length - 2);


                // Get new NamespaceSet index
                U30 setIndex = new U30();
                setIndex.Value = (uint)pool.NamespaceSetTable.Count;


                // Create MultinameInfo
                MultinameInfo newName = new MultinameInfo();
                newName.Data = new U30[1] { setIndex };
                newName.Kind = MultinameInfo.MultinameL;


                // Create NamespaceSet
                NamespaceSetInfo newSet = new NamespaceSetInfo();
                newSet.NamespaceSet = new ArrayList();

                pool.NamespaceSetTable.Add(newSet);

                for (int i = 0, n = pool.NamespaceTable.Count; i < n; ++i)
                {
                    ns = pool.NamespaceTable[i];

                    string r2 = getNameSpaceString(abc,ns);

                    if (argument.IndexOf(r2) != -1)
                    {
                        U30 nsIndex = new U30();
                        nsIndex.Value = (uint)i;

                        newSet.NamespaceSet.Add(nsIndex);
                    }
                }

                result.Value = (uint)pool.MultinameTable.Count;
                pool.MultinameTable.Add(newName);

                #endregion
            }
            else
            {
                #region Create QName

                U30 nsIndex = new U30();

                int lastIndex = argument.LastIndexOf("::");
                string nameString = null;
                if (argument.IndexOf("::") == lastIndex)
                {
                    nameString = argument.Substring(0, lastIndex + 2);
                    nsIndex.Value = GetNamespace(abc, nameString).Value;
                }
                else
                {
                    nameString = argument.Substring(0, lastIndex);
                    nsIndex.Value = GetNamespace(abc, nameString).Value;
                }

                nameString = argument.Substring(lastIndex + 2);

                MultinameInfo newQName = new MultinameInfo();
                newQName.Data = new U30[2] { nsIndex, (U30)pool.ResolveString(nameString) };
                newQName.Kind = MultinameInfo.QName;

                result.Value = (uint)pool.MultinameTable.Count;

                pool.MultinameTable.Add(newQName);

                #endregion
            }

            return result;
        }

        

        public static U30 GetNamespace(Abc46 abc, string argument)
        {
            NamespaceInfo ns;
            ConstantPool pool = abc.ConstantPool;

            for (int i = 0, n = pool.NamespaceTable.Count; i < n; ++i)
            {
                ns = pool.NamespaceTable[i];

                string namespaceString = getNameSpaceString(abc, ns);

                if (argument == namespaceString)
                {
                    U30 nsIndex = new U30();
                    nsIndex.Value = (uint)i;

                    return nsIndex;
                }
            }

            NamespaceInfo newNs = new NamespaceInfo();

            string name = "";
            byte kind = 0;

            if (0 == argument.IndexOf("private::"))
            {
                kind = NamespaceInfo.PrivateNs;
                name = argument.Substring(8);
            }
            else if (0 == argument.IndexOf("public::"))
            {
                kind = NamespaceInfo.PackageNamespace;
                name = argument.Substring(7);
            }

            newNs.Kind = kind;

            if ("" == name)
            {
                newNs.Name = (U30)0;
            }
            else
            {
                newNs.Name = (U30)pool.ResolveString(name);
            }

            U30 result = new U30();

            result.Value = (uint)pool.NamespaceTable.Count;

            pool.NamespaceTable.Add(newNs);

            return result;
        }

        public static U30 GetClass(Abc46 abc, string argument)
        {
            U30 multinameIndex = GetMultiname(abc, argument);
            U30 result = new U30();

            for (int i = 0, n = abc.Instances.Count; i < n; ++i)
            {
                InstanceInfo ii = (InstanceInfo)abc.Instances[i];

                if (ii.Name.Value == multinameIndex.Value)
                {
                    result.Value = (uint)i;
                    break;
                }
            }

            return result;
        }


        public static InstanceInfo GetInstanceInfo(Abc46 abc, U30 multinameIndex)
         {

            InstanceInfo result = null;
            InstanceInfo temp;
            for (int i = 0, n = abc.Instances.Count; i < n; ++i)
            {
                temp = (InstanceInfo)abc.Instances[i];

                if (temp.Name.Value == multinameIndex.Value)
                {
                    result = temp;
                }
            }

            return result;
        }

        public static InstanceInfo GetInstanceInfo(Abc46 abc, string fullName)
        {

            InstanceInfo result = null;
            InstanceInfo temp;

            string tempName;
            for (int i = 0, n = abc.Instances.Count; i < n; ++i)
            {
                temp = (InstanceInfo)abc.Instances[i];

                tempName=temp._fullName;

                if (tempName==null)
                {
                    tempName= temp._fullName = ResolveMultiname(abc, temp.Name);
                }

                if (tempName == fullName)
                {
                    result = temp;
                }
            }

            return result;
        }


        public static ABCGetInstanceProperty GetInstancePropertys(Abc46 abc, string fullName, SwfFormat swf=null, bool isOnlyPublicProperty = true)
        {
            ABCGetInstanceProperty getInstanceProperty = null;
            InstanceInfo instanceInfo= GetInstanceInfo(abc, fullName);
            
            if (instanceInfo == null && swf !=null)
            {
                for (int i = 0; i < swf.AbcCount; i++)
                {
                    abc = swf.GetAbcAt(i);

                    instanceInfo = GetInstanceInfo(abc, fullName);
                    if (instanceInfo != null)
                    {
                        break;
                    }
                }
            }

            if (instanceInfo != null)
            {
                List<string> propertys=GetInstanceProperty(abc, instanceInfo,isOnlyPublicProperty);
                if (propertys.Count > 0)
                {
                    getInstanceProperty = new ABCGetInstanceProperty();
                    getInstanceProperty.propertys = propertys;
                    getInstanceProperty.abc = abc;
                    getInstanceProperty.instanceInfo = instanceInfo;
                }
            }

            return getInstanceProperty;
        }

        private static List<string> GetInstanceProperty(Abc46 abc, InstanceInfo instanceInf, bool isOnlyPublicProperty = true)
        {
            List<string> result = new List<string>();

            TraitBody body;
            int index;
            string fullName = null;

            ConstantPool pool = abc.ConstantPool;
            List<StringInfo> stringTable = pool.StringTable;

            if (isOnlyPublicProperty)
            {

                foreach (TraitInfo item in instanceInf.Traits)
                {
                    body = item.Body;

                    if (body is TraitSlot || body is TraitGetter || body is TraitGetter)
                    {
                        fullName = NameUtil.ResolveMultiname(abc, item.Name);
                        if (string.IsNullOrEmpty(fullName))
                        {
                            continue;
                        }
                        index = fullName.LastIndexOf("::");

                        if (index != -1)
                        {
                            fullName = fullName.Substring(index + 2);
                        }
                        result.Add(fullName);
                    }
                }

                return result;
            }

            foreach (TraitInfo item in instanceInf.Traits)
            {
                body = item.Body;

                fullName = NameUtil.ResolveMultiname(abc, item.Name);
                if (string.IsNullOrEmpty(fullName))
                {
                    continue;
                }
                index = fullName.LastIndexOf("::");

                if (index != -1)
                {
                    fullName = fullName.Substring(index + 2);
                }
                result.Add(fullName);
            }

            ClassInfo classInfo = instanceInf.classInfo;
            if (classInfo !=null)
            {
                foreach (TraitInfo item in classInfo.Traits)
                {
                    body = item.Body;

                    fullName = NameUtil.ResolveMultiname(abc, item.Name);
                    if (string.IsNullOrEmpty(fullName))
                    {
                        continue;
                    }
                    index = fullName.LastIndexOf("::");

                    if (index != -1)
                    {
                        fullName = fullName.Substring(index + 2);
                    }
                    result.Add(fullName);
                }
            }


            return result;
        }

    }
}
