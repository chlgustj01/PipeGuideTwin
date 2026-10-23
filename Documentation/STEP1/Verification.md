# STEP 1 verification — 2026-10-23 (Asia/Seoul)

Unity 6000.3.26f1 / URP / PipeGuideTwin.
Main scene: Assets/PipeGuideTwin/Scenes/PipeGuide_Main.unity.

Verified through Unity MCP after the Play Mode input checks completed:

- Saved scene, isDirty=false, isCompiling=false, Play Mode exited.
- Original five roots retained; three equipment, four pipe assemblies, three structures.
- All seven selectable objects have colliders and SelectionHighlight components.
- SelectionManager, camera controller and SelectionInspectorUI are attached.
- Current stage is Model3D; no missing scripts.
- Engineering coordinates remain mm, world transforms use meters.
- SampleScene SHA256 remains 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA.

PlayModeResults.json records 23 passing assertions from queued Input System mouse events
in the running Game Camera at 1920×1080. These cover PIPE_001, PIPE_002 and pump selection;
highlight restoration; pipe properties and inspector updates; empty-space deselection;
navigation/inspector click blocking; orbit, pan, zoom; UI camera blocking; final deselection.
Viewer.png and Selected.png were captured in Play Mode and visually inspected.

Console errors: 0. Warnings: 5, all external Unity MCP package lifecycle messages:
two BufferedFileLogStorage “Flush called but already disposed” warnings and three
McpManagerClientHub “Connection not available and auto-reconnect disabled” warnings.
No STEP 1 script warning was observed. External packages were not modified.
Earlier external authentication/connection logs available before the test were archived in
ConsoleBaseline.txt before clearing the Console; warnings produced during verification remain.

No validation/routing algorithm, validation highlight, import, candidate comparison,
approval or revision feature was added. STEP 2 was not started.
