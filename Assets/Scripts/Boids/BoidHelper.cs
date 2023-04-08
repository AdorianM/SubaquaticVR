using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoidHelper {

    const int directionCount = 300;
    public static readonly Vector3[] directions;

    static BoidHelper () {
        directions = new Vector3[directionCount];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < directionCount; i++) {

            // Get current fragment
            float t =  i / (float) directionCount;
            float inclination = Mathf.Acos (1 - 2 * t);

            // Distance on sphere relative to first point.
            float angularDistance = angleIncrement * i;

            float x = Mathf.Sin (inclination) * Mathf.Cos (angularDistance);
            float y = Mathf.Sin (inclination) * Mathf.Sin (angularDistance);
            float z = Mathf.Cos (inclination);
            directions[i] = new Vector3 (x, y, z);
        }
    }
}