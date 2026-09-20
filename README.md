# 새벽 농장 생존기

농장을 포위한 언데드에게서 8분 동안 생존한 뒤 사신 보스를 처치하는 2D 자동 공격 생존 게임입니다.

## 실행 방법

1. Unity 6에서 프로젝트를 엽니다.
2. `Assets/Scenes/FarmSurvivors.unity` 씬을 엽니다.
3. Play를 누르고 캐릭터를 선택한 뒤 게임을 시작합니다.

조작은 WASD 또는 방향키이며 공격은 자동으로 진행됩니다.

## 주요 구현 기능

- 캐릭터 4종과 고유 능력치
- 일반 적 4종과 사신 보스
- 자동 공격 무기 2종(회전 곡괭이, 삽 던지기)과 패시브 강화 5종
- 경험치, 레벨업 3지선다, 회복·자석·상자 드롭
- 8분 웨이브 진행과 승리·패배 화면
- Addressables 기반 프리팹 로드와 오브젝트 풀링
- 한글·영어 전환, HUD, 사운드와 전투 효과
- Excel/CSV 변경 감지, JSON 자동 생성, 런타임 밸런스 적용

## Excel 밸런스 수정

`Assets/Datas/FarmBalance.xlsx`의 `Value` 열을 수정하고 저장하면 Unity가 자동으로
`Assets/Resources/Tables/FarmBalance.json`을 다시 생성합니다. 플레이 중 수정했다면 플레이 모드를 다시 시작해야 적용됩니다.

상세한 표 작성 규칙은 `Assets/Datas/README.md`에서 확인할 수 있습니다.

## 문서

- 전체 구현 설명과 과제 요소: `Descript.md`
- 게임 실행과 수정 위치: `Assets/README.md`
- Excel/CSV 자동화 사용법: `Assets/Datas/README.md`
