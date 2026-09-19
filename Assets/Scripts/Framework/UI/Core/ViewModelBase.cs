using System;

namespace Framework.UI
{
    /// <summary>
    /// UI Framework의 Base ViewModel 클래스입니다.
    /// View를 직접 참조하지 않으며, Model과의 바인딩 및 UI 비즈니스 로직 상태를 관리합니다.
    /// </summary>
    public abstract class ViewModelBase : IDisposable
    {
        /// <summary>
        /// ViewModel이 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// ViewModel이 Dispose되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// ViewModel을 초기화합니다. 중복 호출 시 최초 1회만 처리됩니다.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized || IsDisposed)
                return;

            IsInitialized = true;
            OnInitialize();
        }

        /// <summary>
        /// ViewModel이 활성화될 때 호출됩니다 (View Show/Open 시).
        /// </summary>
        public virtual void Activate()
        {
        }

        /// <summary>
        /// ViewModel이 비활성화될 때 호출됩니다 (View Hide/Close 시).
        /// </summary>
        public virtual void Deactivate()
        {
        }

        /// <summary>
        /// ViewModel의 리소스를 해제하고 Dispose 생명주기를 수행합니다.
        /// 중복 호출 시 안전하게 처리됩니다.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            if (IsInitialized)
            {
                Deactivate();
            }

            IsDisposed = true;
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// ViewModel 초기화 시 하위 클래스에서 실행할 로직을 작성합니다.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// ViewModel 해제 시 하위 클래스에서 리소스를 정리할 로직을 작성합니다.
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}
