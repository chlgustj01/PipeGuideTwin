using UnityEngine;
using PipeGuideTwin.Data;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Interaction
{
    public sealed class PipeGuideSelectable : MonoBehaviour
    {
        public string ObjectId;
        public string DisplayName;
        public PIDComponentType ObjectType;
        public float DiameterMm;
        public string Material;
        public string StartNodeId;
        public string EndNodeId;
        public Vector3 PositionMm => EngineeringUnitConverter.UnityToPositionMm(transform.position);
    }
}
