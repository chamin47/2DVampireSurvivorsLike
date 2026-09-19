namespace Framework.Table
{
    /// <summary>
    /// 테이블 객체의 공통 인터페이스입니다.
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// 테이블 데이터 초기화 및 딕셔너리 맵을 구축합니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// 테이블에 포함된 데이터 개수를 반환합니다.
        /// </summary>
        int Count { get; }
    }
}
