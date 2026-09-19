namespace Framework.Scene.Events
{
    /// <summary>
    /// 씬 로드가 시작될 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneLoadStartEvent
    {
        public string SceneName { get; }
        public SceneLoadMode LoadMode { get; }
        public string LoadingSceneName { get; }

        public SceneLoadStartEvent(string sceneName, SceneLoadMode loadMode, string loadingSceneName = null)
        {
            SceneName = sceneName;
            LoadMode = loadMode;
            LoadingSceneName = loadingSceneName;
        }
    }

    /// <summary>
    /// 씬 로드 진행 상태(0.0 ~ 1.0)가 갱신될 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneLoadProgressEvent
    {
        public string SceneName { get; }
        public float Progress { get; }

        public SceneLoadProgressEvent(string sceneName, float progress)
        {
            SceneName = sceneName;
            Progress = progress;
        }
    }

    /// <summary>
    /// 씬 전환이 성공적으로 완료되었을 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneLoadCompleteEvent
    {
        public string SceneName { get; }
        public SceneLoadMode LoadMode { get; }
        public float ElapsedTime { get; }

        public SceneLoadCompleteEvent(string sceneName, SceneLoadMode loadMode, float elapsedTime)
        {
            SceneName = sceneName;
            LoadMode = loadMode;
            ElapsedTime = elapsedTime;
        }
    }

    /// <summary>
    /// 특정 씬의 언로드가 시작될 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneUnloadStartEvent
    {
        public string SceneName { get; }

        public SceneUnloadStartEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    /// <summary>
    /// 특정 씬의 언로드가 완료되었을 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneUnloadCompleteEvent
    {
        public string SceneName { get; }

        public SceneUnloadCompleteEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    /// <summary>
    /// 씬 전환 중 오류가 발생하여 실패했을 때 EventBus로 전달되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct SceneLoadFailedEvent
    {
        public string SceneName { get; }
        public string ErrorMessage { get; }

        public SceneLoadFailedEvent(string sceneName, string errorMessage)
        {
            SceneName = sceneName;
            ErrorMessage = errorMessage;
        }
    }
}
