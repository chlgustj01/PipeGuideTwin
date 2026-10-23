using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarinePipe
{
    public static class MarinePipeGeometry
    {
        // One Unity unit = one meter. Input waypoints are the theoretical corner intersections.
        // Trim each corner and replace it with a constant-radius circular arc.
        public static List<Vector3> Fillet(IReadOnlyList<Vector3> waypoints, float radius, int segmentsPer90 = 10)
        {
            if (waypoints == null || waypoints.Count < 2) throw new ArgumentException("At least 2 waypoints required");
            var result = new List<Vector3>();
            AddUnique(result, waypoints[0]);
            for (int i = 1; i < waypoints.Count - 1; i++)
            {
                Vector3 a = waypoints[i - 1], b = waypoints[i], c = waypoints[i + 1];
                Vector3 incoming = b - a, outgoing = c - b;
                if (incoming.magnitude < 0.0001f || outgoing.magnitude < 0.0001f) continue;
                Vector3 u = incoming.normalized, v = outgoing.normalized;
                float theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(u, v), -1f, 1f));
                Vector3 normal = Vector3.Cross(u, v);
                if (theta < 0.002f || theta > Mathf.PI - 0.002f || normal.sqrMagnitude < 0.000001f)
                {
                    AddUnique(result, b); continue;
                }
                float trim = Mathf.Min(radius * Mathf.Tan(theta * 0.5f), incoming.magnitude * 0.43f, outgoing.magnitude * 0.43f);
                if (trim < 0.001f) { AddUnique(result, b); continue; }
                float effectiveR = trim / Mathf.Tan(theta * 0.5f);
                normal.Normalize();
                Vector3 entry = b - u * trim;
                Vector3 exit = b + v * trim;
                Vector3 center = entry + Vector3.Cross(normal, u) * effectiveR;
                Vector3 radial = entry - center;
                AddUnique(result, entry);
                int steps = Mathf.Max(3, Mathf.CeilToInt(theta / (Mathf.PI * 0.5f) * segmentsPer90));
                for (int j = 1; j <= steps; j++)
                {
                    Vector3 p = center + Quaternion.AngleAxis(theta * Mathf.Rad2Deg * j / steps, normal) * radial;
                    AddUnique(result, j == steps ? exit : p);
                }
            }
            AddUnique(result, waypoints[waypoints.Count - 1]);
            return result;
        }

        static void AddUnique(List<Vector3> points, Vector3 value)
        {
            if (points.Count == 0 || (points[points.Count - 1] - value).sqrMagnitude > 1e-8f) points.Add(value);
        }

        public static Mesh SweepHollow(IReadOnlyList<Vector3> centerline, float outerRadius, float wallThickness, int radialSegments = 14)
        {
            if (centerline == null || centerline.Count < 2) throw new ArgumentException("At least 2 centerline points required");
            if (outerRadius <= 0 || wallThickness <= 0 || wallThickness >= outerRadius) throw new ArgumentException("Invalid OD/wall thickness");
            radialSegments = Mathf.Max(8, radialSegments);
            int n = centerline.Count, ringStride = radialSegments * 2;
            var verts = new Vector3[n * ringStride];
            var normals = new Vector3[verts.Length];
            var uv = new Vector2[verts.Length];
            var tris = new List<int>((n - 1) * radialSegments * 12 + radialSegments * 12);
            Vector3 prevT = Vector3.zero, up = Vector3.up;
            float distance = 0f;
            for (int i = 0; i < n; i++)
            {
                Vector3 t;
                if (i == 0) t = (centerline[1] - centerline[0]).normalized;
                else if (i == n - 1) t = (centerline[i] - centerline[i - 1]).normalized;
                else t = ((centerline[i] - centerline[i - 1]).normalized + (centerline[i + 1] - centerline[i]).normalized).normalized;
                if (t.sqrMagnitude < 0.1f) t = i == 0 ? Vector3.forward : prevT;
                if (i == 0) up = Mathf.Abs(Vector3.Dot(t, Vector3.up)) > .95f ? Vector3.forward : Vector3.up;
                else up = Quaternion.FromToRotation(prevT, t) * up;
                up = Vector3.ProjectOnPlane(up, t).normalized;
                Vector3 right = Vector3.Cross(up, t).normalized;
                up = Vector3.Cross(t, right).normalized;
                if (i > 0) distance += Vector3.Distance(centerline[i], centerline[i - 1]);
                for (int j = 0; j < radialSegments; j++)
                {
                    float angle = Mathf.PI * 2f * j / radialSegments;
                    Vector3 outward = Mathf.Cos(angle) * right + Mathf.Sin(angle) * up;
                    int o = i * ringStride + j, inner = i * ringStride + radialSegments + j;
                    verts[o] = centerline[i] + outward * outerRadius;
                    verts[inner] = centerline[i] + outward * (outerRadius - wallThickness);
                    normals[o] = outward; normals[inner] = -outward;
                    uv[o] = new Vector2((float)j / radialSegments, distance);
                    uv[inner] = new Vector2((float)j / radialSegments, distance);
                }
                prevT = t;
            }
            for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < radialSegments; j++)
            {
                int nj = (j + 1) % radialSegments;
                int a = i * ringStride + j, b = (i + 1) * ringStride + j;
                int c = i * ringStride + nj, d = (i + 1) * ringStride + nj;
                Quad(tris, a, c, b, d); // outer shell, outward facing
                int ai = a + radialSegments, bi = b + radialSegments;
                int ci = c + radialSegments, di = d + radialSegments;
                Quad(tris, ai, bi, ci, di); // inner shell, inward facing
            }
            // Ring-shaped caps at both ends.
            for (int j = 0; j < radialSegments; j++)
            {
                int nj = (j + 1) % radialSegments;
                int s = j, sn = nj, si = radialSegments + j, sin = radialSegments + nj;
                Quad(tris, s, si, sn, sin);
                int end = (n - 1) * ringStride;
                Quad(tris, end + j, end + nj, end + radialSegments + j, end + radialSegments + nj);
            }
            var mesh = new Mesh { name = "Hollow swept marine pipe" };
            if (verts.Length > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static void Quad(List<int> indices, int a, int b, int c, int d)
        {
            indices.Add(a); indices.Add(b); indices.Add(c);
            indices.Add(b); indices.Add(d); indices.Add(c);
        }
    }
}
