# 새벽 농장 생존기

`Assets/Scenes/FarmSurvivors.unity`를 열고 Play를 누르면 시작 화면이 표시됩니다.

## 조작과 테스트

- 이동: WASD 또는 방향키
- 공격: 자동
- 캐릭터 선택: 시작 화면의 농부 카드
- 언어 전환: 우측 상단 `ENG / 한국어`
- F1: 테스트용 즉시 레벨업
- F2: 테스트용 사신 보스 즉시 등장
- F3: 테스트용 체력 완전 회복

실제 스테이지는 8분입니다. 0~2분에는 기본 좀비, 2~4분에는 빠른 좀비, 4~6분에는 해골과 원거리 미라, 6분 이후에는 고밀도 혼합 웨이브가 등장합니다. 7분 30초에 대규모 웨이브, 8분에 일반 적 생성 중단 후 사신 보스가 등장합니다.

## 어디를 수정하면 되는가

- 플레이 밸런스: `Assets/Datas/FarmBalance.xlsx`의 `Value` 열
- 스프라이트/사운드와 기본값: `Assets/Data/FarmGameConfig.asset`
- 게임 진행/스폰/드롭/승패: `Assets/Scripts/Core/FarmGameBootstrap.cs`
- 플레이어 이동과 체력: `Assets/Scripts/Gameplay/FarmPlayer.cs`
- 적 AI: `Assets/Scripts/Gameplay/FarmEnemy.cs`
- 무기와 강화: `Assets/Scripts/Gameplay/FarmWeaponSystem.cs`
- 화면 구성: `Assets/Scripts/UI/FarmGameUI.cs`
- 다국어 문구: `Assets/Scripts/Presentation/FarmLocalization.cs`
- 에셋 재생성: Unity 메뉴 `Tools > Dawn Farm > Build Complete Game`

씬에는 `GameBootstrap` 오브젝트가 명시적으로 존재합니다. Play 시 이 컴포넌트가 프레임워크의 `AssetLoader`를 통해 `farm/player`, `farm/enemy`, `farm/projectile`, `farm/pickup`, `farm/weapon-visual` Addressables 주소를 로드하고 풀에서 인스턴스를 꺼내 씬에 배치합니다.

## 과제 요소 매핑

- 필수: 폴더 구조, uGUI UI, ScriptableObject Data, Scene Manager, EventBus, AudioPlayer, AssetLoader, 전체 게임 로직
- 선택: 한/영 Localization, Addressables, 스프라이트 Animation, Hit/Flash Effect, Object Pool, Service Locator

모든 생성 프리팹은 `Assets/Prefabs`에 있으며 Addressables 기본 그룹에 등록됩니다.

## Excel 자동화

`Assets/Datas/FarmBalance.xlsx`를 저장하면 `Assets/Resources/Tables/FarmBalance.json`이 자동으로 갱신됩니다.
플레이 시작 시 JSON의 78개 밸런스 값을 런타임 설정에 적용합니다. 세부 작성 규칙과 전체 구현 설명은
각각 `Assets/Datas/README.md`, 프로젝트 루트의 `Descript.md`에서 확인할 수 있습니다.
