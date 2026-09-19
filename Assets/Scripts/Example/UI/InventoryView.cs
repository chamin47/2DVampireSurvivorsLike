using Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Example.UI
{
    /// <summary>
    /// Inventory UI 화면을 담당하는 View 예제입니다.
    /// InventoryViewModel의 ObservableProperty 변화를 수신하고 UI를 갱신합니다.
    /// </summary>
    public class InventoryView : ViewBase
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text itemCountText;
        [SerializeField] private Button addGoldButton;

        protected override void OnBind(ViewModelBase viewModel)
        {
            base.OnBind(viewModel);

            var vm = GetViewModel<InventoryViewModel>();
            if (vm == null)
                return;

            // 1. ObservableProperty 데이터 바인딩
            vm.Gold.Subscribe(OnGoldChanged);
            vm.ItemCount.Subscribe(OnItemCountChanged);

            // 초기값 UI 갱신
            OnGoldChanged(vm.Gold.Value);
            OnItemCountChanged(vm.ItemCount.Value);

            // 2. Button -> Command 바인딩
            if (addGoldButton != null)
            {
                addGoldButton.onClick.AddListener(OnAddGoldButtonClicked);
            }
        }

        protected override void OnUnbind(ViewModelBase viewModel)
        {
            var vm = GetViewModel<InventoryViewModel>();
            if (vm != null)
            {
                // 이벤트 해제하여 메모리 누수 방지
                vm.Gold.Unsubscribe(OnGoldChanged);
                vm.ItemCount.Unsubscribe(OnItemCountChanged);
            }

            if (addGoldButton != null)
            {
                addGoldButton.onClick.RemoveListener(OnAddGoldButtonClicked);
            }

            base.OnUnbind(viewModel);
        }

        private void OnGoldChanged(int newGold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {newGold}";
            }
        }

        private void OnItemCountChanged(int newItemCount)
        {
            if (itemCountText != null)
            {
                itemCountText.text = $"Items: {newItemCount}";
            }
        }

        private void OnAddGoldButtonClicked()
        {
            var vm = GetViewModel<InventoryViewModel>();
            if (vm != null && vm.AddGoldCommand.CanExecute())
            {
                vm.AddGoldCommand.Execute();
            }
        }
    }
}
