using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using as3c.As3c.Compiler;
using As3c.Common;
using As3c.Compiler;
using SwfLibrary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace as3c
{

    class Program
    {
        static void Main(string[] args)
        {
            Translator.InitTable();
            /*execute(new String[] { "-bjym", @"D:\m.swf", @"D:\m_en.swf", @"D:\huaqiangu.dic", @"\\192.168.0.5\embed\junyou\junyou.cls", @"\\192.168.0.5\embed\junyou\airglobal.bf", @"\\192.168.0.5\embed\junyou\core.bf", @"\\192.168.0.5\embed\junyou\playerglobal.bf", @"\\192.168.0.5\embed\junyou\filter.txt" });
            execute(new String[] { "-bjyp", @"D:\workspace\chuanqi2\bin-debug\pingtai\37.swf", @"D:\workspace\chuanqi2\bin-debug\pingtai\37_en1.swf", @"D:\workspace\chuanqi2\bin-debug\chuanqi2.dic", @"C:\Users\Administrator\Desktop\2222.cls",@"D:\workspace\SWFBlurer\src\assets\airglobal.bf",@"D:\workspace\SWFBlurer\src\assets\core.bf",@"D:\workspace\SWFBlurer\src\assets\playerglobal.bf",@"D:\workspace\SWFBlurer\src\assets\filter.txt"});
            return;*/
            if (args.Length > 0)
            {
                execute(args);
                return;
            }

#if DEBUG
            while (true)
            {
                string action = Console.ReadLine();
                if ("exit" == action.ToLower())
                {
                    break;
                }

                args = action.Split(' ');
                if (args.Length > 1)
                {
                    startProgress(args[0], args[1]);
                }
                else
                {
                    startProgress(args[0]);
                }
            }
#endif
        }

        static void startProgress(string _pathSwf, string _pathOutput = "")
        {
            FileStream fileInput;
            FileStream fileOutput;
            SwfFormat swf;

            string currentDirectory = System.Environment.CurrentDirectory;
            if (String.IsNullOrEmpty(_pathSwf))
            {
                _pathSwf = Path.Combine(currentDirectory, "test.swf");
            }else if (String.IsNullOrEmpty(_pathOutput))
            {
                FileInfo fileInfo = new FileInfo(_pathSwf);
                string fileName=fileInfo.Name.Replace(fileInfo.Extension, "_en");
                _pathOutput=Path.Combine(fileInfo.Directory.FullName, fileName + fileInfo.Extension);
            }

            if (File.Exists(_pathSwf) == false)
            {
                Console.WriteLine("{0} 文件不存在", _pathSwf);
                return;
            }

            swf = new SwfFormat();
            using (fileInput = File.Open(_pathSwf, FileMode.Open, FileAccess.Read))
            {
                swf.Read(fileInput);
            }

           ICompile compInline;

           
            //test Blur;
            string[] parms = new string[3];
            parms[0] = Path.Combine(currentDirectory, "airglobal.bf");
            parms[1] = Path.Combine(currentDirectory, "core.bf");
            parms[2] = Path.Combine(currentDirectory, "filter.txt");
            compInline = new CompilerBlur(parms);

            //test get;
            //compInline= new GetStringTabel();

            //test insert;
            //compInline = new CompilerInline();

            //test ia
            //compInline = new ImportAssetsRewriter(@"http://localhost/lib.swf");

            compInline.Compile(swf);

            if (String.IsNullOrEmpty(_pathOutput))
            {
                _pathOutput = Path.Combine(currentDirectory, "test_en.swf");
            }
            
            using (fileOutput = File.Open(_pathOutput, FileMode.OpenOrCreate, FileAccess.Write))
            {
                compInline.Write(fileOutput);
            }

        }

        static void execute(string[] args)
        {
            if (args.Length < 1)
            {
                WriteInfo();
                Console.WriteLine("[?] Use -help for some instructions.");
                return;
            }


            string _pathOutput = "";
            string _pathSwf = "";

            ICompile compiler=null;

            switch (args[0])
            {
                case "-bjym"://君游混淆主程序
                    compiler = new CompilerBlurJYMain(args);
                    break;
                case "-bjyp"://君游混淆插件
                    compiler = new CompilerBlurJYPlugin(args);
                    break;
                case "-i":
                case "-inline":
                    compiler = new CompilerInline();
                    break;

                case "-b":
                case "-blur":
                    compiler = new CompilerBlur(args);
                    break;

                case "-gs":
                case "-getString":
                    compiler = new GetStringTabel();
                    break;

                case "-ia":
                case "-importAssets":

                    if (args.Length > 3)
                    {
                        string newURL = args[3];
                        if (string.IsNullOrEmpty(newURL)==false)
                        {
                            compiler = new ImportAssetsRewriter(newURL);
                        }
                    }

                    if(compiler==null)
                    {
                        Console.WriteLine("[?] Use -help for some instructions.");
                        return;
                    }
                    break;
            }


            if (compiler != null)
            {
                _pathSwf = args[1];

                if (args.Length > 2)
                {
                    _pathOutput = args[2];
                }

                SwfFormat swf  = new SwfFormat();
                using (FileStream fileInput = File.Open(_pathSwf, FileMode.Open, FileAccess.Read))
                {
                    swf.Read(fileInput);
                }

                compiler.Compile(swf);

                if (string.IsNullOrEmpty(_pathOutput))
                {
                    _pathOutput = _pathSwf;
                }


                using (FileStream fileOutput = File.Open(_pathOutput, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    compiler.Write(fileOutput);
                }
            }

        }



        static private void WriteInfo()
        {
            Console.WriteLine("[i] As3c - ActionScript3 ASM compiler");
        }

        static private void Usage()
        {
            string exec = Path.GetFileName(Assembly.GetEntryAssembly().Location);

            Console.WriteLine("[?] Help");
            Console.WriteLine("General syntax:");
            Console.WriteLine("");
            Console.WriteLine(" {0} [options] [action [properties]] [files]", exec);
            Console.WriteLine("");
            Console.WriteLine("  Options:");
            Console.WriteLine("");
            Console.WriteLine("   -quiet");
            Console.WriteLine("   As3c will not generate any output besides error messages while running.");
            Console.WriteLine("");
            Console.WriteLine("   -output [file]");
            Console.WriteLine("   Specify output file.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("  Actions:");
            Console.WriteLine("");
            Console.WriteLine("   -replace [properties] [swf] [asm]");
            Console.WriteLine("");
            Console.WriteLine("   Replaces an existing method body. In order to replace a method body with");
            Console.WriteLine("   your compiled bytecode you have to tell As3c which method to replace.");
            Console.WriteLine("");
            Console.WriteLine("   You will have to specify the SWF file to work with and an ASM file to");
            Console.WriteLine("   replace the currently existing method.");
            Console.WriteLine("");
            Console.WriteLine("    Properties:");
            Console.WriteLine("");
            Console.WriteLine("     -namespace [uri]");
            Console.WriteLine("");
            Console.WriteLine("     The namespace of the method. You only have to pass a namespace");
            Console.WriteLine("     if the method lies inside on. Otherwise you do not have to pass");
            Console.WriteLine("     this parameter.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -class [name]");
            Console.WriteLine("");
            Console.WriteLine("     The name of the class that contains the method to replace. The");
            Console.WriteLine("     name should also include the package path.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -method [method]");
            Console.WriteLine("");
            Console.WriteLine("     The name of the method to replace. This method can have any visibility.");
            Console.WriteLine("     It does not matter if it is private, protected, internal or public.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -constructor");
            Console.WriteLine("");
            Console.WriteLine("     The constructor of the class. This is the so called instance constructor");
            Console.WriteLine("     and what you basically know from actionscript when writing a simple");
            Console.WriteLine("     constructor.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -static-method [method]");
            Console.WriteLine("");
            Console.WriteLine("     Same as the -method but this one will work with static functions.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -static-constructor");
            Console.WriteLine("");
            Console.WriteLine("     This one is the class constructor which is executed on class initialization");
            Console.WriteLine("     level. This constructor is not executed per instance.");
            Console.WriteLine("");
            Console.WriteLine("    Example:");
            Console.WriteLine("");
            Console.WriteLine("     {0} -o optimized.swf -class foo.Bar", exec);
            Console.WriteLine("      -method toString test.swf optimized.asm");
            Console.WriteLine("");
            Console.WriteLine("     This would replace the toString() function of the Bar class which");
            Console.WriteLine("     is located in the foo package.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("   -inline [swf]");
            Console.WriteLine("");
            Console.WriteLine("   Compile inline ASM instructions that have to be written using the As3c");
            Console.WriteLine("   ActionScript 3 framework.");
            Console.WriteLine("");
            Console.WriteLine("    Example:");
            Console.WriteLine("");
            Console.WriteLine("     {0} -o output.swf -inline input.swf", exec);
            Console.WriteLine("");
            Console.WriteLine("     This would replace all inline ASM instructions in the input.swf with");
            Console.WriteLine("     their proper bytecode and write to output.swf.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     {0} -inline input.swf", exec);
            Console.WriteLine("");
            Console.WriteLine("     This is the simple command to just replace all inline ASM instructions");
            Console.WriteLine("     in one file and save it. Probably what you have been looking for ;)");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("   -dasm [properties] [swf]");
            Console.WriteLine("");
            Console.WriteLine("   Disassemble bytecode of given SWF. If you do not specify any output As3c");
            Console.WriteLine("   will write it to the console.");
            Console.WriteLine("");
            Console.WriteLine("    Properties:");
            Console.WriteLine("");
            Console.WriteLine("     -as3c");
            Console.WriteLine("");
            Console.WriteLine("     Disassemble to As3c syntax. The output will include some comments");
            Console.WriteLine("     to make it easier for you to navigate through and find the function");
            Console.WriteLine("     you search for.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("     -plain");
            Console.WriteLine("");
            Console.WriteLine("     Disassemble keeping plain syntax without resolving any constant");
            Console.WriteLine("     or namespace.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("    Example:");
            Console.WriteLine("");
            Console.WriteLine("     {0} -o dasm.txt -as3c test.swf", exec);
            Console.WriteLine("");
            Console.WriteLine("     This would disassemble the test.swf file using As3c syntax and write");
            Console.WriteLine("     the output into dasm.txt.");
        }
    }
}
