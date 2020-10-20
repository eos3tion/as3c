using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SwfLibrary.Abc.Constants;
using SwfLibrary.Types;
using SwfLibrary.Utils;
using SwfLibrary.Exceptions;

namespace SwfLibrary.Abc
{
    public class ConstantPool : IExternalizeable
    {
        #region ConstantPool Members

        protected List<S32> _intTable;
        protected List<U32> _uintTable;
        protected List<double> _doubleTable;
        protected List<StringInfo> _stringTable;
        protected List<NamespaceInfo> _namespaceTable;
        protected List<NamespaceSetInfo> _nsTable;
        protected List<MultinameInfo> _multinameTable;

        public List<S32> IntTable
        {
            get { return _intTable; }
            set { _intTable = value; }
        }

        public int ResolveInt(S32 value)
        {
            for (int i = 1; i < _intTable.Count; ++i)
            {
                if (value.Value == ((S32)_intTable[i]).Value)
                    return i;
            }

            int j = _intTable.Count;

            _intTable.Add(value);

            return j;
        }

        public List<U32> UIntTable
        {
            get { return _uintTable; }
            set { _uintTable = value; }
        }

        public int ResolveUInt(U32 value)
        {
            for (int i = 1; i < _uintTable.Count; ++i)
            {
                if (value.Value == ((U32)_uintTable[i]).Value)
                    return i;
            }

            int j = _uintTable.Count;

            _uintTable.Add(value);

            return j;
        }

        public List<double> DoubleTable
        {
            get { return _doubleTable; }
            set { _doubleTable = value; }
        }

        public int ResolveDouble(double value)
        {
            for (int i = 1; i < _doubleTable.Count; ++i)
            {
                if (value == ((double)_doubleTable[i]))
                    return i;
            }

            int j = _doubleTable.Count;

            _doubleTable.Add(value);

            return j;
        }

        public List<StringInfo> StringTable
        {
            get { return _stringTable; }
            set { _stringTable = value; }
        }

        public int ResolveString(string value)
        {
            StringInfo stringInfo;
            bool equal;

            for (int i = 1; i < _stringTable.Count; ++i)
            {
                stringInfo = _stringTable[i];

                if (0 == stringInfo.Data.Length && 0 < value.Length)
                    continue;

                equal = true;

                for (int j = 0; j < stringInfo.Data.Length; ++j)
                {
                    if (j == value.Length)
                    {
                        equal = false;
                        break;
                    }

                    if ((byte)value[j] != stringInfo.Data[j])
                    {
                        equal = false;
                        break;
                    }
                }

                if (equal)
                    return i;
            }

            int k = _stringTable.Count;

            stringInfo = new StringInfo();

            char[] x = value.ToCharArray();
            int n = x.Length;

            stringInfo.Data = new byte[n];

            for (int i = 0; i < n; ++i)
            {
                stringInfo.Data[i] = (byte)x[i];
            }

            _stringTable.Add(stringInfo);

            return k;
        }

        public List<NamespaceInfo> NamespaceTable
        {
            get { return _namespaceTable; }
            set { _namespaceTable = value; }
        }

        public List<NamespaceSetInfo> NamespaceSetTable
        {
            get { return _nsTable; }
            set { _nsTable = value; }
        }

        public List<MultinameInfo> MultinameTable
        {
            get { return _multinameTable; }
            set { _multinameTable = value; }
        }

        #endregion

        #region IExternalizeable Members

        public void ReadExternal(BinaryReader input)
        {
            #region integer

            uint n = Primitives.ReadU30(input).Value;
            
            _intTable = new List<S32>(Capacity.Max(n));
            _intTable.Add(new S32());
            
            for (uint i = 1; i < n; ++i)
                _intTable.Add(Primitives.ReadS32(input));

            #endregion

            #region uinteger

            n = Primitives.ReadU30(input).Value;

            _uintTable = new List<U32>(Capacity.Max(n));
            _uintTable.Add(new U32());

            for (uint i = 1; i < n; ++i)
                _uintTable.Add(Primitives.ReadU32(input));

            #endregion

            #region double

            n = Primitives.ReadU30(input).Value;

            _doubleTable = new List<double>(Capacity.Max(n));
            _doubleTable.Add(double.NaN);

            for (uint i = 1; i < n; ++i)
                _doubleTable.Add(input.ReadDouble());

            #endregion

            #region string_info

            n = Primitives.ReadU30(input).Value;

            _stringTable = new List<StringInfo>(Capacity.Max(n));
            _stringTable.Add(new StringInfo());

            for (uint i = 1; i < n; ++i)
            {
                StringInfo stringInfo = new StringInfo();
                stringInfo.ReadExternal(input);

                _stringTable.Add(stringInfo);
            }

            #endregion

            #region namespace_info

            n = Primitives.ReadU30(input).Value;

            _namespaceTable = new List<NamespaceInfo>(Capacity.Max(n));
            _namespaceTable.Add(new NamespaceInfo());

            for (uint i = 1; i < n; ++i)
            {
                NamespaceInfo namespaceInfo = new NamespaceInfo();
                namespaceInfo.ReadExternal(input);

                _namespaceTable.Add(namespaceInfo);
            }

            #endregion

            #region ns_set_info

            n = Primitives.ReadU30(input).Value;

            _nsTable = new List<NamespaceSetInfo>(Capacity.Max(n));
            _nsTable.Add(new NamespaceSetInfo());

            for (uint i = 1; i < n; ++i)
            {
                NamespaceSetInfo nsInfo = new NamespaceSetInfo();
                nsInfo.ReadExternal(input);

                _nsTable.Add(nsInfo);
            }

            #endregion

            #region multiname_info

            n = Primitives.ReadU30(input).Value;

            _multinameTable = new List<MultinameInfo>(Capacity.Max(n));
            _multinameTable.Add(new MultinameInfo());

            for (uint i = 1; i < n; ++i)
            {
                MultinameInfo multinameInfo = new MultinameInfo();
                multinameInfo.ReadExternal(input);

                _multinameTable.Add(multinameInfo);
            }

            #endregion
        }

        public void WriteExternal(BinaryWriter output)
        {
            #region integer

            int n = GetCount(_intTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                Primitives.WriteS32(output, _intTable[i]);
            }
            #endregion

            #region uinteger

            n = GetCount(_uintTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                Primitives.WriteU32(output, _uintTable[i]);
            }
            #endregion

            #region double

            n = GetCount(_doubleTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                output.Write(_doubleTable[i]);
            }
            #endregion

            #region string_info

            n = GetCount(_stringTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                _stringTable[i].WriteExternal(output);
            }
            #endregion

            #region namespace_info

            n = GetCount(_namespaceTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
                _namespaceTable[i].WriteExternal(output);

            #endregion

            #region ns_set_info

            n = GetCount(_nsTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                _nsTable[i].WriteExternal(output);
            }
            #endregion

            #region multiname_info

            n = GetCount(_multinameTable);

            Primitives.WriteU30(output, (uint)n);

            for (int i = 1; i < n; ++i)
            {
                _multinameTable[i].WriteExternal(output);
            }
            #endregion
        }

        #endregion

        protected int GetCount(IList table)
        {
            if (1 == table.Count) return 0;
            return table.Count;
        }


        public object getConstantValue(byte type, int index)
        {
            switch (type)
            {

                case 0x03 /* int */: return _intTable[index];
                case 0x04 /* uint */: return _uintTable[index];
                case 0x06 /* double */: return _doubleTable[index];
                case 0x01 /* UTF-8 */: return _stringTable[index];
                case 0x0B /* true */: return true;
                case 0x0A /* false */: return false;

                case 0x0C /* null */:                           // Cascade
                case 0x00 /* undefined */:
                    return null;

                case 0x08 /* namespace */:                      // Cascade
                case 0x16 /* package namespace */:
                case 0x17 /* package internal namespace */:
                case 0x18 /* protected namespace */:
                case 0x19 /* explicit namespace */:
                case 0x1A /* static protected namespace */:
                case 0x05 /* private namespace */:
                    return _namespaceTable[index];

                default:
                    throw new Exception("Unknown parameter type.");
            }
        }
		

    }
}
