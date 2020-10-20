using System;
using System.Collections.Generic;
using System.Text;

namespace as3c.As3c.Common
{
    public class Op
    {
        public const byte MIN_VALUE = 0x00;
        public const byte MAX_VALUE = 0xFF;

        public const byte Breakpoint = 0x01;
        public const byte Nop = 0x02;
        public const byte Throw = 0x03;
        public const byte GetSuper = 0x04;
        public const byte SetSuper = 0x05;
        public const byte DefaultXmlNamespace = 0x06;
        public const byte DefaultXmlNamespaceL = 0x07;
        public const byte Kill = 0x08;
        public const byte Label = 0x09;
        public const byte IfNotLessThan = 0x0C;
        public const byte IfNotLessEqual = 0x0D;
        public const byte IfNotGreaterThan = 0x0E;
        public const byte IfNotGreaterEqual = 0x0F;
        public const byte Jump = 0x10;
        public const byte IfTrue = 0x11;
        public const byte IfFalse = 0x12;
        public const byte IfEqual = 0x13;
        public const byte IfNotEqual = 0x14;
        public const byte IfLessThan = 0x15;
        public const byte IfLessEqual = 0x16;
        public const byte IfGreaterThan = 0x17;
        public const byte IfGreaterEqual = 0x18;
        public const byte IfStrictEqual = 0x19;
        public const byte IfStrictNotEqual = 0x1A;
        public const byte LookupSwitch = 0x1B;
        public const byte PushWith = 0x1C;
        public const byte PopScope = 0x1D;
        public const byte NextName = 0x1E;
        public const byte HasNext = 0x1F;
        public const byte PushNull = 0x20;
        public const byte PushUndefined = 0x21;
        public const byte NextValue = 0x23;
        public const byte PushByte = 0x24;
        public const byte PushShort = 0x25;
        public const byte PushTrue = 0x26;
        public const byte PushFalse = 0x27;
        public const byte PushNaN = 0x28;
        public const byte Pop = 0x29;
        public const byte Dup = 0x2A;
        public const byte Swap = 0x2B;
        public const byte PushString = 0x2C;
        public const byte PushInt = 0x2D;
        public const byte PushUInt = 0x2E;
        public const byte PushDouble = 0x2F;
        public const byte PushScope = 0x30;
        public const byte PushNamespace = 0x31;
        public const byte HasNext2 = 0x32;

        public const byte PushDecimal = 0x33;	// NEW: PushDecimal according to FlexSDK, lix8 according to Tamarin
        public const byte PushDNaN = 0x34;		// NEW: PushDNaN according to Flex SDK, lix16 according to Tamarin

        public const byte NewFunction = 0x40;
        public const byte Call = 0x41;
        public const byte Construct = 0x42;
        public const byte CallMethod = 0x43;
        public const byte CallStatic = 0x44;
        public const byte CallSuper = 0x45;
        public const byte CallProperty = 0x46;
        public const byte ReturnVoid = 0x47;
        public const byte ReturnValue = 0x48;
        public const byte ConstructSuper = 0x49;
        public const byte ConstructProp = 0x4A;
        public const byte CallSuperId = 0x4B;	// NOT HANDLED
        public const byte CallPropLex = 0x4C;
        public const byte CallInterface = 0x4D;	// NOT HANDLED
        public const byte CallSuperVoid = 0x4E;
        public const byte CallPropVoid = 0x4F;
        public const byte ApplyType = 0x53;
        public const byte NewObject = 0x55;
        public const byte NewArray = 0x56;
        public const byte NewActivation = 0x57;
        public const byte NewClass = 0x58;
        public const byte GetDescendants = 0x59;
        public const byte NewCatch = 0x5A;

        //		public const byte FindPropGlobalStrict= 0x5B;	// NEW from Tamarin (internal)
        //		public const byte FindPropGlobal= 0x5C;			// NEW from Tamarin (internal)

        public const byte FindPropStrict = 0x5D;
        public const byte FindProperty = 0x5E;
        public const byte FindDef = 0x5F;		// NOT HANDLED
        public const byte GetLex = 0x60;
        public const byte SetProperty = 0x61;
        public const byte GetLocal = 0x62;
        public const byte SetLocal = 0x63;
        public const byte GetGlobalScope = 0x64;
        public const byte GetScopeObject = 0x65;
        public const byte GetProperty = 0x66;
        public const byte GetPropertyLate = 0x67;
        public const byte InitProperty = 0x68;
        public const byte SetPropertyLate = 0x69;
        public const byte DeleteProperty = 0x6A;
        public const byte DeletePropertyLate = 0x6B;
        public const byte GetSlot = 0x6C;
        public const byte SetSlot = 0x6D;
        public const byte GetGlobalSlot = 0x6E;
        public const byte SetGlobalSlot = 0x6F;
        public const byte ConvertString = 0x70;
        public const byte EscXmlElem = 0x71;
        public const byte EscXmlAttr = 0x72;
        public const byte ConvertInt = 0x73;
        public const byte ConvertUInt = 0x74;
        public const byte ConvertDouble = 0x75;
        public const byte ConvertBoolean = 0x76;
        public const byte ConvertObject = 0x77;
        public const byte CheckFilter = 0x78;
        // 0x79 convert_m
        // 0x7A convert_m_p
        public const byte Coerce = 0x80;
        public const byte CoerceBoolean = 0x81;
        public const byte CoerceAny = 0x82;
        public const byte CoerceInt = 0x83;
        public const byte CoerceDouble = 0x84;
        public const byte CoerceString = 0x85;
        public const byte AsType = 0x86;
        public const byte AsTypeLate = 0x87;
        public const byte CoerceUInt = 0x88;
        public const byte CoerceObject = 0x89;
        // 0x8F negate_p
        public const byte Negate = 0x90;
        public const byte Increment = 0x91;
        public const byte IncLocal = 0x92;
        public const byte Decrement = 0x93;
        public const byte DecLocal = 0x94;
        public const byte TypeOf = 0x95;
        public const byte Not = 0x96;
        public const byte BitNot = 0x97;
        public const byte Concat = 0x9A;
        public const byte AddDouble = 0x9B;
        // 0x9c increment_p
        // 0x9d inclocal_p
        // 0x9e decrement_p
        // 0x9f declocal_p
        public const byte Add = 0xA0;
        public const byte Subtract = 0xA1;
        public const byte Multiply = 0xA2;
        public const byte Divide = 0xA3;
        public const byte Modulo = 0xA4;
        public const byte ShiftLeft = 0xA5;
        public const byte ShiftRight = 0xA6;
        public const byte ShiftRightUnsigned = 0xA7;
        public const byte BitAnd = 0xA8;
        public const byte BitOr = 0xA9;
        public const byte BitXor = 0xAA;
        new public const byte Equals = 0xAB;
        public const byte StrictEquals = 0xAC;
        public const byte LessThan = 0xAD;
        public const byte LessEquals = 0xAE;
        public const byte GreaterThan = 0xAF;
        public const byte GreaterEquals = 0xB0;
        public const byte InstanceOf = 0xB1;
        public const byte IsType = 0xB2;
        public const byte IsTypeLate = 0xB3;
        public const byte In = 0xB4;

        public const byte IncrementInt = 0xC0;
        public const byte DecrementInt = 0xC1;
        public const byte IncLocalInt = 0xC2;
        public const byte DecLocalInt = 0xC3;
        public const byte NegateInt = 0xC4;
        public const byte AddInt = 0xC5;
        public const byte SubtractInt = 0xC6;
        public const byte MultiplyInt = 0xC7;
        public const byte GetLocal0 = 0xD0;
        public const byte GetLocal1 = 0xD1;
        public const byte GetLocal2 = 0xD2;
        public const byte GetLocal3 = 0xD3;
        public const byte SetLocal0 = 0xD4;
        public const byte SetLocal1 = 0xD5;
        public const byte SetLocal2 = 0xD6;
        public const byte SetLocal3 = 0xD7;
        // 0xee abs_jump 
        public const byte Debug = 0xEF;
        public const byte DebugLine = 0xF0;
        public const byte DebugFile = 0xF1;
        public const byte BreakpointLine = 0xF2;
        // 0xf3 timestamp
        // 0xf5 verifypass
        // 0xf6 alloc
        // 0xf7 mark
        // 0xf8 wb
        // 0xf9 prologue
        // 0xfa sendenter
        // 0xfb doubletoatom
        // 0xfc sweep
        // 0xfd codegenop
        // 0xfe verifyop
        // 0xff decode

        // Alchemy Opcodes
        public const byte SetByte = 0x3A;
        public const byte SetShort = 0x3B;
        public const byte SetInt = 0x3C;
        public const byte SetFloat = 0x3D;
        public const byte SetDouble = 0x3E;
        public const byte GetByte = 0x35;
        public const byte GetShort = 0x36;
        public const byte GetInt = 0x37;
        public const byte GetFloat = 0x38;
        public const byte GetDouble = 0x39;
        public const byte Sign1 = 0x50;
        public const byte Sign8 = 0x51;
        public const byte Sign16 = 0x52;


        private static HashSet<Byte> _jumpOpcodes;
        public static HashSet<Byte> getJumpOpcodesLookup()
        {
            if (_jumpOpcodes==null)
            {
                _jumpOpcodes = new HashSet<byte>();

                _jumpOpcodes.Add(Op.IfEqual);
                _jumpOpcodes.Add(Op.IfGreaterEqual);
                _jumpOpcodes.Add(Op.IfGreaterThan);
                _jumpOpcodes.Add(Op.IfLessEqual);
                _jumpOpcodes.Add(Op.IfLessThan);
                _jumpOpcodes.Add(Op.IfNotEqual);
                _jumpOpcodes.Add(Op.IfNotGreaterEqual);
                _jumpOpcodes.Add(Op.IfNotGreaterThan);
                _jumpOpcodes.Add(Op.IfNotLessEqual);
                _jumpOpcodes.Add(Op.IfNotLessThan);
                _jumpOpcodes.Add(Op.IfStrictEqual);
                _jumpOpcodes.Add(Op.IfStrictNotEqual);
                _jumpOpcodes.Add(Op.IfFalse);
                _jumpOpcodes.Add(Op.IfTrue);
                _jumpOpcodes.Add(Op.Jump);
                _jumpOpcodes.Add(Op.LookupSwitch);
            }
			return _jumpOpcodes;
		}
    }

    //AVM2Command create;
    //jump;
    /*int atIndex = 2;
    if ((instructions[0] as AVM2Command).OpCode == 0xf1)
    {
        atIndex = 4;
    }
    create = Translator.ToCommand(0x10);
     create.Parameters.Add("crl");
     instructions.Insert(atIndex, create);
     //createobject;
     create = Translator.ToCommand(0x55);
     U30 u30 = new U30();
     u30.Value = 10000000;
     create.Parameters.Add(u30);
     instructions.Insert(atIndex + 1, create);
     //setlocal
     create = Translator.ToCommand(0xd4);
     instructions.Insert(atIndex + 2, create);

     label = new Label("crl");
     labels.Add("crl", label);
     instructions.Insert(atIndex + 3, label);*/
    
}
