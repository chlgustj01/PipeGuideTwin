using UnityEngine;

namespace PipeGuideTwin.Visualization
{
    // The sole visualization conversion boundary. Axis mapping remains identity in STEP 0.
    public static class EngineeringUnitConverter
    {
        public static float MmToMeters(float mm) => mm * 0.001f;
        public static float MetersToMm(float meters) => meters * 1000f;
        public static Vector3 PositionMmToUnity(Vector3 mm) => mm * 0.001f;
        public static Vector3 UnityToPositionMm(Vector3 meters) => meters * 1000f;
    }
}
