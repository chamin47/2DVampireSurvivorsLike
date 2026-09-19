# Excel / CSV 테이블 자동화 사용법

이 폴더의 `.xlsx` 또는 `.csv` 파일을 저장하면 Unity가 변경을 자동 감지하여
`Assets/Resources/Tables/<파일명>.json`으로 변환합니다. 추가·수정·이동·삭제가 모두 반영됩니다.

첫 번째 워크시트의 형식은 다음과 같습니다.

1. 1행: 필드 이름 (`Key`, `Type`, `Value`, `Description`, `Section`)
2. 2행: JSON 변환 타입 (`string`, `int`, `float`, `bool`, `byte`)
3. 3행 이후: 실제 데이터

주의사항:

- 구형 `.xls`가 아니라 `.xlsx`로 저장합니다.
- 1행의 필드 이름과 첫 번째 열의 Key는 중복되면 안 됩니다.
- 게임은 `Resources/Tables/FarmBalance.json`을 읽으므로 `FarmBalance.xlsx`의 이름을 바꾸지 않습니다.
- 이름이 `~$`로 시작하는 Excel 임시 잠금 파일은 자동으로 무시됩니다.
- 표 형식이나 타입이 잘못되면 마지막으로 정상 생성된 JSON을 덮어쓰지 않습니다.

자동 변환이 필요 없거나 수동으로 다시 생성하려면 Unity 메뉴에서
`Tools > Framework > Table > Convert All Tables to JSON`을 실행합니다.
