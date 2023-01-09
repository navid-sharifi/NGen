using System.Transactions;

namespace NGen
{
    public class FileManager : AModule , DependentModule
    {
        public override string GetReactPage(Type pageType, Type moduleType)
        {
            return  @$"";

        }
    }
}
