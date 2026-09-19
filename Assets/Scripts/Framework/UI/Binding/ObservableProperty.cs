using System;
using System.Collections.Generic;

namespace Framework.UI
{
    /// <summary>
    /// MVVM 데이터 바인딩을 위한 Observable Property 클래스입니다.
    /// 값이 변경될 때 등록된 Listener에게 알림을 전송합니다.
    /// </summary>
    /// <typeparam name="T">관찰 대상 데이터의 타입</typeparam>
    public class ObservableProperty<T>
    {
        private T value;
        private Action<T> onValueChanged;

        /// <summary>
        /// 현재 관찰 속성의 값입니다.
        /// 값이 새로 설정될 때 기존 값과 다를 경우 이벤트를 발생시킵니다.
        /// </summary>
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                    return;

                this.value = value;
                NotifyValueChanged();
            }
        }

        /// <summary>
        /// ObservableProperty를 생성합니다.
        /// </summary>
        /// <param name="initialValue">초기 설정 값</param>
        public ObservableProperty(T initialValue = default)
        {
            value = initialValue;
        }

        /// <summary>
        /// 값 변경 이벤트를 구독합니다.
        /// </summary>
        /// <param name="listener">알림을 전달받을 콜백 Action</param>
        public void Subscribe(Action<T> listener)
        {
            if (listener == null)
                return;

            // 중복 구독 방지
            onValueChanged -= listener;
            onValueChanged += listener;
        }

        /// <summary>
        /// 값 변경 이벤트 구독을 해제합니다.
        /// </summary>
        /// <param name="listener">해제할 콜백 Action</param>
        public void Unsubscribe(Action<T> listener)
        {
            if (listener == null)
                return;

            onValueChanged -= listener;
        }

        /// <summary>
        /// 등록된 모든 Listener를 제거합니다.
        /// </summary>
        public void ClearListeners()
        {
            onValueChanged = null;
        }

        /// <summary>
        /// IDisposable 기반의 구독을 생성합니다.
        /// Dispose() 호출 시 자동으로 Unsubscribe됩니다.
        /// </summary>
        public IDisposable SubscribeDisposable(Action<T> listener)
        {
            Subscribe(listener);
            return new Subscription(this, listener);
        }

        /// <summary>
        /// 모든 구독자에게 현재 변경된 값을 전송합니다.
        /// </summary>
        private void NotifyValueChanged()
        {
            onValueChanged?.Invoke(value);
        }

        /// <summary>
        /// IDisposable을 지원하는 내부 Subscription 클래스입니다.
        /// </summary>
        private sealed class Subscription : IDisposable
        {
            private ObservableProperty<T> property;
            private Action<T> listener;

            public Subscription(ObservableProperty<T> property, Action<T> listener)
            {
                this.property = property;
                this.listener = listener;
            }

            public void Dispose()
            {
                if (property != null && listener != null)
                {
                    property.Unsubscribe(listener);
                    property = null;
                    listener = null;
                }
            }
        }
    }
}
