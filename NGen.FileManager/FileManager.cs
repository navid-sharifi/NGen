using NSharp;
using System.Transactions;

namespace NGen
{
    public class FileManager : AModule, IIndependentModule
    {
        public (string name, string content) Controller()
        {
            var content = NPath.GetBaseDirectory().SubDirectory("FileManager").ReadFile("Controller.cs");
            return ("FileManager.cs", content);
        }

        public (string name, string content) Entity()
        {
            var content = NPath.GetBaseDirectory().SubDirectory("FileManager").ReadFile("Entity.cs");
            return ("FileManager.cs", content);
        }

        public (string name, string content) ReactCssFile()
        {
            var content = NPath.GetBaseDirectory().SubDirectory("FileManager").ReadFile("FileManagerModule.scss");
            return ("FileManagerModule.scss", content);
        }

        public (string name, string content) ReactModule()
        {
            var content = NPath.GetBaseDirectory().SubDirectory("FileManager").ReadFile("FileManager.js");
            return ("FileManager.js", content);
        }
    }
}
