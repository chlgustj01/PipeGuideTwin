# STEP 2 — 정합성 결과 시각화 검증

2026-10-23 / Asia/Seoul / Unity 6000.3.26f1 / URP.

## 환경 및 저장 상태

- Scene: Assets/PipeGuideTwin/Scenes/PipeGuide_Main.unity
- 저장된 시작 Stage: Model3D. Play Mode에서 정합성 검사 메뉴로 전환.
- Unity MCP 최종 확인: isDirty=false, isCompiling=false, isPlaying=false.
- Console Errors=0, Warnings=5. Missing Script=0.
- 경고 5개는 외부 MCP 패키지의 로그 정리 2건 및 연결 불가 3건이며, 패키지는 수정하지 않음.
- 기존 Root 5개 유지. _WORLD/ValidationVisualizationRoot 추가.
- 장비 3개, 배관 4개 유지. PIPE_005 모델/Selectable 없음.
- SampleScene SHA256: 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA (변경 없음).

## Mock 데이터

| Object | Status | Category | Expected | Actual | Severity |
|---|---|---|---|---|---|
| PIPE_001 | Match | General | 200 mm | 200 mm | None |
| PIPE_002 | Warning | Material | Carbon Steel | Stainless Steel | Medium |
| PIPE_003 | Mismatch | Diameter | 200 mm | 150 mm | High |
| PIPE_004 | Duplicate | Duplicate Pipe | 1 Pipe | 2 Pipes | Medium |
| PIPE_005 | Missing | Missing Pipe | PIPE_005 exists | Not found | High |

누락 예상 위치는 (-1800,1700,-2400) mm이며, 공유 변환기를 거쳐 (-1.8,1.7,-2.4) m에 표시.
Marker는 ConsistencyCheck 실행 중에만 생성되며, 실제 PIPE_005를 생성하지 않음.

## Play Mode 검증

PlayModeResults.json: 최종 46개 검증 PASS, 1920×1080.
Unity Input System의 Dynamic update 직전에 가상 마우스 이벤트를 주입하고 실제
Navigation/Button/EventSystem 및 SelectionManager.Update 경로를 검증함.

- 정합성 Stage, 문제 버튼 4개, 결과 기반 요약, 정상 항목 목록 제외.
- Warning/Mismatch/Duplicate Material, 정상 기본 Material, 누락 Marker.
- 문제 목록에서 PIPE_003/004/002 선택 및 한글 Expected/Actual/Severity 표시.
- 누락 문제 선택 시 실제 Object 선택 없음 + 빨간 Marker의 Cyan 테두리 + 누락 Inspector.
- 3D 직접 선택 → 결과 Inspector. 빈 공간 선택 해제 후 Validation Material 유지.
- Model3D 복귀 → 일반 Inspector 및 기본 Material, Validation 데이터 유지.
- Orbit/Pan/Zoom 회귀 검증.
- 결과 목록 교체 → PIPE_001/002의 상태·목록·Material 변화 및 원래 Mock 복원.

Overview.png, Mismatch.png, Missing.png는 Play Mode 캡처이며 시각 검토 완료.
Missing Marker와 문제 색상, Cyan 선택 테두리가 함께 구분됨.

## 문제 → 원인 → 수정 → 결과

- 선택 시 상태 색상 소실 가능성 → 기존 전체 Material 선택 덮어쓰기 →
  정합성 Stage에서 독립 Cyan bounds 테두리와 기반 Material 분리 →
  선택 중 및 선택 해제 후 상태 색상 유지 검증 PASS.
- 초기 테두리 가독성 부족 → 조명 영향을 받는 Lit Material/얇은 선 →
  Cyan 선택 Material을 Unlit으로 변경하고 선폭 보강 → 캡처에서 선명함 확인.
- 입력 검증 간헐 실패 → 에디터 입력 갱신이 가상 입력을 먼저 소비하고 초기 해상도 변동 →
  화면 크기 안정화 대기 및 Dynamic update에서 이벤트 주입 → 최종 46개 PASS.

## 역할 경계

정합성 판정/Tag/직경/재질/연결 비교, 누락·중복 탐지, Routing, Import,
압력손실/비용 계산, Approval/Revision 미구현. B Mock 결과 표시와 선택만 구현.
ReplaceResults는 향후 B 결과 입력 경계이며 실제 B 연동은 하지 않음.
STEP 3는 시작하지 않음.
