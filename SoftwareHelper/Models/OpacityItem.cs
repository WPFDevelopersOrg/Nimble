using SoftwareHelper.ViewModels;

namespace SoftwareHelper.Models
{
    public class OpacityItem : ViewModelBase
    {
        private bool _isSelected;
        private string _itemName;
        private readonly string Symbol = "%";

        public string ItemName
        {
            get => Value + Symbol;
            set => _itemName = value;
        }

        public double Value { get; set; }

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