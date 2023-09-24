using Nimble.ViewModels;

namespace Nimble.Models
{
    public class ItemBase : ViewModelBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyPropertyChange("IsSelected");
            }
        }
    }
}
