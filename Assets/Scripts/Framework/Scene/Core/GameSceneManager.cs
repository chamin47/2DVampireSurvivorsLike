using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework.Event;
using Framework.Scene;
using Framework.Scene.Events;
using Framework.Scene.Interfaces;
using Framework.Scene.Provider;
using Debug = UnityEngine.Debug;

namespace Framework.Scene.Core
{
    /// <summary>
    /// 게임 내 씬의 로드, 언로드, Additive 로드, Loading Scene 및 생명주기를 총괄하는 전역 Singleton Scene Manager입니다.
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        private static GameSceneManager instance;

        /// <summary>
        /// GameSceneManager 전역 Singleton 인스턴스입니다.
        /// </summary>
        public static GameSceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameSceneManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("[GameSceneManager]");
                        instance = go.AddComponent<GameSceneManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        public SceneState CurrentState { get; private set; } = SceneState.Idle;
        public bool IsLoading => CurrentState == SceneState.Loading || CurrentState == SceneState.Unloading;
        public string CurrentSceneName { get; private set; }
        public string PreviousSceneName { get; private set; }
        public float CurrentProgress { get; private set; }

        public ISceneLoadProvider Provider { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (Provider == null)
            {
                Provider = new DefaultSceneLoadProvider();
            }

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                CurrentSceneName = activeScene.name;
            }
        }

        /// <summary>
        /// 사용자 정의 SceneLoadProvider(예: AddressableSceneLoadProvider)를 등록합니다.
        /// </summary>
        public static void SetProvider(ISceneLoadProvider provider)
        {
            if (provider != null)
            {
                Instance.Provider = provider;
            }
        }

        #region Static Scene Loading APIs

        /// <summary>
        /// 지정한 씬 이름으로 비동기 씬 로드를 요청합니다.
        /// </summary>
        public static Task<bool> LoadSceneAsync(string sceneName, SceneLoadParams parameters = null)
        {
            var paramsObj = parameters ?? new SceneLoadParams(sceneName);
            paramsObj.TargetSceneName = sceneName;
            return Instance.InternalLoadSceneAsync(paramsObj);
        }

        /// <summary>
        /// SceneLoadParams 옵션을 활용하여 비동기 씬 로드를 요청합니다.
        /// </summary>
        public static Task<bool> LoadSceneAsync(SceneLoadParams parameters)
        {
            if (parameters == null)
            {
                Debug.LogError("[GameSceneManager] LoadSceneAsync failed: parameters object is null.");
                return Task.FromResult(false);
            }
            return Instance.InternalLoadSceneAsync(parameters);
        }

        /// <summary>
        /// Additive 방식으로 새로운 씬을 추가 비동기 로드합니다.
        /// </summary>
        public static Task<bool> LoadSceneAdditiveAsync(string sceneName, SceneLoadParams parameters = null)
        {
            var paramsObj = parameters ?? new SceneLoadParams(sceneName);
            paramsObj.TargetSceneName = sceneName;
            paramsObj.LoadMode = SceneLoadMode.Additive;
            return Instance.InternalLoadSceneAsync(paramsObj);
        }

        /// <summary>
        /// Additive로 로드되어 있던 특정 씬을 비동기 언로드합니다.
        /// </summary>
        public static Task<bool> UnloadSceneAsync(string sceneName)
        {
            return Instance.InternalUnloadSceneAsync(sceneName);
        }

        #endregion

        #region Internal Implementation

        private async Task<bool> InternalLoadSceneAsync(SceneLoadParams parameters)
        {
            string targetSceneName = parameters.TargetSceneName;

            // 1. 중복 요청 및 로딩 중 검사 (Lock)
            if (IsLoading)
            {
                Debug.LogWarning($"[GameSceneManager] Scene load request for '{targetSceneName}' rejected: Scene transition is already in progress.");
                EventBus.Publish(new SceneLoadFailedEvent(targetSceneName, "Duplicate transition request rejected."));
                parameters.OnCompleted?.Invoke(false);
                return false;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("[GameSceneManager] Target scene name is null or empty.");
                parameters.OnCompleted?.Invoke(false);
                return false;
            }

            CurrentState = SceneState.Loading;
            CurrentProgress = 0f;
            var stopwatch = Stopwatch.StartNew();

            EventBus.Publish(new SceneLoadStartEvent(targetSceneName, parameters.LoadMode, parameters.LoadingSceneName));

            PreviousSceneName = CurrentSceneName;
            bool isSuccess = false;
            bool isLoadingSceneUsed = !string.IsNullOrEmpty(parameters.LoadingSceneName);

            try
            {
                // 2. Loading Scene 지원 (필요 시 Additive로 로드)
                if (isLoadingSceneUsed)
                {
                    Debug.Log($"[GameSceneManager] Loading intermediate loading scene: '{parameters.LoadingSceneName}'");
                    await Provider.LoadSceneAsync(parameters.LoadingSceneName, SceneLoadMode.Additive, true);
                }

                // 3. PreLoadTask 실행
                if (parameters.PreLoadTask != null)
                {
                    Debug.Log("[GameSceneManager] Executing PreLoadTask...");
                    await parameters.PreLoadTask.Invoke();
                }

                // 4. 주 목표 씬 로드
                Action<float> progressHandler = p =>
                {
                    CurrentProgress = Mathf.Clamp01(p);
                    parameters.OnProgress?.Invoke(CurrentProgress);
                    EventBus.Publish(new SceneLoadProgressEvent(targetSceneName, CurrentProgress));
                };

                var operation = await Provider.LoadSceneAsync(targetSceneName, parameters.LoadMode, parameters.AutoActivate, progressHandler);

                // 5. 수동 활성화 대기 처리
                if (!parameters.AutoActivate && operation != null)
                {
                    await Provider.ActivateSceneAsync(operation);
                }

                CurrentSceneName = targetSceneName;

                // 6. 이전 씬 언로드 알림 및 ISceneController 정리
                if (parameters.LoadMode == SceneLoadMode.Single && !string.IsNullOrEmpty(PreviousSceneName))
                {
                    EventBus.Publish(new SceneUnloadStartEvent(PreviousSceneName));
                    await NotifySceneCleanupAsync(PreviousSceneName);
                    EventBus.Publish(new SceneUnloadCompleteEvent(PreviousSceneName));
                }

                // 7. 신규 씬 ISceneController 초기화
                await NotifySceneInitAsync(targetSceneName);

                // 8. PostLoadTask 실행
                if (parameters.PostLoadTask != null)
                {
                    Debug.Log("[GameSceneManager] Executing PostLoadTask...");
                    await parameters.PostLoadTask.Invoke();
                }

                // 9. 필요 시 미사용 Asset 해제 (AssetLoader 통합)
                if (parameters.ReleaseUnusedAssets)
                {
                    Debug.Log("[GameSceneManager] Releasing unused scene assets...");
                    try
                    {
                        Framework.Asset.AssetLoader.ReleaseSceneAssets();
                    }
                    catch (Exception assetEx)
                    {
                        Debug.LogWarning($"[GameSceneManager] AssetLoader.ReleaseSceneAssets threw exception: {assetEx.Message}");
                    }
                }

                // 10. Intermediate Loading Scene 언로드
                if (isLoadingSceneUsed)
                {
                    Debug.Log($"[GameSceneManager] Unloading intermediate loading scene: '{parameters.LoadingSceneName}'");
                    await Provider.UnloadSceneAsync(parameters.LoadingSceneName);
                }

                stopwatch.Stop();
                float elapsedTime = (float)stopwatch.Elapsed.TotalSeconds;

                CurrentState = SceneState.Completed;
                CurrentProgress = 1.0f;

                EventBus.Publish(new SceneLoadCompleteEvent(targetSceneName, parameters.LoadMode, elapsedTime));
                parameters.OnCompleted?.Invoke(true);

                isSuccess = true;
                Debug.Log($"[GameSceneManager] Scene '{targetSceneName}' successfully loaded in {elapsedTime:F2} seconds.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                CurrentState = SceneState.Failed;
                Debug.LogError($"[GameSceneManager] Exception occurred while loading scene '{targetSceneName}':\n{ex}");
                EventBus.Publish(new SceneLoadFailedEvent(targetSceneName, ex.Message));
                parameters.OnCompleted?.Invoke(false);
                isSuccess = false;
            }
            finally
            {
                CurrentState = SceneState.Idle;
            }

            return isSuccess;
        }

        private async Task<bool> InternalUnloadSceneAsync(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[GameSceneManager] Scene unload request for '{sceneName}' rejected: Scene transition is already in progress.");
                return false;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[GameSceneManager] Unload request failed: sceneName is null or empty.");
                return false;
            }

            CurrentState = SceneState.Unloading;
            EventBus.Publish(new SceneUnloadStartEvent(sceneName));

            bool success = false;
            try
            {
                await NotifySceneCleanupAsync(sceneName);
                success = await Provider.UnloadSceneAsync(sceneName);

                if (success)
                {
                    EventBus.Publish(new SceneUnloadCompleteEvent(sceneName));
                    Debug.Log($"[GameSceneManager] Scene '{sceneName}' successfully unloaded.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSceneManager] Exception during scene unload for '{sceneName}':\n{ex}");
            }
            finally
            {
                CurrentState = SceneState.Idle;
            }

            return success;
        }

        private async Task NotifySceneInitAsync(string sceneName)
        {
            var targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!targetScene.IsValid() || !targetScene.isLoaded) return;

            var rootObjects = targetScene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var controllers = root.GetComponentsInChildren<ISceneController>(true);
                foreach (var controller in controllers)
                {
                    try
                    {
                        await controller.OnSceneInitAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameSceneManager] Exception in ISceneController.OnSceneInitAsync on '{root.name}': {ex}");
                    }
                }
            }
        }

        private async Task NotifySceneCleanupAsync(string sceneName)
        {
            var targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!targetScene.IsValid() || !targetScene.isLoaded) return;

            var rootObjects = targetScene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var controllers = root.GetComponentsInChildren<ISceneController>(true);
                foreach (var controller in controllers)
                {
                    try
                    {
                        await controller.OnSceneCleanupAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameSceneManager] Exception in ISceneController.OnSceneCleanupAsync on '{root.name}': {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
