using System.Collections.Generic;
using Framework.Event;

namespace DawnFarm
{
    public sealed class FarmLocalization
    {
        private readonly Dictionary<string, string[]> terms = new Dictionary<string, string[]>
        {
            ["title"] = new[] { "새벽 농장 생존기", "DAWN FARM SURVIVORS" },
            ["subtitle"] = new[] { "해가 뜰 때까지 농장을 지켜라", "Defend the farm until sunrise" },
            ["start"] = new[] { "생존 시작", "START RUN" },
            ["choose"] = new[] { "농부를 선택하세요", "CHOOSE YOUR FARMER" },
            ["level_up"] = new[] { "레벨 업!", "LEVEL UP!" },
            ["pick_upgrade"] = new[] { "강화 하나를 선택하세요", "Choose one upgrade" },
            ["victory"] = new[] { "새벽이 밝았습니다!", "DAWN HAS BROKEN!" },
            ["defeat"] = new[] { "농장이 함락되었습니다", "THE FARM HAS FALLEN" },
            ["restart"] = new[] { "다시 도전", "TRY AGAIN" },
            ["level"] = new[] { "레벨", "LEVEL" },
            ["kills"] = new[] { "처치", "KILLS" },
            ["time"] = new[] { "생존 시간", "SURVIVAL TIME" },
            ["language"] = new[] { "ENG", "한국어" },
        };

        public bool Korean { get; private set; } = true;

        public string Text(string key)
        {
            return terms.TryGetValue(key, out var values) ? values[Korean ? 0 : 1] : key;
        }

        public string Choose(string korean, string english) => Korean ? korean : english;

        public void Toggle()
        {
            Korean = !Korean;
            EventBus.Publish(new LanguageChangedEvent());
        }
    }
}
