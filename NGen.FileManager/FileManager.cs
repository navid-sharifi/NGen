using NSharp;
using System.Transactions;

namespace NGen
{
    public class FileManager : AModule , IIndependentModule
    {
        public (string name, string content) Controller()
        {
          var content =  NPath.GetBaseDirectory().SubDirectory("FileManager").ReadFile("Controller.cs");
            return ("FileManager.cs", content);
        }

        public override string GetReactPage(Type pageType, Type moduleType)
        {
            return  @$"";

        }

        public (string name, string content) ReactModule()
        {
            throw new NotImplementedException();
        }
    }
}
