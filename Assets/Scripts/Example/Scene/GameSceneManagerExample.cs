using System.Threading.Tasks;
using UnityEngine;
using Framework.Event;
using Framework.Scene.Core;
using Framework.Scene.Events;
using Framework.Scene.Interfaces;

namespace Example.Scene
{
    /// <summary>
    /// GameSceneManager 사용법을 예시와 함께 가이드하는 샘플 컴포넌트입니다.
    /// </summary>
    public class GameSceneManagerExample : MonoBehaviour, ISceneController
    {
        [Header("Scene Settings")]
        [SerializeField] private string targetSceneName = "GamePlayScene";
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private string uiOverlaySceneName = "InGameUIScene";

        private void OnEnable()
        {
            // EventBus 이벤트 구독
            EventBus.Subscribe<SceneLoadStartEvent>(OnSceneLoadStart);
            EventBus.Subscribe<SceneLoadProgressEvent>(OnSceneLoadProgress);
            EventBus.Subscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete);
            EventBus.Subscribe<SceneLoadFailedEvent>(OnSceneLoadFailed);
        }

        private void OnDisable()
        {
            // EventBus 이벤트 구독 해제
            EventBus.Unsubscribe<SceneLoadStartEvent>(OnSceneLoadStart);
            EventBus.Unsubscribe<SceneLoadProgressEvent>(OnSceneLoadProgress);
            EventBus.Unsubscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete);
            EventBus.Unsubscribe<SceneLoadFailedEvent>(OnSceneLoadFailed);
        }

        private async void Start()
        {
            Debug.Log("=== GameSceneManager Example Started ===");

            // 1. 단순 씬 로드 요청 예시
            Debug.Log("[Example] Testing basic scene load call parameters creation...");
            var loadParams = new SceneLoadParams(targetSceneName)
                .SetLoadingScene(loadingSceneName)
                .SetLoadMode(Framework.Scene.SceneLoadMode.Single)
                .SetAutoActivate(true)
                .SetReleaseUnusedAssets(true)
                .SetProgressCallback(p => Debug.Log($"[Example Callback] Progress: {p * 100:F0}%"))
                .SetPreLoadTask(async () =>
                {
                    Debug.Log("[Example PreLoadTask] Simulating resource preloading (e.g. AssetLoader preloads)...");
                    await Task.Delay(200);
                })
                .SetPostLoadTask(async () =>
                {
                    Debug.Log("[Example PostLoadTask] Simulating post-load initialization...");
                    await Task.Delay(100);
                });

            // 2. 중복 요청 거부 테스트
            Debug.Log($"[Example] Current GameSceneManager IsLoading: {GameSceneManager.Instance.IsLoading}");

            // Note: 실제 Unity 에디터 Build Settings에 씬이 포함되어 있지 않으면 씬 로드는 실패 이벤트를 전달합니다.
            Debug.Log($"[Example] Attempting to load scene '{targetSceneName}'...");
            bool result = await GameSceneManager.LoadSceneAsync(loadParams);
            Debug.Log($"[Example] Scene load result: {result}");

            // 3. Additive 씬 로드 및 언로드 예시
            Debug.Log($"[Example] Testing Additive scene load for '{uiOverlaySceneName}'...");
            bool additiveResult = await GameSceneManager.LoadSceneAdditiveAsync(uiOverlaySceneName);
            Debug.Log($"[Example] Additive scene load result: {additiveResult}");

            if (additiveResult)
            {
                Debug.Log($"[Example] Unloading additive scene '{uiOverlaySceneName}'...");
                bool unloadResult = await GameSceneManager.UnloadSceneAsync(uiOverlaySceneName);
                Debug.Log($"[Example] Unload scene result: {unloadResult}");
            }

            Debug.Log("=== GameSceneManager Example Completed ===");
        }

        #region EventBus Callbacks

        private void OnSceneLoadStart(SceneLoadStartEvent evt)
        {
            Debug.Log($"[EventBus Event] Scene Load Started -> Target: {evt.SceneName}, Mode: {evt.LoadMode}, LoadingScene: {evt.LoadingSceneName ?? "None"}");
        }

        private void OnSceneLoadProgress(SceneLoadProgressEvent evt)
        {
            Debug.Log($"[EventBus Event] Scene Load Progress -> Target: {evt.SceneName}, Progress: {evt.Progress * 100:F0}%");
        }

        private void OnSceneLoadComplete(SceneLoadCompleteEvent evt)
        {
            Debug.Log($"[EventBus Event] Scene Load Completed -> Target: {evt.SceneName}, Mode: {evt.LoadMode}, Time: {evt.ElapsedTime:F2}s");
        }

        private void OnSceneLoadFailed(SceneLoadFailedEvent evt)
        {
            Debug.LogWarning($"[EventBus Event] Scene Load Failed -> Target: {evt.SceneName}, Error: {evt.ErrorMessage}");
        }

        #endregion

        #region ISceneController Lifecycle Hooks

        public Task OnSceneInitAsync()
        {
            Debug.Log("[ISceneController] OnSceneInitAsync called on scene root object.");
            return Task.CompletedTask;
        }

        public Task OnSceneCleanupAsync()
        {
            Debug.Log("[ISceneController] OnSceneCleanupAsync called on scene root object.");
            return Task.CompletedTask;
        }

        #endregion
    }
}
