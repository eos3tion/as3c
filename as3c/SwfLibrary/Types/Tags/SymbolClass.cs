using SwfLibrary.Abc.Constants;
using SwfLibrary.Swf;
using System;
using System.Collections.Generic;
using System.IO;

namespace SwfLibrary.Types.Tags
{
    public class SymbolClass : DefaultBody
    {
        private List<SWFSymbol> _symbols;
        public SymbolClass(Tag parent) : base(parent) { }

        public List<SWFSymbol> Symbols
        {
            get
            {
                return _symbols;
            }
        }

        public override void ReadExternal(BinaryReader input)
        {
            int n = (int)input.ReadUInt16();

            _symbols = new List<SWFSymbol>(n);

            while (n-->0)
            {
                _symbols.Add(new SWFSymbol(input));
            }

        }

        public override void WriteExternal(BinaryWriter output)
        {
           ushort numSymbols = (ushort)Symbols.Count;

            output.Write(numSymbols);
            
            for (ushort i = 0; i < numSymbols; i++) {
                _symbols[i].WriteExternal(output);
            }

        }
    }
}
