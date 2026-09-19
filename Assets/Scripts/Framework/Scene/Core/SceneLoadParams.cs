using System;
using System.Threading.Tasks;

namespace Framework.Scene.Core
{
    /// <summary>
    /// 씬 전환 시 전달할 옵션 및 콜백 설정을 정의하는 파라미터 클래스입니다.
    /// </summary>
    public class SceneLoadParams
    {
        public string TargetSceneName { get; set; }
        public string LoadingSceneName { get; set; }
        public SceneLoadMode LoadMode { get; set; } = SceneLoadMode.Single;
        public bool AutoActivate { get; set; } = true;
        public bool ReleaseUnusedAssets { get; set; } = true;

        public Func<Task> PreLoadTask { get; set; }
        public Func<Task> PostLoadTask { get; set; }
        public Action<float> OnProgress { get; set; }
        public Action<bool> OnCompleted { get; set; }

        public SceneLoadParams() { }

        public SceneLoadParams(string targetSceneName)
        {
            TargetSceneName = targetSceneName;
        }

        public SceneLoadParams SetLoadingScene(string loadingSceneName)
        {
            LoadingSceneName = loadingSceneName;
            return this;
        }

        public SceneLoadParams SetLoadMode(SceneLoadMode loadMode)
        {
            LoadMode = loadMode;
            return this;
        }

        public SceneLoadParams SetAutoActivate(bool autoActivate)
        {
            AutoActivate = autoActivate;
            return this;
        }

        public SceneLoadParams SetReleaseUnusedAssets(bool release)
        {
            ReleaseUnusedAssets = release;
            return this;
        }

        public SceneLoadParams SetPreLoadTask(Func<Task> task)
        {
            PreLoadTask = task;
            return this;
        }

        public SceneLoadParams SetPostLoadTask(Func<Task> task)
        {
            PostLoadTask = task;
            return this;
        }

        public SceneLoadParams SetProgressCallback(Action<float> callback)
        {
            OnProgress = callback;
            return this;
        }

        public SceneLoadParams SetCompletedCallback(Action<bool> callback)
        {
            OnCompleted = callback;
            return this;
        }
    }
}
