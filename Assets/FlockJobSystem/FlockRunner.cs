using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace FlockJobSystem
{
    public class FlockRunner : MonoBehaviour
    {
        public GameObject prefab;
        public Transform target;

        public FlockSetting setting = new FlockSetting()
        {
            avoidWalls = false,
            neighborRadius = 100,
            maxSpeed = 4,
            maxSteerForce = .1f,
            width = 500,
            height = 500,
            depth = 200,
            boidsCount = 200
        };


        private List<Transform> boids;
        private NativeArray<float3> accelerates, tempAccelerates;
        private NativeArray<float3> positions, tempPositions;
        private NativeArray<float3> velocities, tempVelocites;
        private TransformAccessArray transformArray;
        private JobHandle moveHandle, avoidHandle, flockHandle;
        private bool isStarted, isInit;

        private void Update()
        {
            if (!moveHandle.IsCompleted && isStarted)
            {
                return;
            }

            setting.goal = setting.chaseGoal ? target.position : float3.zero;

            AssureCount();

            if (moveHandle.IsCompleted)
            {
                moveHandle.Complete();
                avoidHandle.Complete();
                flockHandle.Complete();

                isStarted = false;
            }

            isStarted = true;

            if (boids.Count != positions.Length)
            {
                isInit = false;
                Dispose();
            }

            if (!isInit)
            {
                transformArray = new TransformAccessArray(boids.ToArray());
                accelerates = new NativeArray<float3>(boids.Count, Allocator.Persistent);
                velocities = new NativeArray<float3>(boids.Count, Allocator.Persistent);
                positions = new NativeArray<float3>(boids.Count, Allocator.Persistent);
                tempAccelerates = new NativeArray<float3>(boids.Count, Allocator.Persistent);
                tempVelocites = new NativeArray<float3>(boids.Count, Allocator.Persistent);
                tempPositions = new NativeArray<float3>(boids.Count, Allocator.Persistent);

                isInit = true;
            }


            var avoidJob = new AvoidWallJob()
            {
                accelerates = accelerates,
                setting = setting,
                positions = positions,
                velocities = velocities,
                acceleratesForIndex = tempAccelerates,
                positionsForIndex = tempPositions,
                velocitiesForIndex = tempVelocites,
            };

            avoidHandle = avoidJob.Schedule(transformArray);

            var flockJob = new FlockJob()
            {
                accelerates = accelerates,
                acceleratesForIndex = tempAccelerates,
                positions = positions,
                positionsForIndex = tempPositions,
                setting = setting,
                velocitiesForIndex = tempVelocites,
            };

            flockHandle = flockJob.Schedule(boids.Count, 16, avoidHandle);

            var moveJob = new MoveJob()
            {
                accelerates = accelerates,
                positions = positions,
                setting = setting,
                velocities = velocities
            };

            moveHandle = moveJob.Schedule(transformArray, flockHandle);
        }


        void AssureCount()
        {
            if (!prefab)
            {
                Debug.LogError("empty prefab!");
                return;
            }

            if (boids == null)
            {
                boids = new List<Transform>();
            }

            if (boids.Count < setting.boidsCount)
            {
                boids.Add(Instantiate(prefab).transform);
            }

            if (boids.Count > setting.boidsCount)
            {
                var go = boids[^1];
                boids.RemoveAt(boids.Count - 1);
                Destroy(go.gameObject);
            }
        }

        void Dispose()
        {
            moveHandle.Complete();
            avoidHandle.Complete();
            flockHandle.Complete();
            if (accelerates.IsCreated)
                accelerates.Dispose();
            if (velocities.IsCreated)
                velocities.Dispose();
            if (positions.IsCreated)
                positions.Dispose();
            if (tempAccelerates.IsCreated)
                tempAccelerates.Dispose();
            if (tempVelocites.IsCreated)
                tempVelocites.Dispose();
            if (tempPositions.IsCreated)
                tempPositions.Dispose();
            if (transformArray.isCreated)
                transformArray.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}