using System;
using System.Threading.Tasks;

namespace Framework.Scene.Provider
{
    /// <summary>
    /// 씬 로드 및 언로드 로직을 추상화하는 인터페이스입니다.
    /// 기본 Unity SceneManager 외에 Addressables 기반 씬 로더 등으로 확장 가능합니다.
    /// </summary>
    public interface ISceneLoadProvider
    {
        /// <summary>
        /// 비동기로 씬을 로드합니다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름 또는 주소</param>
        /// <param name="loadMode">Single 또는 Additive</param>
        /// <param name="autoActivate">로드 완료 즉시 활성화 여부</param>
        /// <param name="onProgress">진행도(0.0~1.0) 콜백</param>
        /// <returns>씬 로드 작업 핸들</returns>
        Task<SceneLoadOperation> LoadSceneAsync(string sceneName, SceneLoadMode loadMode, bool autoActivate, Action<float> onProgress = null);

        /// <summary>
        /// 비동기로 씬을 언로드합니다.
        /// </summary>
        /// <param name="sceneName">언로드할 씬 이름</param>
        /// <param name="onProgress">진행도(0.0~1.0) 콜백</param>
        /// <returns>성공 여부</returns>
        Task<bool> UnloadSceneAsync(string sceneName, Action<float> onProgress = null);

        /// <summary>
        /// 로드된 씬 활성화를 수행합니다.
        /// </summary>
        /// <param name="operation">로드 작업 핸들</param>
        Task ActivateSceneAsync(SceneLoadOperation operation);
    }
}
