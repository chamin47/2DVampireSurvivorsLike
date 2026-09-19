namespace Framework.UI
{
    /// <summary>
    /// UI의 표시 레이어를 정의하는 Enum입니다.
    /// 숫자가 높을수록 상위에 표시됩니다.
    /// </summary>
    public enum UILayer
    {
        Background = 0,
        HUD = 100,
        Popup = 200,
        Dialog = 300,
        System = 400,
        Overlay = 500
    }
}
