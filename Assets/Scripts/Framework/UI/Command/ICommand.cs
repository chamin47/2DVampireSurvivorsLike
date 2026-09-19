using System;

namespace Framework.UI
{
    /// <summary>
    /// MVVM UI Command 인터페이스입니다.
    /// View에서 발생한 사용자 Action을 ViewModel의 메서드와 바인딩하는 역할을 수행합니다.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command 실행 가능 여부가 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action CanExecuteChanged;

        /// <summary>
        /// 현재 Command를 실행할 수 있는지 여부를 확인합니다.
        /// </summary>
        /// <returns>실행 가능하면 true, 불가능하면 false</returns>
        bool CanExecute();

        /// <summary>
        /// Command를 실행합니다.
        /// </summary>
        void Execute();

        /// <summary>
        /// CanExecute 상태가 변경되었음을 알리고 CanExecuteChanged 이벤트를 호출합니다.
        /// </summary>
        void NotifyCanExecuteChanged();
    }
}
