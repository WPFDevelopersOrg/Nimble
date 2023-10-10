namespace Nimble.Models
{
    public class WallpaperItem : ItemBase
    {
        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                NotifyPropertyChange("ItemName");
            }
        }
        public string VideoPath { get; set; }
    }
}
