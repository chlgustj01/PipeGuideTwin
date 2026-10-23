using System;
using UnityEngine;

namespace MarinePipe
{
    [Serializable]
    public class PipeRecord
    {
        public string id;
        public string system;
        public float outerDiameterM = 0.18f;
        public float wallThicknessM = 0.008f;
        public float bendRadiusM = 0.32f;
        public Vector3[] waypoints;
        public bool valve;
    }

    [Serializable]
    public class PipeDatabase
    {
        public PipeRecord[] pipes;
    }

    public sealed class MarinePipePath : MonoBehaviour
    {
        public Vector3[] waypoints;
        public float outerDiameterM;
        public float wallThicknessM;
        public float bendRadiusM;
    }

    public sealed class MarinePipeTag : MonoBehaviour
    {
        public string pipeId;
        public string system;
        public float outerDiameterM;
    }
}
