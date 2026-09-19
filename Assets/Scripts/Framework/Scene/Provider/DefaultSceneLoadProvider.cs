using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Scene.Provider
{
    /// <summary>
    /// UnityEngine.SceneManagement.SceneManager를 사용하는 기본 씬 로드 제공자입니다.
    /// </summary>
    public class DefaultSceneLoadProvider : ISceneLoadProvider
    {
        public async Task<SceneLoadOperation> LoadSceneAsync(string sceneName, SceneLoadMode loadMode, bool autoActivate, Action<float> onProgress = null)
        {
            var unityMode = (loadMode == SceneLoadMode.Additive)
                ? UnityEngine.SceneManagement.LoadSceneMode.Additive
                : UnityEngine.SceneManagement.LoadSceneMode.Single;

            var operation = new SceneLoadOperation(sceneName, loadMode, autoActivate);

            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, unityMode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefaultSceneLoadProvider] LoadSceneAsync failed for '{sceneName}': {ex.Message}");
                throw;
            }

            if (asyncOp == null)
            {
                throw new InvalidOperationException($"[DefaultSceneLoadProvider] Failed to start LoadSceneAsync for '{sceneName}'. Scene might not be in Build Settings.");
            }

            operation.UnityAsyncOp = asyncOp;
            asyncOp.allowSceneActivation = autoActivate;

            // 로드 진행률 추적 loop
            while (!asyncOp.isDone)
            {
                // allowSceneActivation이 false인 경우 progress는 0.9에서 멈춤
                float normalizedProgress = autoActivate
                    ? asyncOp.progress
                    : Mathf.Clamp01(asyncOp.progress / 0.9f);

                operation.NormalizedProgress = normalizedProgress;
                onProgress?.Invoke(normalizedProgress);

                // 0.9 단계에 도달했고 수동 활성화 대기 중이면 진행 루프 탈출
                if (!autoActivate && asyncOp.progress >= 0.9f)
                {
                    operation.NormalizedProgress = 1.0f;
                    onProgress?.Invoke(1.0f);
                    break;
                }

                await Task.Yield();
            }

            return operation;
        }

        public async Task ActivateSceneAsync(SceneLoadOperation operation)
        {
            if (operation == null || operation.UnityAsyncOp == null) return;

            operation.Activate();

            while (!operation.UnityAsyncOp.isDone)
            {
                await Task.Yield();
            }

            // 활성화된 씬을 ActiveScene으로 설정
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(operation.SceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(loadedScene);
            }
        }

        public async Task<bool> UnloadSceneAsync(string sceneName, Action<float> onProgress = null)
        {
            var sceneToUnload = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
            {
                Debug.LogWarning($"[DefaultSceneLoadProvider] UnloadSceneAsync skipped. Scene '{sceneName}' is not loaded.");
                return false;
            }

            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneToUnload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefaultSceneLoadProvider] UnloadSceneAsync failed for '{sceneName}': {ex.Message}");
                return false;
            }

            if (asyncOp == null)
            {
                return false;
            }

            while (!asyncOp.isDone)
            {
                onProgress?.Invoke(asyncOp.progress);
                await Task.Yield();
            }

            onProgress?.Invoke(1.0f);
            return true;
        }
    }
}
