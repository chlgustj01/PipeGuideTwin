using System;
using UnityEngine;

namespace PipeGuideTwin.Data.Mock
{
    [Serializable] public sealed class MockPathVisual
    {
        public string Id;
        public Vector3[] PointsMm;
    }
    [CreateAssetMenu(menuName="PipeGuide Twin/HiFi Visual Fixtures")]
    public sealed class HiFiVisualFixtures : ScriptableObject
    {
        public MockPathVisual[] Initial;
        public MockPathVisual[] Local;
        public MockPathVisual Original;
        public Vector3 CollisionPositionMm;
    }
}
