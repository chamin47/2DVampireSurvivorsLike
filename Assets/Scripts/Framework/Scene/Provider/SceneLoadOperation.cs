using System;
using UnityEngine;

namespace Framework.Scene.Provider
{
    /// <summary>
    /// 진행 중인 씬 로드 작업을 추적하고 제어하는 래퍼 클래스입니다.
    /// </summary>
    public class SceneLoadOperation
    {
        public string SceneName { get; }
        public SceneLoadMode LoadMode { get; }
        public AsyncOperation UnityAsyncOp { get; internal set; }
        public bool AutoActivate { get; }

        public bool IsDone => UnityAsyncOp != null && UnityAsyncOp.isDone;

        public float RawProgress => UnityAsyncOp != null ? UnityAsyncOp.progress : 0f;

        /// <summary>
        /// allowSceneActivation이 false일 경우 AsyncOperation.progress는 0.9까지 상승합니다.
        /// 이를 0.0 ~ 1.0 범위로 정규화한 프로그래스 값입니다.
        /// </summary>
        public float NormalizedProgress
        {
            get;
            internal set;
        }

        public SceneLoadOperation(string sceneName, SceneLoadMode loadMode, bool autoActivate)
        {
            SceneName = sceneName;
            LoadMode = loadMode;
            AutoActivate = autoActivate;
            NormalizedProgress = 0f;
        }

        /// <summary>
        /// 로드 완료 후 씬 활성화를 허용합니다. (allowSceneActivation = true)
        /// </summary>
        public void Activate()
        {
            if (UnityAsyncOp != null)
            {
                UnityAsyncOp.allowSceneActivation = true;
            }
        }
    }
}
