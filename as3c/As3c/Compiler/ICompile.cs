using System.IO;
using SwfLibrary;

namespace As3c.Compiler
{
    public interface ICompile
    {
        void Compile(SwfFormat swf);

        void Write(Stream fileOutput);
    }
}
