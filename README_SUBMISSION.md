# 새벽 농장 생존기 — 최종 과제 제출 문서

## 1. 프로젝트 개요

강의에서 제작한 공용 프레임워크를 활용하여 만든 2D 자동 공격 생존 게임입니다.
플레이어는 농부를 선택한 뒤 몰려오는 언데드를 처치하며 성장하고, 8분에 등장하는 사신 보스를 처치해야 합니다.

- Unity 버전: `6000.3.20f1`
- 실행 씬: [`Assets/Scenes/FarmSurvivors.unity`](Assets/Scenes/FarmSurvivors.unity)
- 조작: `WASD` 또는 방향키
- 공격: 자동 공격
- 디버그 키: `F1` 레벨업, `F2` 보스 즉시 등장, `F3` 체력 회복

## 2. 실행 방법

1. Unity에서 프로젝트를 엽니다.
2. [`Assets/Scenes/FarmSurvivors.unity`](Assets/Scenes/FarmSurvivors.unity) 씬을 엽니다.
3. Play를 누릅니다.
4. 캐릭터를 선택하고 게임을 시작합니다.

## 3. 프로젝트 폴더 구조

```text
Assets
├─ Data                       게임 ScriptableObject 설정
├─ Datas                      Excel/CSV 원본 데이터
├─ Editor                     게임 자동 구성 Editor 도구
├─ Localization               Unity Localization 설정과 테이블
├─ Prefabs                    플레이어, 적, 투사체, 아이템 프리팹
├─ Resources
│  └─ Tables                  Excel/CSV에서 자동 생성된 JSON
├─ Scenes                     실행 씬과 프레임워크 예제 씬
├─ Scripts
│  ├─ Core                    게임 흐름, 이벤트 타입, 서비스 등록
│  ├─ Data                    게임 설정과 Excel 런타임 적용
│  ├─ Gameplay                플레이어, 적, 무기, 투사체, 아이템
│  ├─ Presentation            애니메이션, 이펙트, 다국어, 월드 표현
│  ├─ UI                      게임 전용 UI
│  ├─ Framework               강의에서 제작한 재사용 프레임워크
│  │  ├─ Asset
│  │  ├─ Audio
│  │  ├─ Effect
│  │  ├─ Event
│  │  ├─ Localization
│  │  ├─ Scene
│  │  ├─ Table
│  │  └─ UI
│  └─ Example                 프레임워크 사용 예제
├─ UI                         폰트 등 UI 에셋
├─ AddressableAssetsData      Addressables 그룹과 빌드 설정
└─ Undead Survivor            게임에 사용한 외부 아트/사운드 에셋
```

게임 코드와 프레임워크 코드는 `Assets/Scripts` 아래에서 역할별로 구분했습니다.
`Framework`는 게임 전용 코드를 참조하지 않으며, 게임 코드가 프레임워크 API를 사용하는 단방향 구조입니다.

## 4. 과제 요소 구현 현황

### 필수 요소 — 8/8 구현

- [x] 프로젝트 폴더 구조
- [x] UI System
- [x] Data System
- [x] Scene System
- [x] Event System
- [x] Audio System
- [x] Asset 관리
- [x] 기본 게임 로직

### 선택 요소 — 6/6 구현

- [x] Localization
- [x] Addressables
- [x] Animation System
- [x] Effect System
- [x] Object Pool
- [x] DI / Service Locator

## 5. 필수 요소 상세

| 프로젝트 폴더 구조  |

프레임워크, 게임 로직, 데이터, 프리팹, 씬, UI를 역할별 폴더로 분리했습니다. 

| UI System |

프레임워크 `UIManager`를 초기화하고, 게임 전용 `FarmGameUI`가 시작 화면, 캐릭터 선택, HUD, 레벨업 선택지, 결과 화면을 관리합니다.
[`UIManager.cs`](Assets/Scripts/Framework/UI/Core/UIManager.cs), 
[`FarmGameUI.cs`](Assets/Scripts/UI/FarmGameUI.cs) |

| Data System | 

`FarmGameConfig` ScriptableObject에 에셋 참조와 기본값을 보관합니다. 
`FarmBalance.xlsx`를 저장하면 JSON으로 자동 변환되며, 실행 시 JSON 값을 설정 복사본에 적용합니다. 
| [`FarmGameConfig.cs`](Assets/Scripts/Data/FarmGameConfig.cs), 
[`FarmBalanceRuntime.cs`](Assets/Scripts/Data/FarmBalanceRuntime.cs), 
[`TableAutoImporter.cs`](Assets/Scripts/Framework/Table/Editor/TableAutoImporter.cs) |

| Scene System | 
재시작할 때 프레임워크 `GameSceneManager.LoadSceneAsync`를 통해 현재 게임 씬을 다시 불러옵니다. 로딩 성공 여부에 따른 기본 SceneManager 대체 경로도 있습니다. | 
[`GameSceneManager.cs`](Assets/Scripts/Framework/Scene/Core/GameSceneManager.cs), 
[`FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs) |

| Event System | 
체력, 경험치, 생존 시간, 처치 수, 언어 변경을 구조체 이벤트로 발행합니다. UI는 해당 이벤트를 구독하여 화면을 갱신합니다. | [`EventBus.cs`](Assets/Scripts/Framework/Event/Core/EventBus.cs), 
[`GameTypes.cs`](Assets/Scripts/Core/GameTypes.cs), 
[`FarmGameUI.cs`](Assets/Scripts/UI/FarmGameUI.cs)

| Audio System | 
프레임워크 `AudioPlayer`로 BGM과 공격, 타격, 사망, 레벨업, 선택, 승리·패배 효과음을 재생합니다. 반복 효과음은 피치 변화와 재생 간격 제한을 적용했습니다. 
[`AudioPlayer.cs`](Assets/Scripts/Framework/Audio/Core/AudioPlayer.cs), 
[`FarmWeaponSystem.cs`](Assets/Scripts/Gameplay/FarmWeaponSystem.cs), 
[`FarmEnemy.cs`](Assets/Scripts/Gameplay/FarmEnemy.cs)

| Asset 관리 | 
`AssetLoader`가 에셋 정의 등록, Addressables 로드, 인스턴스 생성, 캐싱, 해제를 통합 관리합니다. 게임 오브젝트는 문자열 주소로 생성합니다.
[`AssetLoader.cs`](Assets/Scripts/Framework/Asset/Core/AssetLoader.cs), 
[`AssetHandle.cs`](Assets/Scripts/Framework/Asset/Core/AssetHandle.cs), 
[`FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs) |

| 기본 게임 로직 | 
이동, 자동 공격, 적 스폰과 AI, 경험치, 레벨업 3지선다, 패시브 강화, 아이템 드롭, 상자, 8분 웨이브, 사신 보스, 승리·패배를 구현했습니다. | 
[`FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs), 
[`FarmPlayer.cs`](Assets/Scripts/Gameplay/FarmPlayer.cs), 
[`FarmWeaponSystem.cs`](Assets/Scripts/Gameplay/FarmWeaponSystem.cs) |

## 6. 선택 요소 상세

### Localization

- 게임 화면에서 한국어와 영어를 즉시 전환할 수 있습니다.
- 언어 변경 시 `LanguageChangedEvent`가 발행되고 UI 전체가 갱신됩니다.
- Unity Localization 패키지와 프레임워크 확장 코드도 프로젝트에 포함되어 있습니다.
- 확인 파일: [`FarmLocalization.cs`](Assets/Scripts/Presentation/FarmLocalization.cs), 
[`LocalizationExtension.cs`](Assets/Scripts/Framework/Localization/LocalizationExtension.cs)

### Addressables

다음 런타임 프리팹을 Addressables 주소로 등록하여 생성합니다.

| 주소 | 프리팹 |
|---|---|
| `farm/player` | `Player.prefab` |
| `farm/enemy` | `Enemy.prefab` |
| `farm/projectile` | `Projectile.prefab` |
| `farm/pickup` | `Pickup.prefab` |
| `farm/weapon-visual` | `WeaponVisual.prefab` |

- 런타임 사용: [`FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs)

### Animation System

- `SpriteFlipbook`이 스프라이트 배열과 FPS를 받아 플레이어와 적의 이동 애니메이션을 재생합니다.
- 이동 방향에 따라 SpriteRenderer를 좌우 반전합니다.
- 사망 시 각 적의 사망 스프라이트로 전환합니다.
- 확인 파일: [`SpriteFlipbook.cs`](Assets/Scripts/Presentation/SpriteFlipbook.cs), [`FarmEnemy.cs`](Assets/Scripts/Gameplay/FarmEnemy.cs)

### Effect System

- 적 피격 시 파티클 버스트와 흰색 플래시를 출력합니다.
- 플레이어 피격 시 붉은색 플래시를 출력합니다.
- `FarmEffectService`를 서비스로 등록하여 게임 오브젝트가 직접 생성 구현에 의존하지 않도록 구성했습니다.
- 확인 파일: [`FarmEffectService.cs`](Assets/Scripts/Presentation/FarmEffectService.cs), [`FarmEnemy.cs`](Assets/Scripts/Gameplay/FarmEnemy.cs), [`FarmPlayer.cs`](Assets/Scripts/Gameplay/FarmPlayer.cs)

### Object Pool

- 플레이어, 적, 투사체, 드롭 아이템, 무기 표현을 `AssetLoader.Spawn/Despawn`으로 관리합니다.
- `AssetPool`이 비활성 인스턴스를 보관하고 재사용합니다.
- Addressables로 생성된 인스턴스는 최종 해제 시 `Addressables.ReleaseInstance`로 정리합니다.
- 확인 파일: [`AssetPool.cs`](Assets/Scripts/Framework/Asset/Pool/AssetPool.cs), [`AssetLoader.cs`](Assets/Scripts/Framework/Asset/Core/AssetLoader.cs)

### DI / Service Locator

- `IFarmGameSession` 인터페이스를 플레이어, 적, 투사체, 아이템, 무기 시스템의 `Configure` 메서드로 전달합니다.
- 구체 클래스 대신 세션 인터페이스에 의존하도록 구성했습니다.
- `GameServices`에 다국어 서비스와 이펙트 서비스를 등록하고 필요한 객체가 타입으로 조회합니다.
- 확인 파일: [`GameTypes.cs`](Assets/Scripts/Core/GameTypes.cs), [`FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs)

## 7. 데이터 자동화 흐름

```text
Assets/Datas/FarmBalance.xlsx
        ↓ 저장 또는 Unity 재임포트
TableAutoImporter / ExcelTableImporter
        ↓
Assets/Resources/Tables/FarmBalance.json
        ↓ 게임 시작
FarmBalanceRuntime.LoadAndApply
        ↓
FarmGameConfig 런타임 복사본에 밸런스 적용
```

- Excel의 `Value` 열이 밸런스 원본입니다.
- JSON 변환에 실패하면 기존 JSON을 보존합니다.
- JSON이 없거나 값이 잘못되면 ScriptableObject 기본값을 사용합니다.
- 원본 파일: [`FarmBalance.xlsx`](Assets/Datas/FarmBalance.xlsx)
- 자동 생성 결과: [`FarmBalance.json`](Assets/Resources/Tables/FarmBalance.json)

## 8. 주요 게임 기능

- 농부 캐릭터 4종과 서로 다른 능력치 보너스
- 기본 무기 `회전 곡괭이`
- 레벨업으로 획득하는 `삽 던지기`
- 공격력, 이동속도, 쿨타임, 획득 범위, 최대 체력 패시브
- 좀비, 빠른 적, 해골, 원거리 미라, 사신 보스
- 경험치, 회복, 자석, 상자 아이템
- 60초마다 상자 생성 및 보유 무기 무료 강화
- 7분 30초 대규모 웨이브
- 8분에 일반 적 생성 중단 및 사신 보스 등장
- 보스 처치 시 승리, 체력 0 시 패배

## 9. 평가자 빠른 확인 순서

1. `FarmSurvivors` 씬을 실행하고 캐릭터를 선택합니다.
2. 이동과 회전 곡괭이 자동 공격을 확인합니다.
3. `F1`을 눌러 레벨업 선택 UI와 일시정지를 확인합니다.
4. 언어 버튼을 눌러 한국어/영어 실시간 전환을 확인합니다.
5. `F2`를 눌러 일반 적 제거와 사신 보스 등장을 확인합니다.
6. 보스 처치 후 승리 결과 화면을 확인합니다.
7. `FarmBalance.xlsx`의 값을 수정하고 저장하여 JSON 자동 갱신을 확인합니다.

## 10. 핵심 진입점

- 게임 실행 씬: [`Assets/Scenes/FarmSurvivors.unity`](Assets/Scenes/FarmSurvivors.unity)
- 전체 게임 흐름: [`Assets/Scripts/Core/FarmGameBootstrap.cs`](Assets/Scripts/Core/FarmGameBootstrap.cs)
- 게임 설정: [`Assets/Data/FarmGameConfig.asset`](Assets/Data/FarmGameConfig.asset)
- 프레임워크: [`Assets/Scripts/Framework`](Assets/Scripts/Framework)
