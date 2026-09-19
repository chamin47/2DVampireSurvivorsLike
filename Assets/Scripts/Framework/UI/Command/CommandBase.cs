using System;

namespace Framework.UI
{
    /// <summary>
    /// ICommand 인터페이스의 기본 추상 구현 클래스입니다.
    /// CanExecuteChanged 이벤트 알림을 관리합니다.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Command 실행 가능 여부 변경 이벤트입니다.
        /// </summary>
        public event Action CanExecuteChanged;

        /// <summary>
        /// Command 실행 가능 여부를 판단합니다.
        /// </summary>
        public abstract bool CanExecute();

        /// <summary>
        /// Command 로직을 실행합니다.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// CanExecuteChanged 이벤트를 안전하게 발생시킵니다.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }
}
