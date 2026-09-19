using Framework.UI;

namespace Example.UI
{
    /// <summary>
    /// Inventory 데이터와 비즈니스 로직을 보관하는 Model 예제입니다.
    /// UI 프레임워크나 Unity API에 의존하지 않습니다.
    /// </summary>
    public class InventoryModel : ModelBase
    {
        /// <summary>
        /// 보유 골드 Observable Property입니다. (초기값: 1000)
        /// </summary>
        public ObservableProperty<int> Gold { get; } = new ObservableProperty<int>(1000);

        /// <summary>
        /// 아이템 개수 Observable Property입니다. (초기값: 0)
        /// </summary>
        public ObservableProperty<int> ItemCount { get; } = new ObservableProperty<int>(0);

        protected override void OnDispose()
        {
            Gold.ClearListeners();
            ItemCount.ClearListeners();
            base.OnDispose();
        }
    }
}
