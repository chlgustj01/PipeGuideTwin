# PipeGuide Twin — High-Fidelity Product Demo

A owns visualization, interaction, UI and integration. B owns validation/calculation;
C owns routing. No B/C algorithm is implemented here. Open Scenes/PipeGuide_Main.unity.

Engineering data stays in millimeters. Unity uses meters. Visualization/EngineeringUnitConverter
is the shared conversion boundary; coordinate axes remain unchanged until CAD mapping is agreed.
IDs (EquipmentId, NozzleId, PipeId, StartNodeId, EndNodeId), PositionMm, DiameterMm and
Material are future schema concepts, not finalized fields. ValidationIssue now carries reported
status, category, expected/actual text, severity and an optional expected position in millimeters.
Routing, comparison and revision contracts remain empty.

Original design data and models are read-only. Future flow: Original → Working Copy → Revision.
Data/Input holds source references; Data/Demo and Scripts/Data/Mock are reserved for mock fixtures;
Data/Revisions is reserved for future records. No persistence or revision engine exists yet.
Validation and Routing folders are B/C result adapter boundaries. UI does not call algorithms.
InitialRouting and LocalRerouting are distinct workflow stages. Navigation is metadata only.

STEP 1 adds three selectable mock equipment objects, four hand-placed orthogonal pipe
assemblies and three structure objects. These are interaction fixtures, not engineering results.
MOCK identifies this demo; no parser, routing or validation result is simulated.
Future comparison metrics include length, bends, estimated pressure loss/material cost,
penetrations and collisions. The designer approves candidates before working-copy changes.

Assets/PipeGuideTwin/StreamingAssets is an organizational placeholder, not Unity's special
Assets/StreamingAssets runtime folder. No runtime file loading is implemented.
Korean shell text uses installed Malgun Gothic, Apple SD Gothic Neo or Noto Sans CJK KR;
install one of these on a target without Korean fonts. OS font files are not redistributed.
The editor builders refuse to overwrite populated demo roots or proceed with dirty scenes.

## Viewer controls (Play Mode)

- Left click: select pipe/equipment; click background or structure to clear.
- Right drag: orbit about the current pivot.
- Middle drag: pan.
- Mouse wheel: zoom, limited to 3–45 meters from the pivot.
- Navigation and inspector panels block 3D selection and camera gestures.

SelectionManager publishes SelectionChanged; SelectionInspectorUI subscribes and updates
inspector/status/debug labels. Objects are identified by PipeGuideSelectable metadata, never names.
SelectionHighlight overrides shared material references in Model3D without creating material instances.
During ConsistencyCheck it adds a cyan bounding outline while preserving validation surfaces.
SetBaseMaterials supplies the underlying visual state; deselection retains the reported status.
Palette assets live in Materials/.
Coordinates and diameters cross the existing EngineeringUnitConverter boundary; model transforms
are meters and exposed engineering positions are millimeters. CurrentStage is serialized as Model3D.

Run PipeGuide Twin > Verify STEP 1 in Play Mode from a saved main scene to reproduce the
mouse-input integration checks. The explicit editor harness creates a temporary virtual mouse,
queues clicks/drags/wheel through Unity Input System, checks runtime selection/material/UI/camera
results, writes Temp/PipeGuideTwin-Step1-Tests.json and screenshots, then exits Play Mode.
It restores input settings and removes its temporary device. Tests do not run in player builds.

## STEP 2 reported validation results

Click 정합성 검사 to display the mock summary, four issue buttons and validation materials.
Click 3D 모델 to return to the normal inspector and base materials. The Product Shell now
provides all eight navigation pages; their additional contents are UI mock fixtures.
All user-facing labels remain Korean.

MockValidationDataProvider supplies five independent reported results. ValidationResultController
accepts replacement results through ReplaceResults(IEnumerable<ValidationIssue>); UI and visuals
consume those records rather than infer statuses from IDs, geometry, design attributes or names.
Summary aggregation only counts reported statuses; no engineering comparison is performed.
Issue-list and direct 3D selection share SelectionManager. Missing issues select no model object;
the visual controller creates a separate indicator at ExpectedPositionMm under
_WORLD/ValidationVisualizationRoot in Play Mode. PIPE_005 never exists as a pipe/GameObject.
Markers are rebuilt on result replacement and deactivated on Model3D return; results are retained.

ValidationWarning is yellow, ValidationMismatch amber, ValidationDuplicate orange and
ValidationMissing red. Match keeps its original material. This is mock result visualization only:
no missing/duplicate detection, matching, routing, import, comparison calculation or approval.

Run PipeGuide Twin > Verify STEP 2 in Play Mode for actual queued mouse clicks on navigation,
issue rows and 3D objects, marker and selection-state checks, replacement-result checks and
camera regression checks. Reports/screenshots are written to Temp/PipeGuideTwin-Step2-*.

## Product UI Shell

The saved starting page is Project. Top navigation switches exactly one of eight roots under
MainCanvas/Pages. Every page has LeftPanel, CenterWorkspace, RightPanel and BottomStatus.
The existing STEP 1 inspector/debug objects move between Model3D and Consistency right panels;
STEP 2 issue data, list and visuals remain functional. Old MainContent placeholders are preserved
inactive. Original world objects and SampleScene are unchanged.

Materials/PipeGuideTheme.asset holds the product palette. Data/Demo/ProductShellMockData.asset
holds display fixtures separately from UI behavior. ProductShellDemoContent is the fixture factory;
ProductShellSceneSetup builds the saved UI from those assets. Editing fixture data and rebuilding
the layout is an editor workflow, not a runtime import/data-binding system.

Project, Data, Routing, Local Rerouting, Compare and History contain illustrative records only.
Routing workspaces reuse the room camera without generating routes; selection is enabled only
in Model3D/Consistency. The file button only displays a message, and approve/reject only update
local UI labels. They never open files, change project revision, mutate original design data or save
history. Comparison percentages and candidate evaluation values are prepared fixtures, not calculations.
Candidate A's recommendation badge is a demo label, not a clearance/engineering suitability judgment.

Run PipeGuide Twin > Verify Product Shell in Play Mode for eight-page navigation, mock actions,
pipe/tank selection, empty deselection, UI blocking, camera and retained STEP 2 integration checks.
The harness sends virtual input on Dynamic input updates, waits for stable Game View dimensions,
captures all pages and exits Play Mode. Temp/PipeGuideTwin-Shell-* contains generated evidence.

## High-fidelity rebuild

The saved main scene has a separate pre-rebuild backup at Scenes/PipeGuide_Main_PreHiFiBackup.unity.
The current presentation uses raised metric cards, compact status chips, a sheet/file table,
progress indicators, system groups, a structured selection/issue inspector and a revision timeline.
Long shell paragraphs remain archived under inactive _LegacyContent nodes; all eight page roots,
original selection/camera components and prior validation data are preserved.

The room now contains eight equipment assemblies and twenty pipe lines: four interactive design
pipes plus sixteen non-selectable environment pipes. Eight valve handwheels, sixteen equipment
nozzles, flanges, curved elbow display meshes, deck seams, a pipe rack, frames and a grating walkway
provide industrial detail. PIPE_005 remains absent because its validation record represents Missing.
New equipment is selectable; environment pipes have no selection collider/metadata.

HiFiVisualFixtures.asset supplies explicitly authored millimeter point lists for initial/local
candidate lines and the original path/collision marker. MockRouteVisualController only draws those
lists and changes selection emphasis. It performs no routing, collision or suitability calculation.
StructuredInspectorUI consumes selectable metadata or reported ValidationIssue values, including
a supplied mock DifferenceValue; it never computes diameter/material differences.

Compare uses two independent preview cameras/RenderTextures: the original/red layer and candidate
B/cyan layer are isolated by camera culling masks. A separate wide-aspect project camera prevents
thumbnail distortion. Private numeric visual layers 25–28 are used only for these display routes;
no project layer names or physics collision settings are modified. The selected-object overlay label
is non-interactive, and 3D-position focus uses the existing camera pivot extension.

Run PipeGuide Twin > Verify HiFi Product in Play Mode for eight-page captures, actual queued mouse
selection of PIPE_001/PIPE_003/EQ_TANK_001, deselection, UI blocking, orbit/pan/zoom, issue-marker
re-entry, candidate selection and separate preview checks. Generated evidence is Temp/PipeGuideTwin-HiFi-*.
This is still MOCK data: no Excel/DB, actual matching, missing/duplicate detection, path search,
collision analysis, pressure loss, cost calculation or persisted revision is implemented.
