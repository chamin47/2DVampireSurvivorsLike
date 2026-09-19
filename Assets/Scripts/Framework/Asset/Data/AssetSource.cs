namespace Framework.Asset
{
    /// <summary>
    /// Asset의 로딩 출처(출처 유형)를 정의하는 열거형입니다.
    /// </summary>
    public enum AssetSource
    {
        /// <summary>
        /// 자동 판별 (등록된 정의, Addressables, Resources 순서로 확인)
        /// </summary>
        Auto,

        /// <summary>
        /// Unity Resources 폴더 기반 로드
        /// </summary>
        Resources,

        /// <summary>
        /// Unity Addressables 패키지 기반 로드
        /// </summary>
        Addressables
    }
}
