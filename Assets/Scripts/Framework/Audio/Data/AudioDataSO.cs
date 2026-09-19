using System.Collections.Generic;
using UnityEngine;

namespace Framework.Audio
{
    /// <summary>
    /// AudioType별 AudioDefinition 목록을 관리하는 ScriptableObject 데이터 자산입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioData", menuName = "Framework/Audio/Audio Data", order = 1)]
    public class AudioDataSO : ScriptableObject
    {
        [SerializeField]
        private List<AudioDefinition> definitions = new List<AudioDefinition>();

        private readonly Dictionary<AudioType, AudioDefinition> dictionary = new Dictionary<AudioType, AudioDefinition>();
        private bool isInitialized = false;

        public IReadOnlyList<AudioDefinition> Definitions => definitions;

        /// <summary>
        /// 내부 Dictionary 맵을 빌드하여 빠른 검색(O(1))이 가능하도록 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            dictionary.Clear();
            if (definitions != null)
            {
                foreach (var def in definitions)
                {
                    if (def != null && def.type != AudioType.None)
                    {
                        if (!dictionary.ContainsKey(def.type))
                        {
                            dictionary.Add(def.type, def);
                        }
                        else
                        {
                            Debug.LogWarning($"[AudioDataSO] Duplicate AudioType '{def.type}' found in '{name}'. Overwriting entry.");
                            dictionary[def.type] = def;
                        }
                    }
                }
            }
            isInitialized = true;
        }

        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// 지정된 AudioType에 해당하는 AudioDefinition을 찾습니다.
        /// </summary>
        public bool TryGetDefinition(AudioType type, out AudioDefinition definition)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            return dictionary.TryGetValue(type, out definition);
        }

        /// <summary>
        /// 동적으로 오디오 정의를 추가하거나 갱신합니다.
        /// </summary>
        public void RegisterDefinition(AudioDefinition definition)
        {
            if (definition == null || definition.type == AudioType.None) return;

            if (!isInitialized)
            {
                Initialize();
            }

            dictionary[definition.type] = definition;

            // List에도 없으면 추가
            int index = definitions.FindIndex(d => d.type == definition.type);
            if (index >= 0)
            {
                definitions[index] = definition;
            }
            else
            {
                definitions.Add(definition);
            }
        }
    }
}
