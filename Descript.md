# 새벽 농장 생존기 구현 설명서

## 1. 프로젝트 개요

`새벽 농장 생존기`는 밤마다 되살아나는 언데드에게 포위된 농장에서 8분간 버티고,
마지막에 등장하는 사신 보스를 처치하는 2D 뱀서라이크 형식의 자동 공격 생존 게임입니다.

- 실행 씬: `Assets/Game/Scenes/FarmSurvivors.unity`
- 게임 설정 에셋: `Assets/Game/Data/FarmGameConfig.asset`
- Excel 밸런스 원본: `Assets/Datas/FarmBalance.xlsx`
- 자동 생성 데이터: `Assets/Resources/Tables/FarmBalance.json`

## 2. 기본 플레이

- WASD 또는 방향키로 캐릭터를 이동합니다.
- 무기는 가장 가까운 적 또는 플레이어 주변의 적을 자동으로 공격합니다.
- 적을 처치하면 경험치 보석이 생성됩니다.
- 경험치를 모아 레벨이 오르면 게임이 일시정지되고 강화 선택지 3개가 표시됩니다.
- 체력이 0이 되면 패배합니다.
- 8분이 지나면 일반 적 생성이 중단되고 사신 보스가 등장합니다.
- 사신 보스를 처치하면 승리 화면이 표시됩니다.

## 3. 캐릭터

시작 화면에서 네 명의 농부 중 한 명을 선택할 수 있습니다.

| 캐릭터 | 특징 | 기본 보너스 |
| --- | --- | --- |
| Farmer 0 | 균형형 | 공격력 10% 증가 |
| Farmer 1 | 속도형 | 이동속도 15% 증가 |
| Farmer 2 | 연사형 | 재사용 대기시간 10% 감소 |
| Farmer 3 | 생존형 | 최대 체력 25% 증가 |

캐릭터 능력치는 Excel의 `Farmer0~3` 관련 Key로 변경할 수 있습니다.

## 4. 적과 웨이브

### 적 종류

| 적 | 역할 |
| --- | --- |
| Zombie | 기본 근접 적 |
| Runner | 체력이 낮지만 빠른 적 |
| Skeleton | 체력이 높은 근접 적 |
| Mummy | 투사체를 사용하는 원거리 적 |
| Reaper | 8분에 등장하는 최종 보스 |

### 진행 시간표

- 0:00~2:00: 기본 좀비 등장
- 2:00~4:00: 빠른 좀비 추가
- 4:00~6:00: 해골과 원거리 미라 추가
- 6:00 이후: 적 수와 생성 속도가 증가한 혼합 웨이브
- 7:30: 사방에서 대규모 최종 웨이브
- 8:00: 일반 적 생성을 중단하고 사신 보스 등장

적의 체력, 이동속도, 접촉 피해, 경험치, 공격 주기는 Excel에서 종류별로 조정할 수 있습니다.

## 5. 무기와 강화

### 자동 공격 무기

1. 농부의 낫
   - 가장 가까운 적 방향으로 범위 공격을 합니다.
   - 레벨에 따라 피해량과 범위가 증가하고 양방향 공격이 해금됩니다.
2. 씨앗총
   - 가장 가까운 적에게 투사체를 자동 발사합니다.
   - 레벨에 따라 탄환 수, 관통 수, 피해량, 발사 속도가 증가합니다.
3. 회전 곡괭이
   - 플레이어 주변을 회전하며 닿은 적에게 지속 피해를 줍니다.
   - 레벨에 따라 개수, 회전속도, 범위와 피해량이 증가합니다.

### 패시브 능력

- 공격력 증가
- 이동속도 증가
- 재사용 대기시간 감소
- 아이템 획득 범위 증가
- 최대 체력 증가

무기의 피해량, 쿨타임, 범위, 투사체 속도와 회전 수치는 모두 `FarmBalance.xlsx`에서 수정할 수 있습니다.

## 6. 드롭과 성장

- 모든 적은 처치 시 경험치 보석을 생성합니다.
- 적 처치 시 일정 확률로 회복 아이템이 생성됩니다.
- 낮은 확률로 화면 내 경험치를 끌어오는 자석 아이템이 생성됩니다.
- 일정 시간마다 상자가 생성됩니다.
- 상자를 획득하면 현재 보유 중인 무기 하나가 무료로 강화됩니다.
- 레벨업 시 무기 또는 패시브 능력 중 무작위 선택지 3개가 표시됩니다.

회복·자석 드롭 확률, 자석 지속시간, 상자 등장 간격도 Excel에서 변경할 수 있습니다.

## 7. 화면과 편의 기능

- 타이틀과 캐릭터 선택 화면
- 체력, 경험치, 생존 시간, 처치 수를 표시하는 전투 HUD
- 게임을 일시정지하는 레벨업 선택 화면
- 생존 시간, 레벨, 처치 수를 표시하는 결과 화면
- 한글과 영어 실시간 전환
- 무한 반복 방식의 농장 바닥
- 카메라의 플레이어 추적
- 피격 플래시, 파티클, 무기 이펙트
- BGM과 근접·원거리·피격·사망·레벨업·승패 효과음

## 8. Addressables와 프리팹 생성 원리

씬에는 `GameBootstrap` 오브젝트가 배치되어 있습니다. Play를 시작하면
`FarmGameBootstrap`이 다음 Addressables 주소를 프레임워크의 `AssetLoader`에 등록합니다.

- `farm/player`
- `farm/enemy`
- `farm/projectile`
- `farm/pickup`
- `farm/weapon-visual`

게임 진행 중 필요한 순간에 `AssetLoader.Spawn`을 호출하면 Addressables에 등록된 프리팹이 로드되고,
오브젝트 풀에서 인스턴스를 꺼내 `[Runtime Game Objects]` 하위에 생성합니다. 씬 파일에 플레이어와 적이
미리 배치되어 있지 않아도 화면에 나타나는 이유가 이 런타임 생성 구조입니다. 사용이 끝난 오브젝트는
`AssetLoader.Despawn`을 통해 풀로 반환합니다.

## 9. Excel/CSV 데이터 자동화

### 자동 처리 흐름

1. `Assets/Datas` 아래의 `.xlsx` 또는 `.csv` 파일을 수정하고 저장합니다.
2. `TableAutoImporter`가 파일의 추가·변경·이동·삭제를 감지합니다.
3. 첫 번째 워크시트를 읽어 필드 이름, 변환 타입, 데이터 행을 검사합니다.
4. 정상 데이터는 `Assets/Resources/Tables/<파일명>.json`으로 원자적으로 저장됩니다.
5. `FarmGameBootstrap`이 실행될 때 원본 ScriptableObject를 복제합니다.
6. `FarmBalanceRuntime`이 JSON을 읽어 복제된 런타임 설정에 값을 적용합니다.
7. 스테이지, 캐릭터, 적, 무기 시스템이 적용된 값을 사용합니다.

### 표 형식

- 1행: 필드 이름
- 2행: JSON 변환 타입
- 3행 이후: 데이터
- `Key`: 게임에서 찾는 고유 키
- `Type`: 값의 의미상 타입
- `Value`: 실제로 변경할 값
- `Description`: 값의 용도
- `Section`: 분류

필드 이름, 첫 번째 열의 Key, 지원 타입을 검사합니다. 중복 Key나 잘못된 형식이 발견되면 오류를 출력하고
마지막으로 정상 생성된 JSON을 유지하므로 잘못 저장한 Excel 때문에 기존 게임 데이터가 사라지지 않습니다.
Excel이 생성하는 `~$` 임시 잠금 파일은 무시합니다.

### 수동 변환

자동 변환이 필요 없거나 전체 표를 다시 생성하려면 Unity 메뉴에서
`Tools > Framework > Table > Convert All Tables to JSON`을 실행합니다.

## 10. 프레임워크 활용

- `AssetLoader`: Addressables 에셋 로드, 프리팹 생성과 반환
- Object Pool: 적, 투사체, 드롭 아이템, 무기 시각 효과 재사용
- `EventBus`: 체력, 경험치, 시간과 처치 수 변경 이벤트 전달
- `AudioPlayer`: BGM과 효과음 재생
- `GameSceneManager`: 재시작 시 현재 씬 비동기 로드
- `GameServices`: 게임 세션, 로컬라이제이션, 효과 서비스 접근
- `Table` Framework: Excel/CSV 파싱, 타입 변환, JSON 생성
- ScriptableObject: 스프라이트, 사운드와 기본 게임 설정 보관
- uGUI와 TextMeshPro: 타이틀, HUD, 선택창과 결과 화면 구성

## 11. 주요 코드 위치

- 게임 초기화와 진행: `Assets/Game/Scripts/Core/FarmGameBootstrap.cs`
- 게임 이벤트와 공용 타입: `Assets/Game/Scripts/Core/GameTypes.cs`
- 기본 설정: `Assets/Game/Scripts/Data/FarmGameConfig.cs`
- Excel 런타임 적용: `Assets/Game/Scripts/Data/FarmBalanceRuntime.cs`
- 플레이어: `Assets/Game/Scripts/Gameplay/FarmPlayer.cs`
- 적 AI: `Assets/Game/Scripts/Gameplay/FarmEnemy.cs`
- 무기와 강화: `Assets/Game/Scripts/Gameplay/FarmWeaponSystem.cs`
- 투사체: `Assets/Game/Scripts/Gameplay/FarmProjectile.cs`
- 드롭 아이템: `Assets/Game/Scripts/Gameplay/FarmPickup.cs`
- UI: `Assets/Game/Scripts/UI/FarmGameUI.cs`
- 다국어: `Assets/Game/Scripts/Presentation/FarmLocalization.cs`
- 무한 타일 바닥: `Assets/Game/Scripts/Presentation/FarmWorldTiler.cs`
- Excel 변경 감지: `Assets/Scripts/Framework/Table/Editor/TableAutoImporter.cs`
- 표 검증: `Assets/Scripts/Framework/Table/Editor/TableSchemaValidator.cs`
- JSON 내보내기: `Assets/Scripts/Framework/Table/Editor/TableJsonExporter.cs`

## 12. 테스트 기능

- F1: 즉시 레벨업
- F2: 생존 시간을 종료 시점으로 변경하고 사신 보스 즉시 등장
- F3: 플레이어 체력 완전 회복

Excel 적용 여부는 플레이 시작 시 Console에 출력되는 다음 로그로 확인할 수 있습니다.

`[FarmBalance] Applied 78 Excel values.`

## 13. 최종 과제 요소 매핑

### 필수 요소

- Unity 프로젝트 폴더 구조와 기능별 스크립트 분리
- 플레이 가능한 전체 게임 루프
- uGUI 기반 시작·전투·레벨업·결과 UI
- ScriptableObject와 Excel/JSON 기반 Data System
- Scene Manager를 이용한 재시작
- EventBus 기반 시스템 간 이벤트 전달
- 프레임워크 AudioPlayer 적용
- 프레임워크 AssetLoader 적용

### 선택 요소

- Addressables 에셋 로드
- 오브젝트 풀링
- 한글·영어 Localization
- 스프라이트 프레임 애니메이션
- 피격 플래시와 파티클 효과
- Service Locator 방식의 GameServices
- Excel/CSV 자동 파싱과 Unity 데이터 자동 생성
- 캐릭터 선택과 서로 다른 능력치
- 보스전과 시간 기반 웨이브
- 테스트 단축키

## 14. 검증 결과

- Unity C# 컴파일 오류 0개
- Excel 데이터 78행 로드 확인
- 중복 Key 0개 확인
- 플레이 모드에서 Excel 값 적용 로그 확인
- `FarmSurvivors` 씬이 빌드 실행 씬으로 등록됨
