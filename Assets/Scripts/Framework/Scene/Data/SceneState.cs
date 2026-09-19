namespace Framework.Scene
{
    /// <summary>
    /// Scene Manager의 현재 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum SceneState
    {
        /// <summary>
        /// 대기 상태 (씬 전환 요청 처리 가능)
        /// </summary>
        Idle,

        /// <summary>
        /// 씬 로드 진행 중
        /// </summary>
        Loading,

        /// <summary>
        /// 씬 언로드 진행 중
        /// </summary>
        Unloading,

        /// <summary>
        /// 씬 전환 완료
        /// </summary>
        Completed,

        /// <summary>
        /// 씬 전환 실패
        /// </summary>
        Failed
    }
}
