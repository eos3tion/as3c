using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwfLibrary.Swf
{
    public class SWFSymbol
    {
        public ushort tagId;
        public string name;

        public SWFSymbol(BinaryReader input)
        {
            tagId = input.ReadUInt16();

            char chr;
            while ((chr = (char)input.ReadByte()) != 0)
            {
                name += chr;
            }

        }

        internal void WriteExternal(BinaryWriter output)
        {
            output.Write(tagId);

            for (int i = 0; i < name.Length; ++i)
            {
                output.Write(name[i]);
            }

            output.Write((byte)0);
        }
    }
}
