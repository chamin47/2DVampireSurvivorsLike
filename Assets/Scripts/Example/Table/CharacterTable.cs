using System;
using Framework.Table;

namespace Example.Table
{
    /// <summary>
    /// CharacterData 행 객체들을 관리하는 테이블 클래스입니다.
    /// TableBase<CharacterData>를 상속받아 Key(id) 기반 $O(1)$ 조회를 지원합니다.
    /// </summary>
    [Serializable]
    public class CharacterTable : TableBase<CharacterData>
    {
    }
}
