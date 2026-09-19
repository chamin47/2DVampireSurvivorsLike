using Framework.UI;

namespace Example.UI
{
    /// <summary>
    /// Inventory UI의 상태와 명령을 처리하는 ViewModel 예제입니다.
    /// InventoryView를 직접 참조하지 않습니다.
    /// </summary>
    public class InventoryViewModel : ViewModelBase
    {
        private readonly InventoryModel model;

        /// <summary>
        /// Gold Observable Property 전달입니다.
        /// </summary>
        public ObservableProperty<int> Gold => model.Gold;

        /// <summary>
        /// ItemCount Observable Property 전달입니다.
        /// </summary>
        public ObservableProperty<int> ItemCount => model.ItemCount;

        /// <summary>
        /// Gold 100 증가 Command입니다.
        /// </summary>
        public RelayCommand AddGoldCommand { get; private set; }

        public InventoryViewModel(InventoryModel model)
        {
            this.model = model;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddGoldCommand = new RelayCommand(AddGold);
        }

        private void AddGold()
        {
            if (model != null)
            {
                model.Gold.Value += 100;
            }
        }

        protected override void OnDispose()
        {
            model?.Dispose();
            base.OnDispose();
        }
    }
}
