namespace Framework.Scene
{
    /// <summary>
    /// 씬 로드 방식을 정의하는 열거형입니다.
    /// UnityEngine.SceneManagement.LoadSceneMode를 래핑하여 프레임워크 독립성을 확보합니다.
    /// </summary>
    public enum SceneLoadMode
    {
        /// <summary>
        /// 기존 씬을 언로드하고 새로운 씬 하나만 로드합니다.
        /// </summary>
        Single = 0,

        /// <summary>
        /// 기존 씬을 유지한 채 새로운 씬을 추가로 로드합니다.
        /// </summary>
        Additive = 1
    }
}
