using System.Threading.Tasks;

namespace Framework.Scene.Interfaces
{
    /// <summary>
    /// 씬의 루트 오브젝트에 부착하여 씬 진입 및 이탈 시 생명주기 훅을 전달받는 인터페이스입니다.
    /// </summary>
    public interface ISceneController
    {
        /// <summary>
        /// 씬이 로드되고 활성화된 후 비동기로 실행할 초기화 작업입니다.
        /// </summary>
        Task OnSceneInitAsync();

        /// <summary>
        /// 씬이 언로드되기 직전에 실행할 정리 작업입니다.
        /// </summary>
        Task OnSceneCleanupAsync();
    }
}
