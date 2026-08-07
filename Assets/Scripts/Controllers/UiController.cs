using SoulsLike.Services;
using SoulsLike.Ui.Base;

namespace SoulsLike
{
    public abstract class UiController
    {
        protected IUiService UiService { get; private set; }

        protected UiController(IUiService uiService)
        {
            UiService = uiService;
        }

        protected TUi CreateUi<TUi>()  where TUi:IBaseUi
        {
            return UiService.CreateUi<TUi>();
        }
    }
}