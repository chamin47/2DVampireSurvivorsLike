using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Event
{
    /// <summary>
    /// 게임 내 객체 간 결합도를 낮추고 메시지(Event)를 전달하는 중앙 static Event Bus 클래스입니다.
    /// 제네릭 기반 이벤트 타입(struct)을 구독, 해제, 발행할 수 있습니다.
    /// </summary>
    public static class EventBus
    {
        // 이벤트 타입(Type)별로 등록된 Subscriber(Action<TEvent>) 리스트를 저장하는 Dictionary
        private static readonly Dictionary<Type, List<Delegate>> subscriberMap = new Dictionary<Type, List<Delegate>>();

        // 멀티스레드 환경에서도 안전하게 접근할 수 있도록 동기화에 사용할 lock 개체
        private static readonly object lockObject = new object();

        /// <summary>
        /// 특정 이벤트 타입을 구독(Subscribe)합니다.
        /// 동일한 callback이 중복 등록되지 않도록 보호 처리되어 있습니다.
        /// </summary>
        /// <typeparam name="TEvent">구독할 이벤트 구조체 타입</typeparam>
        /// <param name="callback">이벤트 발생 시 호출될 콜백 메서드</param>
        public static void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : struct
        {
            if (callback == null)
            {
                Debug.LogWarning("[EventBus] Subscribe attempted with a null callback.");
                return;
            }

            Type eventType = typeof(TEvent);

            lock (lockObject)
            {
                if (!subscriberMap.TryGetValue(eventType, out var subscribers))
                {
                    subscribers = new List<Delegate>();
                    subscriberMap[eventType] = subscribers;
                }

                if (subscribers.Contains(callback))
                {
                    Debug.LogWarning($"[EventBus] Callback '{callback.Method.Name}' is already subscribed to '{eventType.Name}'. Duplicate registration skipped.");
                    return;
                }

                subscribers.Add(callback);
            }
        }

        /// <summary>
        /// 구독 중인 특정 이벤트 타입의 콜백을 해제(Unsubscribe)합니다.
        /// 등록되지 않은 콜백을 해제하더라도 오류가 발생하지 않습니다.
        /// </summary>
        /// <typeparam name="TEvent">해제할 이벤트 구조체 타입</typeparam>
        /// <param name="callback">해제할 콜백 메서드</param>
        public static void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : struct
        {
            if (callback == null) return;

            Type eventType = typeof(TEvent);

            lock (lockObject)
            {
                if (!subscriberMap.TryGetValue(eventType, out var subscribers))
                {
                    return;
                }

                subscribers.Remove(callback);

                // 더 이상 구독자가 없으면 메모리 관리를 위해 Dictionary에서 제거
                if (subscribers.Count == 0)
                {
                    subscriberMap.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 이벤트를 발생(Publish)시켜 해당 이벤트를 구독 중인 모든 Subscriber에게 전달합니다.
        /// 구독자가 없어도 오류가 발생하지 않으며, 특정 Subscriber에서 예외가 발생하더라도 다른 Subscriber의 처리에는 영향을 주지 않습니다.
        /// </summary>
        /// <typeparam name="TEvent">발행할 이벤트 구조체 타입</typeparam>
        /// <param name="eventData">전달할 이벤트 데이터</param>
        public static void Publish<TEvent>(TEvent eventData) where TEvent : struct
        {
            Type eventType = typeof(TEvent);
            Delegate[] snapshot = null;

            lock (lockObject)
            {
                if (!subscriberMap.TryGetValue(eventType, out var subscribers) || subscribers.Count == 0)
                {
                    return;
                }

                // 이벤트 처리 중 Subscribe/Unsubscribe가 일어날 때의 Collection Modified Exception 방지를 위해 스냅샷 생성
                snapshot = subscribers.ToArray();
            }

            // 스냅샷 순회 및 이벤트 전달 (lock 외부에서 실행하여 데드락 및 블로킹 방지)
            foreach (var subscriber in snapshot)
            {
                if (subscriber is Action<TEvent> action)
                {
                    try
                    {
                        action.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        // 특정 Subscriber 예외 발생 시 로그 출력 후 다른 Subscriber 처리를 계속 진행
                        Debug.LogError($"[EventBus] Exception caught during invocation of '{subscriber.Method.Name}' for event '{eventType.Name}':");
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 이벤트 타입에 등록된 모든 구독자를 비웁니다.
        /// </summary>
        /// <typeparam name="TEvent">비울 이벤트 구조체 타입</typeparam>
        public static void Clear<TEvent>() where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            lock (lockObject)
            {
                if (subscriberMap.ContainsKey(eventType))
                {
                    subscriberMap.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 등록된 모든 이벤트 타입의 구독 정보를 초기화합니다. (씬 전환 또는 전체 리셋 시 사용)
        /// </summary>
        public static void ClearAll()
        {
            lock (lockObject)
            {
                subscriberMap.Clear();
            }
        }

        /// <summary>
        /// 디버깅 및 단위 테스트용: 특정 이벤트 타입에 등록된 현재 구독자 수 반환
        /// </summary>
        /// <typeparam name="TEvent">조회할 이벤트 구조체 타입</typeparam>
        /// <returns>구독자 수</returns>
        public static int GetSubscriberCount<TEvent>() where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            lock (lockObject)
            {
                if (subscriberMap.TryGetValue(eventType, out var subscribers))
                {
                    return subscribers.Count;
                }
                return 0;
            }
        }
    }
}
