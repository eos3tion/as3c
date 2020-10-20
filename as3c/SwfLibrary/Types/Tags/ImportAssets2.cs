using SwfLibrary.Swf;
using System.Collections.Generic;
using System.IO;

namespace SwfLibrary.Types.Tags
{
    public class ImportAssets2 :TagBody
    {
        public string url;

        private List<SWFSymbol> _symbols;

        public ImportAssets2(Tag parent) : base(parent) { }

        public override void ReadExternal(BinaryReader input)
        {
            char chr;
            while ((chr = (char)input.ReadByte()) != 0)
            {
                url += chr;
            }

            byte te = input.ReadByte();
            te = input.ReadByte();

            uint numSymbols = input.ReadUInt16();
            _symbols = new List<SWFSymbol>((int)numSymbols);

            for (uint i = 0; i < numSymbols; i++)
            {
                _symbols.Add(new SWFSymbol(input));
            }
        }


        public override void WriteExternal(BinaryWriter output)
        {
            for (int i = 0; i < url.Length; ++i)
            {
                output.Write(url[i]);
            }
            output.Write((byte)0);

            output.Write((byte)1);
            output.Write((byte)0);

            ushort numSymbols =(ushort)_symbols.Count;

            output.Write(numSymbols);

            for (ushort i = 0; i < numSymbols; i++)
            {
                _symbols[i].WriteExternal(output);
            }

        }
    }
}
