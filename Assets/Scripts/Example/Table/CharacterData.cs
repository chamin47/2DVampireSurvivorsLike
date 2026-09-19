using System;
using Framework.Table;
using UnityEngine;

namespace Example.Table
{
    /// <summary>
    /// 캐릭터 테이블의 단일 행(Row)을 표현하는 데이터 객체 예시입니다.
    /// </summary>
    [Serializable]
    public class CharacterData : ITableData
    {
        [SerializeField] public int id;
        [SerializeField] public string name;
        [SerializeField] public byte age;
        [SerializeField] public int command;
        [SerializeField] public float loyalty;
        [SerializeField] public bool isFemale;

        public int Id => id;
        public string Name => name;
        public byte Age => age;
        public int Command => command;
        public float Loyalty => loyalty;
        public bool IsFemale => isFemale;
    }
}
