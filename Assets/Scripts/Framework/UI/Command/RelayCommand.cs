using System;

namespace Framework.UI
{
    /// <summary>
    /// Action 및 Func 델리게이트를 전달받아 실행되는 RelayCommand 클래스입니다.
    /// </summary>
    public class RelayCommand : CommandBase
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// RelayCommand를 생성합니다.
        /// </summary>
        /// <param name="execute">실행할 Action 로직</param>
        /// <param name="canExecute">실행 가능 여부를 확인할 조건 Func (null일 경우 항상 true)</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Command를 실행할 수 있는지 확인합니다.
        /// </summary>
        public override bool CanExecute()
        {
            return canExecute == null || canExecute.Invoke();
        }

        /// <summary>
        /// Command를 실행합니다. CanExecute()가 true일 경우에만 Action이 호출됩니다.
        /// </summary>
        public override void Execute()
        {
            if (CanExecute())
            {
                execute.Invoke();
            }
        }
    }
}
