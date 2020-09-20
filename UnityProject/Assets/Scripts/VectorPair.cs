using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AMG
{
    public struct VectorPair
    {
        public Vector2 V1;
        public Vector2 V2;
        public Vector2 Normale;

        public float Magnitude()
        {
            return (this.V1 - this.V2).magnitude;
        }

        public void ComputeNormale()
        {
            this.Normale = (this.V2 - this.V1).normalized;
            float temp = this.Normale.x;
            this.Normale.x = this.Normale.y;
            this.Normale.y = temp;
        }
    }
}