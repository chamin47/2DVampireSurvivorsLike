using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// UILayer별 Root Transform 설정을 위한 Inspector 직렬화 구조체입니다.
    /// </summary>
    [Serializable]
    public struct LayerRootEntry
    {
        public UILayer layer;
        public Transform rootTransform;
    }

    /// <summary>
    /// 게임 내 모든 UI View의 생성, 바인딩, 표시 및 제거를 중앙 관리하는 Singleton UIManager입니다.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;

        /// <summary>
        /// UIManager의 전역 Singleton 인스턴스입니다.
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UIManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("[UIManager]");
                        instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Layer Roots")]
        [SerializeField] private List<LayerRootEntry> layerRoots = new List<LayerRootEntry>();

        [Header("View Prefabs")]
        [SerializeField] private List<ViewBase> viewPrefabs = new List<ViewBase>();

        private readonly Dictionary<Type, ViewBase> activeViews = new Dictionary<Type, ViewBase>();
        private readonly Dictionary<UILayer, Transform> layerRootMap = new Dictionary<UILayer, Transform>();
        private readonly Dictionary<Type, ViewBase> prefabMap = new Dictionary<Type, ViewBase>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMaps();
        }

        /// <summary>
        /// Inspector에서 설정한 Layer Root와 View Prefab 리스트를 Dictionary 맵으로 정리합니다.
        /// </summary>
        private void InitializeMaps()
        {
            layerRootMap.Clear();
            foreach (var entry in layerRoots)
            {
                if (entry.rootTransform != null && !layerRootMap.ContainsKey(entry.layer))
                {
                    layerRootMap.Add(entry.layer, entry.rootTransform);
                }
            }

            prefabMap.Clear();
            foreach (var prefab in viewPrefabs)
            {
                if (prefab != null)
                {
                    var type = prefab.GetType();
                    if (!prefabMap.ContainsKey(type))
                    {
                        prefabMap.Add(type, prefab);
                    }
                }
            }
        }

        /// <summary>
        /// View Prefab을 동적으로 추가하거나 등록할 수 있는 메서드입니다.
        /// </summary>
        public void RegisterPrefab<TView>(TView prefab) where TView : ViewBase
        {
            if (prefab == null) return;
            prefabMap[typeof(TView)] = prefab;
        }

        /// <summary>
        /// UI Layer Root Transform을 동적으로 등록할 수 있는 메서드입니다.
        /// </summary>
        public void RegisterLayerRoot(UILayer layer, Transform root)
        {
            if (root == null) return;
            layerRootMap[layer] = root;
        }

        /// <summary>
        /// 해당 타입의 View가 현재 열려(활성화되어) 있는지 확인합니다.
        /// </summary>
        public bool IsOpen<TView>() where TView : ViewBase
        {
            return activeViews.ContainsKey(typeof(TView));
        }

        /// <summary>
        /// 생성되어 관리 중인 View 인스턴스를 가져옵니다. 존재하지 않으면 null을 반환합니다.
        /// </summary>
        public TView Get<TView>() where TView : ViewBase
        {
            if (activeViews.TryGetValue(typeof(TView), out var view))
            {
                return view as TView;
            }
            return null;
        }

        /// <summary>
        /// View를 생성하고 ViewModel과 바인딩하여 오픈합니다.
        /// 동일한 View가 이미 열려 있다면 중복 생성하지 않고 기존 View를 반환합니다.
        /// 순서: Instantiate -> Initialize -> Bind -> Active -> ViewModel.Activate()
        /// </summary>
        public TView Open<TView, TViewModel>(TViewModel viewModel)
            where TView : ViewBase
            where TViewModel : ViewModelBase
        {
            var viewType = typeof(TView);

            if (activeViews.TryGetValue(viewType, out var existingView))
            {
                var typedView = existingView as TView;
                if (typedView != null)
                {
                    typedView.gameObject.SetActive(true);
                    viewModel?.Activate();
                    return typedView;
                }
            }

            if (!prefabMap.TryGetValue(viewType, out var prefab))
            {
                Debug.LogError($"[{nameof(UIManager)}] Prefab for View type {viewType.Name} is not registered in UIManager.");
                return null;
            }

            // Layer Root 찾기 (없을 경우 fallback으로 UIManager transform 사용)
            Transform parentRoot = null;
            if (!layerRootMap.TryGetValue(prefab.Layer, out parentRoot) || parentRoot == null)
            {
                parentRoot = transform;
            }

            // 1. Instantiate
            TView viewInstance = Instantiate(prefab, parentRoot).GetComponent<TView>();

            // 2. Initialize
            viewInstance.Initialize();

            // 3. Bind
            viewInstance.Bind(viewModel);

            // 4. Active
            viewInstance.gameObject.SetActive(true);

            // ViewModel Active 알림
            viewModel?.Activate();

            activeViews[viewType] = viewInstance;
            return viewInstance;
        }

        /// <summary>
        /// 열려 있는 View를 닫고 제거합니다.
        /// 순서: Unbind -> Destroy -> Dictionary 제거
        /// </summary>
        public void Close<TView>() where TView : ViewBase
        {
            var viewType = typeof(TView);

            if (activeViews.TryGetValue(viewType, out var view))
            {
                // 1. Unbind
                view.Unbind();

                // 2. Destroy
                Destroy(view.gameObject);

                // 3. Dictionary 제거
                activeViews.Remove(viewType);
            }
        }

        /// <summary>
        /// 생성되어 있는 View의 게임 오브젝트를 활성화(Show)합니다.
        /// </summary>
        public void Show<TView>() where TView : ViewBase
        {
            var view = Get<TView>();
            if (view != null)
            {
                view.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 생성되어 있는 View의 게임 오브젝트를 비활성화(Hide)합니다.
        /// </summary>
        public void Hide<TView>() where TView : ViewBase
        {
            var view = Get<TView>();
            if (view != null)
            {
                view.gameObject.SetActive(false);
            }
        }
    }
}
