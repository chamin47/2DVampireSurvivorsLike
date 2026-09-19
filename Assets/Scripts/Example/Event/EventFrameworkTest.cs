using UnityEngine;
using Framework.Event;

namespace Example.Event
{
    /// <summary>
    /// Event Bus 시스템 동작 검증 및 예제용 MonoBehaviour 스크립트입니다.
    /// </summary>
    public class EventFrameworkTest : MonoBehaviour
    {
        private void OnEnable()
        {
            // Unity 생명주기에 맞춘 이벤트 구독
            EventBus.Subscribe<CharacterDeadEvent>(OnCharacterDead);
            EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
            EventBus.Subscribe<CityCapturedEvent>(OnCityCaptured);
        }

        private void OnDisable()
        {
            // Unity 생명주기에 맞춘 이벤트 해제 (메모리 누수 및 파괴된 객체 호출 방지)
            EventBus.Unsubscribe<CharacterDeadEvent>(OnCharacterDead);
            EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
            EventBus.Unsubscribe<CityCapturedEvent>(OnCityCaptured);
        }

        private void Start()
        {
            Debug.Log("[EventFrameworkTest] Starting EventBus test sequence...");

            // 1. 일반적인 이벤트 Publish 테스트
            EventBus.Publish(new CharacterDeadEvent
            {
                CharacterId = 1001,
                CityId = 5
            });

            EventBus.Publish(new TurnChangedEvent
            {
                Turn = 10
            });

            // 2. 중복 구독 시도 테스트 (경고 출력 후 등록 건너뜀)
            EventBus.Subscribe<CharacterDeadEvent>(OnCharacterDead);

            // 3. 예외 격리 테스트 (Subscriber 중 예외가 발생하더라도 다른 Subscriber 정상 처리)
            EventBus.Subscribe<TurnChangedEvent>(OnTurnChangedWithException);
            EventBus.Publish(new TurnChangedEvent { Turn = 11 });
            EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChangedWithException);

            // 4. 구독하지 않은 이벤트 Publish 테스트 (오류 없음)
            EventBus.Publish(new DummyEvent { Value = 99 });

            Debug.Log("[EventFrameworkTest] Test sequence completed.");
        }

        private void OnCharacterDead(CharacterDeadEvent evt)
        {
            Debug.Log($"[Subscriber A] OnCharacterDead -> CharacterId: {evt.CharacterId}, CityId: {evt.CityId}");
        }

        private void OnTurnChanged(TurnChangedEvent evt)
        {
            Debug.Log($"[Subscriber B] OnTurnChanged -> Current Turn: {evt.Turn}");
        }

        private void OnTurnChangedWithException(TurnChangedEvent evt)
        {
            Debug.Log($"[Subscriber Exception Test] Throwing intented exception on turn {evt.Turn}");
            throw new System.Exception("Intended exception in test subscriber");
        }

        private void OnCityCaptured(CityCapturedEvent evt)
        {
            Debug.Log($"[Subscriber C] OnCityCaptured -> CityId: {evt.CityId}, FactionId: {evt.FactionId}");
        }

        // 테스트용 더미 이벤트 구조체
        private struct DummyEvent
        {
            public int Value;
        }
    }
}
