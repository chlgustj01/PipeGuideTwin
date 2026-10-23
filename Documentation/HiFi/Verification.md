# High-Fidelity Product UI Rebuild

2026-10-23 (Asia/Seoul) / Unity 6000.3.26f1 / URP.

## Actual final state (Unity MCP)

- Main: Assets/PipeGuideTwin/Scenes/PipeGuide_Main.unity.
- Pre-rebuild backup: Assets/PipeGuideTwin/Scenes/PipeGuide_Main_PreHiFiBackup.unity.
- isDirty=false, isCompiling=false, isPlaying=false, missing scripts=0.
- Console errors=0. Warnings=5 are external MCP connection/log-cleanup messages.
- Eight workflow pages and the original SelectionManager, EngineeringCameraController,
  PipeGuideSelectable, SelectionHighlight and SelectionInspectorUI are preserved.
- SampleScene hash unchanged: 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA.

## Hero room counts

| Category | Count | Definition |
|---|---:|---|
| Equipment | 8 | Direct equipment assemblies under EquipmentRoot |
| Pipe lines | 20 | Four interactive design pipes + sixteen environment lines |
| Valves | 8 | Handwheel assemblies named VALVE_* |
| Nozzles | 16 | Two per equipment assembly |
| Flanges | 56 | Pipe and equipment flange primitives |
| Structure | 89 | Mesh renderers under StructureRoot, including deck/grating/frame details |

Equipment uses combined motor/casing/base/coupling/fins, vessel/caps/legs/nozzles,
heat-exchanger ribs/control boxes and filter/stand shapes. Display tube meshes contain rounded
elbows along authored points. Environment pipes have neither selectable metadata nor colliders.
PIPE_005 remains absent to preserve the existing Missing result; its red indicator is separate.
No actual collision or pathfinding logic was added. Original design IDs remain attached to
the original four selectable pipe roots and three existing equipment roots.

## Visual system

- Dark navy centralized theme, orange active navigation, raised cards and subtle borders.
- Short page headers, 13–15 body text, 20–24 sections, 24–44 metric typography.
- Project: live wide-aspect room thumbnail, four metrics and workflow progress.
- Data: geometric upload icon, file card, sheet table, quality tiles and 92% health bar.
- Model: colored system groups, industrial room, structured inspector and selected-object label.
- Consistency: compact issue cards/severity chips, three issue indicators plus Missing indicator,
  expected/actual cards and an explicit mock difference value.
- Routing: authored A/B/C mock point lists, selectable candidate cards and colored line emphasis.
- Local rerouting: original red line/collision cross plus A/B/C paths simultaneously.
- Comparison: independent original/B cameras and RenderTextures with isolated route layers,
  large metric changes and improvement cards. Camera framing focuses on the problem region.
- History: revision/status chips, timeline-style records and local approval/rejection state.

All eight page captures plus Selected.png were reviewed at 1920×1080. No core data or card needs
scrolling. The status bar identifies the data source as MOCK. The preview cameras are real scene
renders, not imported pictures. Main room lighting, materials and camera composition are adjusted.

## Final Play Mode check

PlayModeResults.json contains 59 PASS assertions from virtual mouse events injected on Dynamic
input updates. This includes eight-page navigation/exclusivity, PIPE_001/PIPE_003/EQ_TANK_001
selection and structured inspector, empty deselection, panel/inspector UI blocking, orbit/pan/zoom,
issue-marker page re-entry, missing selection, mock difference display, candidate-card border
selection, original + three local paths and distinct before/after targets/culling masks.
Authored route vertices are also checked for X/Y/Z orthogonality.

## Problems → causes → fixes → results

- Partial assembly failed → Unity missing-component wrappers passed a C# type pattern →
  Unity-aware null checks, cleanup of only partial generated materials and restore from saved Main →
  complete scene generation and compile success.
- Issue indicators disappeared on page re-entry → stage deactivation removed markers but Start
  was not repeated → rebuild on OnEnable → re-entry assertion PASS.
- Wide project preview looked flattened → RawImage and shared RenderTexture aspect mismatch →
  dedicated 1200×330 preview camera/texture → aspect-correct thumbnail in final capture.
- Before/after differences were small → full-room framing → closer, identical comparison framing
  and different route-layer masks → red original/collision vs cyan candidate clearly visible.
- Initial A fixture had a diagonal terminal span → authored points skipped an orthogonal corner →
  add the missing corner → all route fixture points pass axis-alignment check.

## Scope

Excel/CSV import, DB/SQLite, actual validation/missing/duplicate detection, A*/grid/pathfinding,
actual routing/collision analysis, pressure-loss/cost calculations, OCR/AI and persisted approval/
revision are NOT implemented. All candidate metrics, suitability badges, collision status,
comparison improvements and revisions are supplied visual fixtures. Difference -50 mm is an
explicit mock field; it is not calculated by the inspector. Engineering inputs stay in mm and
visualization crosses the existing meter conversion boundary.

No next-stage work was started.
