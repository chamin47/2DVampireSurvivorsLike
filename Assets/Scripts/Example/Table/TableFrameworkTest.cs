using Framework.Table;
using UnityEngine;

namespace Example.Table
{
    /// <summary>
    /// TableManager 및 CharacterTable 동작을 검증하기 위한 런타임 테스트 MonoBehaviour 클래스입니다.
    /// </summary>
    public class TableFrameworkTest : MonoBehaviour
    {
        private void Start()
        {
            TestTableManager();
        }

        public void TestTableManager()
        {
            Debug.Log("=== Starting TableManager Framework Test ===");

            // 1. TableManager를 사용하여 CharacterTable 로드
            CharacterTable characterTable = TableManager.Get<CharacterTable>();

            if (characterTable == null)
            {
                Debug.LogError("[TableFrameworkTest] Failed to load CharacterTable.");
                return;
            }

            Debug.Log($"[TableFrameworkTest] Loaded CharacterTable with total rows: {characterTable.Count}");

            // 2. ID를 통한 데이터 검색 테스트 (Requirement 7 & 15)
            int targetId = 1001;
            if (characterTable.Contains(targetId))
            {
                CharacterData character = characterTable.Get(targetId);
                Debug.Log($"[TableFrameworkTest] Found Character ID {character.Id}: Name='{character.Name}', Age={character.Age}, Command={character.Command}, Loyalty={character.Loyalty}, IsFemale={character.IsFemale}");
            }
            else
            {
                Debug.LogWarning($"[TableFrameworkTest] Character ID {targetId} not found.");
            }

            // 3. 또 다른 ID 검색 테스트 (Sun Shangxiang)
            int targetId2 = 1002;
            if (characterTable.TryGet(targetId2, out var character2))
            {
                Debug.Log($"[TableFrameworkTest] Found Character ID {character2.Id}: Name='{character2.Name}', Age={character2.Age}, Command={character2.Command}, Loyalty={character2.Loyalty}, IsFemale={character2.IsFemale}");
            }

            // 4. 전체 목록 순회 테스트
            Debug.Log("=== All Characters in Table ===");
            foreach (var charData in characterTable.GetAll())
            {
                Debug.Log($" - [{charData.Id}] {charData.Name} (Age: {charData.Age}, Command: {charData.Command}, Loyalty: {charData.Loyalty}, Female: {charData.IsFemale})");
            }
        }
    }
}
