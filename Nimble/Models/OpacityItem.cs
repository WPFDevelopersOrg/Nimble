namespace Nimble.Models
{
    public class OpacityItem : ItemBase
    {
        private string _itemName;
        private readonly string Symbol = "%";
        public string ItemName
        {
            get => Value + Symbol;
            set => _itemName = value;
        }

        public double Value { get; set; }
    }
}