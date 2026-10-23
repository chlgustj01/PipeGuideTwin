namespace PipeGuideTwin.Data
{
    public enum PIDComponentType { Unknown, Pipe, Valve, Pump, Tank, Equipment, Junction, Nozzle }
    public enum ValidationStatus { Unknown, Match, Warning, Mismatch, Missing, Duplicate }
    public enum WorkflowStage { Project, DataImport, Model3D, ConsistencyCheck, InitialRouting, LocalRerouting, ResultCompare, ApprovalHistory }
}
