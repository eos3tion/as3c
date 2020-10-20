using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SwfLibrary.Abc;
using SwfLibrary.Types;
using SwfLibrary.Types.Tags;

namespace as3c.SwfLibrary.Types.Tags
{
    public class DoABCDeprecated : DoABC
    {
        public DoABCDeprecated(Tag parent) : base(parent) { }
        public override void ReadExternal(BinaryReader input)
        {
            _abc = new Abc46();
            _abc.Length = (uint)(_parent.Header.Length - 4);
            _abc.ReadExternal(input);
        }

        public override void WriteExternal(BinaryWriter output)
        {
            _abc.WriteExternal(output);
        }
    }

}
