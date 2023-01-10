using System.Transactions;

namespace NGen
{
    public class FileManager : AModule , IIndependentModule
    {
        public (string name, string content) Controller()
        {
            throw new NotImplementedException();
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
