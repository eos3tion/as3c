using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SwfLibrary;
using SwfLibrary.Types;
using SwfLibrary.Types.Tags;

namespace As3c.Compiler
{
    public class ImportAssetsRewriter : ICompile
    {
        private SwfFormat _swf;
        private string newURL;

        public ImportAssetsRewriter(string newURL)
        {
            this.newURL = newURL.Trim();
        }

        public void Compile(SwfFormat swf)
        {
            _swf = swf;


            foreach (Tag item in _swf.Tags)
            {
                if(item.Body is ImportAssets2)
                {
                    ImportAssets2 importAssets = item.Body as ImportAssets2;
                    string oldURL = importAssets.url;

                    importAssets.url = newURL;


                    Console.WriteLine("{0} rewrite to : {1}",oldURL,newURL);
                }
            }
           
        }

        public void Write(Stream fs)
        {
            _swf.Write(fs);
        }
    }
}
