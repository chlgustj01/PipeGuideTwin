using PipeGuideTwin.Data;
using PipeGuideTwin.Data.Mock;

namespace PipeGuideTwin.Editor
{
    // Display fixtures only. No engineering judgment or calculation is performed.
    public static class ProductShellDemoContent
    {
        private static ShellCardData C(string title,string body,string badge="") => new ShellCardData {Title=title,Body=body,Badge=badge};
        private static ShellPageData P(WorkflowStage stage,string name,string label,string title,string subtitle,ShellCardData[] left,ShellCardData[] center,ShellCardData[] right,bool world=false,bool split=false)
            => new ShellPageData {Stage=stage,PageName=name,NavigationLabel=label,Title=title,Subtitle=subtitle,Left=left,Center=center,Right=right,ShowsWorld=world,SplitCenter=split};
        public static ShellPageData[] Create() => new[]
        {
            P(WorkflowStage.Project,"ProjectPage","프로젝트","프로젝트 개요","선박 배관 설계 검토를 위한 작업 공간",
                new[]{C("프로젝트 목록","● 파이프가이드 트윈 데모\n\n데모 선박\n기관실\n개정 A","현재 프로젝트"),C("작업 범위","설계 데이터 확인\n3D 모델 검토\n정합성 결과 확인\n경로 후보 검토\n설계자 결정")},
                new[]{C("파이프가이드 트윈","선박 배관 설계 정합성 검사\n및 국소 재설계 디지털 트윈\n\n데모 데이터로 전체 작업 흐름을 검토합니다.","프로젝트 대시보드"),C("프로젝트 정보","프로젝트   PipeGuide Twin Demo\n선박          DEMO VESSEL\n구역          ENGINE ROOM\n리비전       REV.A"),C("검토 순서","01  설계 데이터  →  02  3D 모델\n03  정합성 검사  →  04  경로 추천\n05  국소 재설계  →  06  결과 비교\n07  설계자 승인 / 이력")},
                new[]{C("현재 상태","상태                준비 완료\n데이터 소스      데모 (MOCK)\n설계 단위         mm\n화면 단위         m","검토 준비"),C("설계자 중심","시스템은 문제와 대안을 표시합니다.\n\n최종 설계 결정은 설계자가 합니다.\n원본 모델은 보존합니다.")}),
            P(WorkflowStage.DataImport,"DataPage","설계 데이터","설계 데이터 불러오기","Excel · CSV 기반 구조화 데이터 입력 화면",
                new[]{C("입력 형식","Excel (.xlsx)\nCSV (.csv)\n\n장비 / 노즐 / 배관 / 연결 정보","구조화 데이터"),C("데이터 흐름","원본 설계 데이터\n↓\n작업 사본\n↓\n검토 및 변경 이력")},
                new[]{C("설계 데이터 업로드","설계 데이터 파일을 불러오세요.\n\nExcel 또는 CSV 파일을 지원할 예정입니다.\n현재 파일 선택 버튼은 UI 데모입니다.","입력 준비"),C("데이터 요약","장비                         3\n노즐                         3\n설계 배관                  5\n3D 배관                    4\n연결                         5","데모 집계")},
                new[]{C("데이터 품질","ID 오류                     0\n참조 키 오류              0\n중복 객체                  1\n3D 누락                    1","데모 품질 값"),C("연결 예정","Excel의 Data_Quality_Check\n결과를 표시할 영역입니다.\n\n파일 로더 및 DB는 미연결입니다.")}),
            P(WorkflowStage.Model3D,"Model3DPage","3D 모델","3D 모델","데모 기관실 · 배관 및 장비 선택",
                new[]{C("시스템 트리","기관실\n\n냉각수 계통\n ├ PIPE_001\n ├ PIPE_002\n └ EQ_PUMP_001\n\n가열 계통\n ├ PIPE_003\n └ EQ_HEATER_001\n\n회수 계통\n └ PIPE_004","데모 계통"),C("뷰어 조작","왼쪽 클릭       선택\n오른쪽 드래그  회전\n가운데 드래그  이동\n마우스 휠       확대 / 축소")},new ShellCardData[0],new ShellCardData[0],true),
            P(WorkflowStage.ConsistencyCheck,"ConsistencyPage","정합성 검사","정합성 검사","전달된 정합성 결과와 3D 위치를 함께 검토",
                new ShellCardData[0],new ShellCardData[0],new ShellCardData[0],true),
            P(WorkflowStage.InitialRouting,"InitialRoutingPage","경로 추천","초기 경로 추천","시작 및 끝 노즐 사이의 경로 후보 검토",
                new[]{C("시작 노즐","NZ_PUMP_OUT_01\n\n냉각수 펌프 출구","시작"),C("끝 노즐","NZ_TANK_IN_01\n\n냉각수 탱크 입구","도착"),C("추천 조건","직교 경로  X / Y / Z\n\n경로 생성은 연결 예정입니다.")},
                new[]{C("3D 경로 작업 공간","시작 / 끝 노즐과 경로 후보를\n표시할 영역입니다.\n\n아직 경로 선은 생성하지 않습니다.","데모 화면")},
                new[]{C("후보 A","길이                 7.8 m\n굽힘                 5개","데모 후보"),C("후보 B","길이                 8.2 m\n굽힘                 3개","데모 후보"),C("후보 C","길이                 8.6 m\n굽힘                 4개","데모 후보")},true),
            P(WorkflowStage.LocalRerouting,"LocalReroutingPage","국소 재설계","국소 재설계","문제 구간과 대체 경로 후보를 비교하는 작업 공간",
                new[]{C("재설계 대상","배관     PIPE_003\n문제     구조물 충돌\n시작     NODE_A\n끝        NODE_B","데모 문제"),C("설계 제약조건","최소 여유 간격  ≥ 150 mm\n직교 경로          유지\n시작 / 끝 노드    유지","예시 조건")},
                new[]{C("경로 비교 범례","현재 경로    ● 빨강\n후보 A        ● 초록\n후보 B        ● 시안\n후보 C        ● 파랑\n\n범례만 표시하며 경로 선은 미구현입니다.","데모 비교")},
                new[]{C("후보 A","길이             3.2 m\n굽힘             3개\n충돌             0개\n여유 간격      100 mm\n비용 추정치   42","추천 · 데모"),C("후보 B","길이             3.5 m\n굽힘             2개\n충돌             0개\n여유 간격      140 mm\n비용 추정치   44","데모 평가"),C("후보 C","길이             3.9 m\n굽힘             3개\n충돌             0개\n여유 간격      220 mm\n비용 추정치   48","데모 평가")},true),
            P(WorkflowStage.ResultCompare,"ResultComparePage","결과 비교","결과 비교","원본 경로와 후보 B의 예시 비교 값",
                new[]{C("변경 요약","대상 배관  PIPE_003\n기존 경로  원본 경로\n선택 후보  후보 B","데모 비교"),C("검토 항목","길이 / 굽힘 / 충돌\n최소 여유 간격\n압력손실 추정치\n재료비 추정치")},
                new[]{C("변경 전 · 원본 경로","길이                  4.1 m\n\n굽힘                  5개\n\n충돌                  1개\n\n최소 여유 간격   20 mm\n\n압력손실 추정    0.55\n\n비용 추정          50","변경 전"),C("변경 후 · 후보 B","길이                  3.5 m\n\n굽힘                  2개\n\n충돌                  0개\n\n최소 여유 간격   140 mm\n\n압력손실 추정    0.40\n\n비용 추정          44","변경 후")},
                new[]{C("개선 효과","길이                 -14.6%\n\n굽힘                 -60%\n\n충돌                 1 → 0\n\n여유 간격         +120 mm\n\n압력손실 추정   -27%\n\n비용 추정         -12%","데모 수치"),C("비교 기준","표시 값은 준비된 Mock 값입니다.\n\n계산 및 설계 적합 판정은\n수행하지 않습니다.")},false,true),
            P(WorkflowStage.ApprovalHistory,"ApprovalHistoryPage","승인 / 이력","승인 및 변경 이력","설계자 결정과 변경 기록을 표시할 화면",
                new[]{C("리비전 목록","REV.A   원본\n\nREV.B   PIPE_003\n             후보 B\n             승인됨 (데모)","데모 이력")},
                new[]{C("리비전 상세","리비전     REV.B\n\n대상         PIPE_003\n\n변경 전     ORIGINAL_PIPE_003\n\n변경 후     CAND_B\n\n결정         승인됨 (데모)\n\n사유         충돌 제거 및 굽힘 감소","검토 기록")},
                new[]{C("설계자 결정","선택 후보  후보 B\n현재 상태  검토 대기\n\n버튼은 화면 상태만 변경합니다.","데모 승인"),C("원본 보호","원본 → 작업 사본 → 리비전\n\n실제 저장 및 이력 생성은\n연결 예정입니다.")})
        };
    }
}
