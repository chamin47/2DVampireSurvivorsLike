namespace Framework.Event
{
    /// <summary>
    /// 캐릭터가 사망했을 때 발생되는 이벤트 구조체입니다.
    /// </summary>
    public struct CharacterDeadEvent
    {
        public int CharacterId;
        public int CityId;
    }

    /// <summary>
    /// 점령지가 함락되었을 때 발생되는 이벤트 구조체입니다.
    /// </summary>
    public struct CityCapturedEvent
    {
        public int CityId;
        public int FactionId;
    }

    /// <summary>
    /// 턴이 변경되었을 때 발생되는 이벤트 구조체입니다.
    /// </summary>
    public struct TurnChangedEvent
    {
        public int Turn;
    }
}
