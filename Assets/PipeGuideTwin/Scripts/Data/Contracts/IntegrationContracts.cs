using System;
using UnityEngine;

namespace PipeGuideTwin.Data.Contracts
{
    // Provisional connection points only. Fields and engineering schema require B/C agreement.
    [Serializable] public class ValidationIssue
    {
        public string IssueId;
        public string ObjectId;
        public ValidationStatus Status;
        public string Category;
        public string Title;
        public string Description;
        public string ExpectedValue;
        public string ActualValue;
        public string Severity;
        public string DifferenceValue;
        public bool HasExpectedPosition;
        public Vector3 ExpectedPositionMm;
    }
    [Serializable] public class RouteCandidate { }
    [Serializable] public class ComparisonResult { }
    [Serializable] public class RevisionRecord { }
}
