namespace Framework.Table
{
    /// <summary>
    /// 테이블의 단일 행(Row) 데이터 객체 제네릭 인터페이스입니다.
    /// </summary>
    public interface ITableData<TKey>
    {
        /// <summary>
        /// 행 데이터를 식별하는 고유 ID 식별자입니다.
        /// </summary>
        TKey Id { get; }
    }

    /// <summary>
    /// int 타입 기본 Key를 사용하는 테이블 행 데이터 인터페이스입니다.
    /// </summary>
    public interface ITableData : ITableData<int>
    {
    }
}
