# STEP 1.5 — Product UI Shell

2026-10-23 (Asia/Seoul). Unity 6000.3.26f1 / Universal Render Pipeline.

## Actual state verified through Unity MCP

- Main scene: Assets/PipeGuideTwin/Scenes/PipeGuide_Main.unity.
- Saved starting stage: Project. Only ProjectPage is active in the saved scene.
- isDirty=false, isCompiling=false, isPlaying=false, missing scripts=0.
- Console errors=0; warnings=5, all external Unity MCP lifecycle/connection messages.
- Original _SYSTEM/_WORLD/_CAMERA/_LIGHTING/_UI roots and the demo models retained.
- SampleScene SHA256 unchanged: 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA.

## Product structure

MainCanvas/Pages contains ProjectPage, DataPage, Model3DPage, ConsistencyPage,
InitialRoutingPage, LocalReroutingPage, ResultComparePage and ApprovalHistoryPage.
Each page has LeftPanel, CenterWorkspace, RightPanel and BottomStatus.
Eight top navigation buttons select exactly one page and display an orange underline.
CanvasScaler uses ScaleWithScreenSize at a 1920×1080 reference resolution.

PipeGuideTheme.asset centralizes the product palette. ProductShellMockData.asset stores display
fixtures separately from UI logic. The saved UI is generated from those fixtures in the editor;
this is not a runtime import/binding implementation. Cards use thin borders and measured Korean
font metrics; the project dashboard uses a full-width overview and a two-card lower row.

Existing STEP 1 inspector/debug objects and STEP 2 issue panel were reparented rather than
replaced. The same inspector moves between the two interactive viewer pages. STEP 2 validation
data, selection, colors and missing indicator remain functional. No new validation logic was added.
Original MainContent placeholders remain inactive. No world models or selection/camera scripts
were deleted or rewritten.

## Play Mode verification

PlayModeResults.json records 68 PASS assertions at 1920×1080 from virtual mouse input.
The test waits for stable dimensions and queues events on Dynamic input updates.

- All eight workflow buttons: correct stage, one active page, orange state, four regions,
  card body text fitting its allocated height.
- File button: message only, no file picker/import.
- Approve/reject: local mock label only, project REV.A and revision-folder contents unchanged.
- PIPE_001 and EQ_TANK_001 selection/highlight/inspector, empty-space deselection.
- Left panel and inspector UI click blocking.
- Orbit, pan and zoom after returning to Model3D.
- Existing validation issue selection, status color plus outline, missing issue and ordinary
  inspector restoration after returning to Model3D.
- Korean font glyph availability and no fabricated PIPE_005.

All eight final page screenshots were visually reviewed. Card bounds also fit their column bounds
in an editor-side MCP check. Console messages were archived; external packages were not modified.

## Problems → causes → fixes → results

Some card text was truncated → initial size estimates did not reflect Korean font line metrics →
measure Text.preferredHeight, reserve action space, and reflow cards; use two lower dashboard
columns → complete last lines visible in all final screenshots and eight fit assertions PASS.

Shared inspector lived outside page containers → exclusive-page switching could disable or
duplicate its behavior → move the same instance between viewer right panels and limit its
updates to interactive stages → selection and retained STEP 2 checks PASS.

## Scope

All added routing, rerouting, comparison and approval records are display fixtures. No route line,
candidate generation, collision analysis, engineering calculation, import, database, revision
persistence or external API was implemented. Candidate A's recommendation badge is an explicit
demo label and does not represent clearance suitability. The completed prior STEP 2 features
were retained because they already existed before this UI-shell request.

Ready for the requested next Excel Data Import & Binding step. That work was not started.
