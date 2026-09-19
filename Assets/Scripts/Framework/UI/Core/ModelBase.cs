using System;

namespace Framework.UI
{
    /// <summary>
    /// UI Framework의 Base Model 클래스입니다.
    /// 게임 데이터 및 비즈니스 로직, 서버 데이터 등을 보관하고 관리하며 UI(View/ViewModel)를 직접 참조하지 않습니다.
    /// </summary>
    public abstract class ModelBase : IDisposable
    {
        /// <summary>
        /// Model이 Dispose 상태인지 여부를 나타냅니다.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Model의 리소스를 해제하고 Dispose 생명주기를 수행합니다.
        /// 중복 호출 시 안전하게 처리됩니다.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Model이 Dispose될 때 하위 클래스에서 리소스를 해제하거나 이벤트를 해제하기 위한 확장 메서드입니다.
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}
