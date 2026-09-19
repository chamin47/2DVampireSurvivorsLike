using System.Collections.Generic;
using UnityEngine;

namespace Framework.Effect
{
    /// <summary>
    /// EffectType과 EffectDefinition 매핑 정보 및 이펙트 기본 설정을 저장하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EffectData", menuName = "Framework/Effect/EffectData")]
    public class EffectDataSO : ScriptableObject
    {
        [SerializeField]
        private List<EffectDefinition> definitions = new List<EffectDefinition>();

        private readonly Dictionary<EffectType, EffectDefinition> definitionDict = new Dictionary<EffectType, EffectDefinition>();

        public List<EffectDefinition> Definitions => definitions;

        /// <summary>
        /// 런타임 조회를 위해 List 데이터를 Dictionary로 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            definitionDict.Clear();
            foreach (var def in definitions)
            {
                if (def != null && def.type != EffectType.None)
                {
                    definitionDict[def.type] = def;
                }
            }
        }

        /// <summary>
        /// EffectType에 해당하는 EffectDefinition을 반환합니다.
        /// </summary>
        public bool TryGetDefinition(EffectType type, out EffectDefinition definition)
        {
            if (definitionDict.Count == 0 && definitions.Count > 0)
            {
                Initialize();
            }

            return definitionDict.TryGetValue(type, out definition);
        }

        /// <summary>
        /// 특정 EffectDefinition을 동적으로 추가하거나 업데이트합니다.
        /// </summary>
        public void RegisterDefinition(EffectDefinition def)
        {
            if (def == null || def.type == EffectType.None) return;

            definitionDict[def.type] = def;

            var existingIndex = definitions.FindIndex(d => d.type == def.type);
            if (existingIndex >= 0)
            {
                definitions[existingIndex] = def;
            }
            else
            {
                definitions.Add(def);
            }
        }
    }
}
