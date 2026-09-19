using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.UI.Editor
{
    /// <summary>
    /// UI 프리팹 및 Model, ViewModel, View 스크립트를 자동 생성하는 유니티 에디터 윈도우입니다.
    /// </summary>
    public class UIGeneratorWindow : EditorWindow
    {
        private string uiName = "Shop";
        private string scriptFolderPath = "Assets/Scripts/UI";
        private string prefabFolderPath = "Assets/Prefabs/UI";
        private string nameSpaceName = "Framework.UI";
        private UILayer targetLayer = UILayer.Popup;

        [MenuItem("Tools/Framework/UI Generate")]
        public static void OpenWindow()
        {
            var window = GetWindow<UIGeneratorWindow>("UI Generator");
            window.minSize = new Vector2(450, 320);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("UI Auto Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("UI 이름과 경로를 입력하면 Model, ViewModel, View 스크립트 및 UI Prefab을 자동으로 생성합니다.", MessageType.Info);
            GUILayout.Space(10);

            uiName = EditorGUILayout.TextField("UI Name", uiName);
            scriptFolderPath = EditorGUILayout.TextField("Script Output Path", scriptFolderPath);
            prefabFolderPath = EditorGUILayout.TextField("Prefab Output Path", prefabFolderPath);
            nameSpaceName = EditorGUILayout.TextField("Namespace", nameSpaceName);
            targetLayer = (UILayer)EditorGUILayout.EnumPopup("UI Layer", targetLayer);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate UI Scripts & Prefab", GUILayout.Height(40)))
            {
                GenerateUI();
            }
        }

        private void GenerateUI()
        {
            if (string.IsNullOrWhiteSpace(uiName))
            {
                EditorUtility.DisplayDialog("Error", "UI 이름을 입력해주세요.", "OK");
                return;
            }

            // UI 이름에서 'View' 접미사가 들어간 경우 정리
            string cleanUiName = uiName.Trim();
            if (cleanUiName.EndsWith("View"))
            {
                cleanUiName = cleanUiName.Substring(0, cleanUiName.Length - 4);
            }

            string modelClassName = $"{cleanUiName}Model";
            string viewModelClassName = $"{cleanUiName}ViewModel";
            string viewClassName = $"{cleanUiName}View";

            // 경로 정리
            string normalizedScriptFolder = scriptFolderPath.Replace('\\', '/').TrimEnd('/');
            string normalizedPrefabFolder = prefabFolderPath.Replace('\\', '/').TrimEnd('/');

            if (!Directory.Exists(normalizedScriptFolder))
            {
                Directory.CreateDirectory(normalizedScriptFolder);
            }

            if (!Directory.Exists(normalizedPrefabFolder))
            {
                Directory.CreateDirectory(normalizedPrefabFolder);
            }

            string prefabPath = $"{normalizedPrefabFolder}/{viewClassName}.prefab";

            // 1. Model Script 생성
            GenerateModelScript(normalizedScriptFolder, modelClassName);

            // 2. ViewModel Script 생성
            GenerateViewModelScript(normalizedScriptFolder, viewModelClassName, modelClassName);

            // 3. View Script 생성 (Address 상수 포함)
            GenerateViewScript(normalizedScriptFolder, viewClassName, viewModelClassName, prefabPath);

            // 4. Prefab 생성
            GeneratePrefab(prefabPath, viewClassName);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"UI 생성 완료!\n\nScripts: {normalizedScriptFolder}\nPrefab: {prefabPath}", "OK");
        }

        private void GenerateModelScript(string folderPath, string modelClassName)
        {
            string filePath = Path.Combine(folderPath, $"{modelClassName}.cs");
            string code = $@"using Framework.UI;

namespace {nameSpaceName}
{{
    /// <summary>
    /// {modelClassName} UI Model 클래스입니다.
    /// </summary>
    public class {modelClassName} : ModelBase
    {{
        protected override void OnDispose()
        {{
            base.OnDispose();
        }}
    }}
}}";
            File.WriteAllText(filePath, code);
        }

        private void GenerateViewModelScript(string folderPath, string viewModelClassName, string modelClassName)
        {
            string filePath = Path.Combine(folderPath, $"{viewModelClassName}.cs");
            string code = $@"using Framework.UI;

namespace {nameSpaceName}
{{
    /// <summary>
    /// {viewModelClassName} UI ViewModel 클래스입니다.
    /// </summary>
    public class {viewModelClassName} : ViewModelBase
    {{
        private readonly {modelClassName} model;

        public {viewModelClassName}({modelClassName} model)
        {{
            this.model = model;
        }}

        protected override void OnInitialize()
        {{
            base.OnInitialize();
        }}

        protected override void OnDispose()
        {{
            model?.Dispose();
            base.OnDispose();
        }}
    }}
}}";
            File.WriteAllText(filePath, code);
        }

        private void GenerateViewScript(string folderPath, string viewClassName, string viewModelClassName, string prefabPath)
        {
            string filePath = Path.Combine(folderPath, $"{viewClassName}.cs");
            string code = $@"using Framework.UI;
using UnityEngine;

namespace {nameSpaceName}
{{
    /// <summary>
    /// {viewClassName} UI View 클래스입니다.
    /// </summary>
    public class {viewClassName} : ViewBase
    {{
        public const string Address = ""{prefabPath}"";

        protected override void OnBind(ViewModelBase viewModel)
        {{
            base.OnBind(viewModel);

            var vm = GetViewModel<{viewModelClassName}>();
            if (vm == null)
                return;
        }}

        protected override void OnUnbind(ViewModelBase viewModel)
        {{
            var vm = GetViewModel<{viewModelClassName}>();
            if (vm != null)
            {{
            }}

            base.OnUnbind(viewModel);
        }}
    }}
}}";
            File.WriteAllText(filePath, code);
        }

        private void GeneratePrefab(string prefabPath, string viewClassName)
        {
            GameObject rootGo = new GameObject(viewClassName, typeof(RectTransform));
            RectTransform rectTransform = rootGo.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
            DestroyImmediate(rootGo);
        }
    }
}
