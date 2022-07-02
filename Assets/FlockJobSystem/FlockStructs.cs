using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FlockJobSystem
{
    [Serializable]
    public struct FlockSetting
    {
        [Range(1,1000)]
        public int boidsCount;

        [Header("Goal")]
        public float3 goal;
        public bool stopWhenReachedGoal;
        public bool chaseGoal;

        [Header("Wall")]
        public float width;
        public float height;
        public float depth;
        public bool avoidWalls;

        [Header("Other")]
        public float neighborRadius;
        public float maxSpeed;
        public float maxSteerForce;

    }
}