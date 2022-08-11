using Vintagestory.API.Common;

namespace cosmosis
{
    public interface IFacadable
    {
        void SetFacade(ItemSlot fromSlot);

        void ApplyFacade();

        void HideFacade();

        void ShowFacade();
    }
}