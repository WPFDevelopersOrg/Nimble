using SoftWareHelper.ViewModels;

namespace SoftWareHelper.Models
{
    public class OpacityItem: ViewModelBase
    {
        private string _itemName;
        public string ItemName
        {
            get 
            { 
                return Value + Symbol;
            }
            set
            {
                _itemName = value;
            }
        }

        public double Value { get; set; }
        private string Symbol = "%";
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                this.NotifyPropertyChange("IsSelected");
            }
        }
    }
}
