using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SwfLibrary;
using System.IO;
using SwfLibrary.Abc;
using SwfLibrary.Abc.Constants;

namespace As3c.Compiler
{
    public class GetStringTabel : ICompile
    {
        protected HashSet<string> result;
        protected SwfFormat _swf;
        public void Compile(SwfFormat swf)
        {
            _swf = swf;


            result = new HashSet<string>();

            int i, j, n, m;

            StringInfo info;
            
            for (i = 0, n = _swf.AbcCount; i < n; ++i)
            {
                Abc46 abc = _swf.GetAbcAt(i);

                List<StringInfo> stringTable = abc.ConstantPool.StringTable;

                for (j = 0, m = stringTable.Count; j < m; j++)
                {
                    info=stringTable[j];

                    result.Add(info.ToString());
                }
            }
        }


        public void Write(Stream stream)
        {
            if (stream==null)
            {
                return;
            }
            using (BinaryWriter writer = new BinaryWriter(stream))
            {

                writer.Write(result.Count);
                foreach (string item in result)
                {
                    writer.Write(item);
                }
                writer.Flush();
            }

            Console.WriteLine("complete!");
        }
    }
}
