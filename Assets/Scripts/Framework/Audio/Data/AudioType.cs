namespace Framework.Audio
{
    /// <summary>
    /// 게임 내에서 식별 가능한 오디오 클립의 키(Key) Enum 타입입니다.
    /// </summary>
    public enum AudioType
    {
        None = 0,

        // BGM
        MainTheme = 100,
        Battle = 101,
        Lobby = 102,
        StageSelect = 103,
        Victory = 104,
        Defeat = 105,

        // SFX
        ButtonClick = 200,
        Attack = 201,
        Hit = 202,
        Critical = 203,
        Skill = 204,
        ItemGet = 205,
        Error = 206
    }
}
