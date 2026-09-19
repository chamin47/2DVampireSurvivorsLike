using System;

namespace Framework.Asset
{
    /// <summary>
    /// Asset의 식별 키, 로딩 출처 및 실제 내부 주소/경로를 정의하는 클래스입니다.
    /// </summary>
    [Serializable]
    public class AssetDefinition
    {
        /// <summary>
        /// 외부에서 호출 시 사용할 식별 Key (예: "Character/CaoCao")
        /// </summary>
        public string key;

        /// <summary>
        /// Asset 로드 출처 (Resources, Addressables 등)
        /// </summary>
        public AssetSource source = AssetSource.Auto;

        /// <summary>
        /// 실제 로딩에 사용할 Addressables Address 또는 Resources 상대 경로 (확장자 제외)
        /// </summary>
        public string address;

        public AssetDefinition() { }

        public AssetDefinition(string key, string address, AssetSource source = AssetSource.Auto)
        {
            this.key = key;
            this.address = string.IsNullOrEmpty(address) ? key : address;
            this.source = source;
        }
    }
}
