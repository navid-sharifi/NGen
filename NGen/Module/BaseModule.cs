
namespace NSharp
{
    public abstract class BaseModule : NCommon
    {
        internal List<Button> Buttons = new List<Button>();

        public Button button(string name)
        {
            var button = new Button(name);
            Buttons.Add(button);
            return button;
        }

        public virtual async Task<string> ReactBuid(System.Type moduleType, System.Type pageType)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<string> ControllerActions(System.Type moduleType, System.Type pageType)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<string> ViewModels(System.Type moduleType, System.Type pageType)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<string> Flutter(System.Type moduleType, System.Type pageType)
        {
            return string.Empty;
        }

    }

}
