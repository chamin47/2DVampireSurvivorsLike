using System;
using Framework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace DawnFarm
{
    public sealed class FarmGameUI : MonoBehaviour
    {
        private FarmGameConfig config;
        private FarmLocalization localization;
        private Action<int> selectCharacter;
        private Action startGame;
        private Action restartGame;
        private Action<UpgradeKind> chooseUpgrade;

        private GameObject titlePanel;
        private GameObject hudPanel;
        private GameObject levelPanel;
        private GameObject resultPanel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI subtitleText;
        private TextMeshProUGUI chooseText;
        private TextMeshProUGUI languageText;
        private TextMeshProUGUI startText;
        private TextMeshProUGUI timeText;
        private TextMeshProUGUI killText;
        private TextMeshProUGUI levelText;
        private TextMeshProUGUI hpText;
        private TextMeshProUGUI levelUpTitle;
        private TextMeshProUGUI levelUpSubtitle;
        private TextMeshProUGUI resultTitle;
        private TextMeshProUGUI resultStats;
        private TextMeshProUGUI restartText;
        private UnityEngine.UI.Image hpFill;
        private UnityEngine.UI.Image xpFill;
        private UnityEngine.UI.Image[] characterCards;
        private TextMeshProUGUI[] characterNames;
        private TextMeshProUGUI[] characterBonuses;
        private UnityEngine.UI.Button[] upgradeButtons;
        private TextMeshProUGUI[] upgradeTitles;
        private TextMeshProUGUI[] upgradeDescriptions;
        private UpgradeOption[] currentChoices;
        private int selectedCharacter;
        private bool lastResultWon;

        public void Build(FarmGameConfig gameConfig, FarmLocalization gameLocalization, Action<int> onSelectCharacter, Action onStart, Action onRestart, Action<UpgradeKind> onChooseUpgrade)
        {
            config = gameConfig;
            localization = gameLocalization;
            selectCharacter = onSelectCharacter;
            startGame = onStart;
            restartGame = onRestart;
            chooseUpgrade = onChooseUpgrade;
            EnsureEventSystem();
            BuildCanvas();
            EventBus.Subscribe<LanguageChangedEvent>(OnLanguageChanged);
            EventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<ExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<RunStatsChangedEvent>(OnRunStatsChanged);
            RefreshLanguage();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            titlePanel = CreatePanel("TitleScreen", canvasObject.transform, new Color(0.035f, 0.055f, 0.07f, 0.96f));
            BuildTitle(titlePanel.transform);
            hudPanel = CreatePanel("BattleHUD", canvasObject.transform, Color.clear);
            BuildHud(hudPanel.transform);
            levelPanel = CreatePanel("LevelUpScreen", canvasObject.transform, new Color(0.015f, 0.025f, 0.035f, 0.88f));
            BuildLevelUp(levelPanel.transform);
            resultPanel = CreatePanel("ResultScreen", canvasObject.transform, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            BuildResult(resultPanel.transform);
            ShowTitle(0);
        }

        private void BuildTitle(Transform parent)
        {
            titleText = CreateText("Title", parent, "", 76, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.80f), new Vector2(0.9f, 0.94f), Vector2.zero, Vector2.zero, new Color(1f, 0.84f, 0.3f));
            subtitleText = CreateText("Subtitle", parent, "", 28, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0.15f, 0.73f), new Vector2(0.85f, 0.81f), Vector2.zero, Vector2.zero, new Color(0.84f, 0.9f, 0.78f));
            chooseText = CreateText("ChooseLabel", parent, "", 32, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.64f), new Vector2(0.8f, 0.71f), Vector2.zero, Vector2.zero, Color.white);

            characterCards = new UnityEngine.UI.Image[4];
            characterNames = new TextMeshProUGUI[4];
            characterBonuses = new TextMeshProUGUI[4];
            for (int i = 0; i < 4; i++)
            {
                int captured = i;
                float left = 0.12f + i * 0.195f;
                var card = CreateButton($"FarmerCard{i}", parent, "", () => { selectedCharacter = captured; selectCharacter?.Invoke(captured); RefreshCharacterCards(); },
                    new Vector2(left, 0.30f), new Vector2(left + 0.175f, 0.62f), new Color(0.12f, 0.17f, 0.17f, 0.98f));
                characterCards[i] = card.GetComponent<UnityEngine.UI.Image>();
                var portraitObject = CreateImage("Portrait", card.transform, Color.white, new Vector2(0.18f, 0.30f), new Vector2(0.82f, 0.92f));
                portraitObject.preserveAspect = true;
                var character = config.GetCharacter(i);
                portraitObject.sprite = character?.standSprites != null && character.standSprites.Length > 0 ? character.standSprites[0] : null;
                characterNames[i] = CreateText("Name", card.transform, "", 25, FontStyles.Bold, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.17f), new Vector2(0.95f, 0.32f), Vector2.zero, Vector2.zero, new Color(1f, 0.84f, 0.3f));
                characterBonuses[i] = CreateText("Bonus", card.transform, "", 18, FontStyles.Normal, TextAlignmentOptions.Center,
                    new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.18f), Vector2.zero, Vector2.zero, new Color(0.82f, 0.88f, 0.82f));
            }

            var startButton = CreateButton("StartButton", parent, "", () => startGame?.Invoke(),
                new Vector2(0.37f, 0.12f), new Vector2(0.63f, 0.23f), new Color(0.72f, 0.23f, 0.16f, 1f));
            startText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            var languageButton = CreateButton("LanguageButton", parent, "", () => localization.Toggle(),
                new Vector2(0.86f, 0.90f), new Vector2(0.97f, 0.97f), new Color(0.12f, 0.18f, 0.2f, 0.95f));
            languageText = languageButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildHud(Transform parent)
        {
            CreateImage("XPBack", parent, new Color(0.05f, 0.08f, 0.08f, 0.92f), new Vector2(0.18f, 0.955f), new Vector2(0.82f, 0.988f));
            xpFill = CreateImage("XPFill", parent, new Color(0.35f, 0.9f, 0.9f, 1f), new Vector2(0.18f, 0.955f), new Vector2(0.82f, 0.988f));
            xpFill.type = UnityEngine.UI.Image.Type.Filled;
            xpFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            xpFill.fillOrigin = 0;
            xpFill.fillAmount = 0f;
            levelText = CreateText("Level", parent, "LV 1", 22, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.43f, 0.948f), new Vector2(0.57f, 0.995f), Vector2.zero, Vector2.zero, Color.white);

            CreateImage("HPBack", parent, new Color(0.06f, 0.08f, 0.08f, 0.9f), new Vector2(0.025f, 0.87f), new Vector2(0.23f, 0.92f));
            hpFill = CreateImage("HPFill", parent, new Color(0.82f, 0.18f, 0.18f, 1f), new Vector2(0.025f, 0.87f), new Vector2(0.23f, 0.92f));
            hpFill.type = UnityEngine.UI.Image.Type.Filled;
            hpFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            hpFill.fillOrigin = 0;
            hpText = CreateText("HPText", parent, "100 / 100", 22, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.025f, 0.865f), new Vector2(0.23f, 0.928f), Vector2.zero, Vector2.zero, Color.white);
            timeText = CreateText("Timer", parent, "00:00", 40, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.42f, 0.875f), new Vector2(0.58f, 0.95f), Vector2.zero, Vector2.zero, new Color(1f, 0.86f, 0.34f));
            killText = CreateText("Kills", parent, "KILLS 0", 25, FontStyles.Bold, TextAlignmentOptions.Right,
                new Vector2(0.77f, 0.865f), new Vector2(0.97f, 0.93f), Vector2.zero, Vector2.zero, Color.white);
            CreateText("Controls", parent, "WASD / ARROWS", 18, FontStyles.Normal, TextAlignmentOptions.BottomLeft,
                new Vector2(0.025f, 0.02f), new Vector2(0.22f, 0.07f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.55f));
        }

        private void BuildLevelUp(Transform parent)
        {
            levelUpTitle = CreateText("LevelUpTitle", parent, "", 64, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.74f), new Vector2(0.8f, 0.87f), Vector2.zero, Vector2.zero, new Color(1f, 0.82f, 0.2f));
            levelUpSubtitle = CreateText("LevelUpSubtitle", parent, "", 25, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.68f), new Vector2(0.8f, 0.75f), Vector2.zero, Vector2.zero, Color.white);
            upgradeButtons = new UnityEngine.UI.Button[3];
            upgradeTitles = new TextMeshProUGUI[3];
            upgradeDescriptions = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                int captured = i;
                float left = 0.16f + i * 0.24f;
                var button = CreateButton($"UpgradeCard{i}", parent, "", () => SelectUpgrade(captured),
                    new Vector2(left, 0.28f), new Vector2(left + 0.20f, 0.64f), new Color(0.1f, 0.16f, 0.17f, 0.98f));
                upgradeButtons[i] = button;
                upgradeTitles[i] = CreateText("UpgradeTitle", button.transform, "", 29, FontStyles.Bold, TextAlignmentOptions.Center,
                    new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.90f), Vector2.zero, Vector2.zero, new Color(1f, 0.82f, 0.2f));
                upgradeDescriptions[i] = CreateText("UpgradeDescription", button.transform, "", 21, FontStyles.Normal, TextAlignmentOptions.Center,
                    new Vector2(0.09f, 0.16f), new Vector2(0.91f, 0.62f), Vector2.zero, Vector2.zero, new Color(0.86f, 0.92f, 0.86f));
            }
        }

        private void BuildResult(Transform parent)
        {
            resultTitle = CreateText("ResultTitle", parent, "", 66, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.18f, 0.65f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero, new Color(1f, 0.82f, 0.2f));
            resultStats = CreateText("ResultStats", parent, "", 30, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.39f), new Vector2(0.75f, 0.64f), Vector2.zero, Vector2.zero, Color.white);
            var restartButton = CreateButton("RestartButton", parent, "", () => restartGame?.Invoke(),
                new Vector2(0.38f, 0.22f), new Vector2(0.62f, 0.33f), new Color(0.72f, 0.23f, 0.16f, 1f));
            restartText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ShowTitle(int characterIndex)
        {
            selectedCharacter = characterIndex;
            titlePanel.SetActive(true);
            hudPanel.SetActive(false);
            levelPanel.SetActive(false);
            resultPanel.SetActive(false);
            RefreshCharacterCards();
        }

        public void ShowPlaying()
        {
            titlePanel.SetActive(false);
            hudPanel.SetActive(true);
            levelPanel.SetActive(false);
            resultPanel.SetActive(false);
        }

        public void ShowLevelUp(UpgradeOption[] choices)
        {
            currentChoices = choices;
            levelPanel.SetActive(true);
            for (int i = 0; i < upgradeButtons.Length; i++) upgradeButtons[i].gameObject.SetActive(i < choices.Length);
            RefreshUpgradeCards();
        }

        public void HideLevelUp() => levelPanel.SetActive(false);

        public void ShowResult(bool won, float elapsed, int level, int kills)
        {
            lastResultWon = won;
            titlePanel.SetActive(false);
            hudPanel.SetActive(false);
            levelPanel.SetActive(false);
            resultPanel.SetActive(true);
            resultStats.text = BuildResultStats(elapsed, level, kills);
            RefreshLanguage();
        }

        private string BuildResultStats(float elapsed, int level, int kills)
        {
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            return $"{localization.Text("time")}  {minutes:00}:{seconds:00}\n{localization.Text("level")}  {level}\n{localization.Text("kills")}  {kills}";
        }

        private void SelectUpgrade(int index)
        {
            if (currentChoices == null || index < 0 || index >= currentChoices.Length) return;
            chooseUpgrade?.Invoke(currentChoices[index].Kind);
        }

        private void OnHealthChanged(HealthChangedEvent value)
        {
            if (hpFill != null) hpFill.fillAmount = value.Maximum <= 0f ? 0f : value.Current / value.Maximum;
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(value.Current)} / {Mathf.CeilToInt(value.Maximum)}";
        }

        private void OnExperienceChanged(ExperienceChangedEvent value)
        {
            if (xpFill != null) xpFill.fillAmount = value.Required <= 0 ? 0f : (float)value.Current / value.Required;
            if (levelText != null) levelText.text = $"LV {value.Level}";
        }

        private void OnRunStatsChanged(RunStatsChangedEvent value)
        {
            int minutes = Mathf.FloorToInt(value.Elapsed / 60f);
            int seconds = Mathf.FloorToInt(value.Elapsed % 60f);
            if (timeText != null) timeText.text = $"{minutes:00}:{seconds:00}";
            if (killText != null) killText.text = $"{localization.Text("kills")} {value.Kills}";
        }

        private void OnLanguageChanged(LanguageChangedEvent value) => RefreshLanguage();

        private void RefreshLanguage()
        {
            if (titleText == null) return;
            titleText.text = localization.Text("title");
            subtitleText.text = localization.Text("subtitle");
            chooseText.text = localization.Text("choose");
            startText.text = localization.Text("start");
            languageText.text = localization.Text("language");
            levelUpTitle.text = localization.Text("level_up");
            levelUpSubtitle.text = localization.Text("pick_upgrade");
            resultTitle.text = localization.Text(lastResultWon ? "victory" : "defeat");
            restartText.text = localization.Text("restart");
            RefreshCharacterCards();
            RefreshUpgradeCards();
        }

        private void RefreshCharacterCards()
        {
            if (characterCards == null) return;
            for (int i = 0; i < characterCards.Length; i++)
            {
                var character = config.GetCharacter(i);
                characterCards[i].color = i == selectedCharacter ? new Color(0.75f, 0.28f, 0.16f, 1f) : new Color(0.12f, 0.17f, 0.17f, 0.98f);
                characterNames[i].text = character == null ? $"Farmer {i}" : localization.Choose(character.koreanName, character.englishName);
                characterBonuses[i].text = character == null ? "" : localization.Choose(character.koreanBonus, character.englishBonus);
            }
        }

        private void RefreshUpgradeCards()
        {
            if (currentChoices == null || upgradeTitles == null) return;
            for (int i = 0; i < currentChoices.Length && i < upgradeTitles.Length; i++)
            {
                var choice = currentChoices[i];
                upgradeTitles[i].text = localization.Choose(choice.KoreanTitle, choice.EnglishTitle) + $"  Lv.{choice.NextLevel}";
                upgradeDescriptions[i].text = localization.Choose(choice.KoreanDescription, choice.EnglishDescription);
            }
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            var image = panel.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.01f;
            return panel;
        }

        private UnityEngine.UI.Button CreateButton(string name, Transform parent, string label, Action callback, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            buttonObject.transform.SetParent(parent, false);
            SetAnchors(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var image = buttonObject.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.sprite = config.panelSprite;
            image.type = config.panelSprite != null && config.panelSprite.border.sqrMagnitude > 0f ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            button.colors = colors;
            button.onClick.AddListener(() => callback?.Invoke());
            CreateText("Label", buttonObject.transform, label, 28, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(10f, 6f), new Vector2(-10f, -6f), Color.white);
            return button;
        }

        private UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            imageObject.transform.SetParent(parent, false);
            SetAnchors(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var image = imageObject.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string value, float size, FontStyles style, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            SetAnchors(textObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = config.font != null ? config.font : TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = (min + max) * 0.5f;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            var systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (systems.Length > 0) return;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<LanguageChangedEvent>(OnLanguageChanged);
            EventBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<ExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Unsubscribe<RunStatsChangedEvent>(OnRunStatsChanged);
        }
    }
}
