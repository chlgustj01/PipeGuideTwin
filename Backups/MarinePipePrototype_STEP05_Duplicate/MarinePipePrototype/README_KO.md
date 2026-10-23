# Unity Marine Pipe Prototype (Unity 2022.3 LTS)

실행 가능한 Unity C# 데모 소스. GitHub의 Ivan Murzak Unity-AI-Splines 소스는 참고 자료이고, 이 패키지의 코드는 자체 구현입니다. Unity Editor에서의 컴파일/실행은 이 환경에서 직접 테스트하지 못했습니다.

## 설치

1. Unity 2022.3 프로젝트를 엽니다.
2. 이 ZIP의 `Assets/MarinePipe` 폴더를 프로젝트의 `Assets/` 안으로 복사합니다. 기존 동일 이름 파일은 백업하세요.
3. Unity가 C# 컴파일을 완료하면 `Tools > Marine Pipe > 01 - Build Industrial Pipe Demo` 메뉴를 실행합니다.
4. Scene 뷰에서 확인하거나 Play를 눌러 Game 뷰에서 확인합니다. 메뉴가 생성하는 `MarinePipePreviewCamera`와 `MarinePipeSun`이 Game 뷰를 지원합니다.
5. 반드시 Ctrl+S로 Scene을 저장합니다.

## 데모 구성

- 기본 72개 (3층 × 24개) 컬러 계통 배관, 7개 수직 횡단 헤더, 4개 펌프 스키드
- 이론 교차점(points) 사이에 정해진 bendRadiusM 기준 원호 필렛을 삽입, 12 samples/90 degrees
- 원형 외부/내부 벽과 양 끝 환형 캡으로 중공 Mesh 구성
- 배관 양 끝 플랜지, 일부 밸브, 구조 철골/랙/갑판
- 카메라/조명 자동 생성

## JSON 배관 입력

`Tools > Marine Pipe > 02 - Import JSON Database`에서 `Samples/sample_pipe_database.json`을 선택합니다.

- JSON 필드: pipes[], 각 항목 `id`, `system`, `outerDiameterM`, `wallThicknessM`, `bendRadiusM`, `waypoints`[{x,y,z}], `valve`
- 좌표는 현재 데모 루트 기준 미터, Y는 높이입니다. CAD에서 가져온 선박 좌표는 별도의 변환이 필요합니다.
- `waypoints`는 실제 엘보의 시작/끝이 아니라 직선 연장선의 이론적 교점입니다.
- 지나치게 짧은 선분은 엘보 반경이 자동 축소되므로 설계 검증 없이 실선 제작 데이터로 사용하면 안 됩니다.
- import는 우선 시각화용입니다. Pipe spec 배치/산업 규격/분기관/충돌 해석은 아직 구현하지 않았습니다.

## 소스 파일

- Runtime/MarinePipeData.cs : JsonUtility용 데이터 모델 및 모델 태그
- Runtime/MarinePipeGeometry.cs : 3D 원호 필렛, 일정 단면의 중공 메쉬 생성
- Runtime/MarinePipeSceneBuilder.cs : 3층 랙, 배관, 펌프 및 피팅 데모 구성
- Editor/MarinePipeMenu.cs : Unity Editor 메뉴 및 JSON 임포트

## 현재 한계와 다음 단계

- Unity Splines 패키지 없이도 동작하도록 `waypoints → 원호 샘플 → hollow sweep` 직접 구현. Unity-AI-Splines와의 MCP 제어 연결은 아직 미구현.
- 배관은 연속 메시이지만 플랜지와 밸브는 데모용 primitive 형상; 정밀 모델 및 단면 절단/연결은 추가 필요.
- 엘보는 waypoint 기준 R 값을 이용하지만 짧은 선분의 인접 엘보 간 최소 간격, 제작 규격, 3D 간섭 검사는 별도 필요.
- 데모 밸브는 지정된 근처에 시각적으로 배치하는 것으로, 배관을 실제 절단해 body와 조립하지 않습니다.
- Undo는 메뉴에 아직 통합하지 않았고, 데모 재생성은 `MARINE_PIPE_DEMO` 루트를 제거합니다. 해당 루트 아래를 수동 편집했다면 백업한 뒤 재생성하세요.
- 모델은 visualization prototype이며, 검증된 생산설계 CAD 데이터가 아닙니다.

## (옵션) Unity-AI-Splines / Unity Splines 연동 준비

1. Unity Package Manager에서 **Splines** (`com.unity.splines`) 설치.
2. Project Settings > Player > Scripting Define Symbols에 `MARINE_PIPE_USE_SPLINES` 추가.
3. 산업 배관 데모 생성 후 Hierarchy에서 예를 들어 `01-P-001` 선택.
4. `Tools > Marine Pipe > 03 - Selected Pipe to Editable Spline` 실행.
5. 자식 `EditableSplineCenterline`의 SplineContainer가 생성됨. 중심선은 일정 R 원호를 샘플링한 점들을 `Linear` knot으로 저장. 세그먼트를 촘촘하게 잡아 원호를 시각적으로 근사.
6. Unity-AI-Splines의 MCP `AddKnot`, `Evaluate` 등을 이 SplineContainer에 적용 가능. **주의: Knot 변경에 따라 원래 Pipe Mesh가 실시간 갱신되는 연동은 다음 단계이며 아직 구현되지 않음.**

## STEP 05 - 실제 연결 관계가 있는 펌프-헤더 계통

`Tools > Marine Pipe > 04 - Build Connected Pump Header Demo` 메뉴를 실행하세요.

- V1 데모는 삭제하지 않고 비활성화합니다.
- `MARINE_CONNECTED_SYSTEM_V2` 아래에 펌프, 흡입/토출 경로, 헤더, 3개 분기관과 설비를 생성합니다.
- 연결점 좌표와 3개 소비자까지의 그래프 도달성을 검사하고 Console 및 팝업에 PASS/FAIL을 표시합니다.
- 모델은 시각화 프로토타입이며, 실제 CAD 배관의 밀폐·압력·유량 및 Tee 부울 결합은 보장하지 않습니다.
- 개발 근거·판정 기준·수정 이력: `Docs/DEVLOG_STEP05_KO.md` 참고.
