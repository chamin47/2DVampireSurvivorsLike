using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// UI Framework의 Base View 컴포넌트입니다.
    /// Unity MonoBehaviour 기반으로 동작하며, ViewModel과의 바인딩 및 UI 컴포넌트 이벤트를 관리합니다.
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private UILayer layer = UILayer.Popup;

        /// <summary>
        /// View가 소속된 UI Layer를 반환합니다.
        /// </summary>
        public UILayer Layer => layer;

        /// <summary>
        /// View가 초기화되었는지 여부입니다.
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// View가 ViewModel과 바인딩되어 있는지 여부입니다.
        /// </summary>
        public bool IsBound { get; protected set; }

        /// <summary>
        /// 바인딩되어 있는 ViewModel 객체입니다.
        /// </summary>
        protected ViewModelBase ViewModel { get; private set; }

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnEnable()
        {
            OnViewEnable();
        }

        protected virtual void OnDisable()
        {
            OnViewDisable();
        }

        protected virtual void OnDestroy()
        {
            if (IsBound)
            {
                Unbind();
            }
        }

        /// <summary>
        /// View를 초기화합니다. 중복 호출 시 최초 1회만 처리됩니다.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            OnInitialize();
        }

        /// <summary>
        /// ViewModel 객체와 View를 바인딩합니다.
        /// 이미 다른 ViewModel이 바인딩되어 있다면 기존 ViewModel을 Unbind한 후 새 ViewModel을 바인딩합니다.
        /// </summary>
        /// <param name="viewModel">바인딩할 ViewModel</param>
        public void Bind(ViewModelBase viewModel)
        {
            if (viewModel == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Attempted to bind a null ViewModel.");
                return;
            }

            if (IsBound)
            {
                if (ViewModel == viewModel)
                    return;

                Unbind();
            }

            ViewModel = viewModel;
            IsBound = true;

            if (!ViewModel.IsInitialized)
            {
                ViewModel.Initialize();
            }

            OnBind(viewModel);
        }

        /// <summary>
        /// 현재 바인딩된 ViewModel과의 연결을 해제합니다.
        /// </summary>
        public void Unbind()
        {
            if (!IsBound)
                return;

            var prevVm = ViewModel;
            ViewModel = null;
            IsBound = false;

            OnUnbind(prevVm);
        }

        /// <summary>
        /// 바인딩된 ViewModel을 특정 ViewModel 타입으로 캐스팅하여 가져옵니다.
        /// </summary>
        protected T GetViewModel<T>() where T : ViewModelBase
        {
            return ViewModel as T;
        }

        /// <summary>
        /// View 초기화 시 실행되는 확장 메서드입니다.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// ViewModel 바인딩 시 이벤트 및 Data Subscription을 등록하는 확장 메서드입니다.
        /// </summary>
        protected virtual void OnBind(ViewModelBase viewModel)
        {
        }

        /// <summary>
        /// ViewModel 바인딩 해제 시 이벤트 및 Data Subscription을 해제하는 확장 메서드입니다.
        /// </summary>
        protected virtual void OnUnbind(ViewModelBase viewModel)
        {
        }

        /// <summary>
        /// Unity OnEnable 호출 시 실행되는 확장 메서드입니다.
        /// </summary>
        protected virtual void OnViewEnable()
        {
        }

        /// <summary>
        /// Unity OnDisable 호출 시 실행되는 확장 메서드입니다.
        /// </summary>
        protected virtual void OnViewDisable()
        {
        }
    }
}
