using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = Unity.Mathematics.Random;

namespace FlockJobSystem
{
    public struct AvoidWallJob : IJobParallelForTransform
    {
        public FlockSetting setting;

        public NativeArray<float3> accelerates;
        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;

        public NativeArray<float3> velocitiesForIndex;
        public NativeArray<float3> positionsForIndex;
        public NativeArray<float3> acceleratesForIndex;


        public void Execute(int index, TransformAccess transform)
        {
            var velocity = velocities[index];
            // if ((Vector3) velocity == Vector3.zero)
            // {
            //     velocity = Random.CreateFromIndex((uint) index).NextFloat3(0, 2);
            // }

            var position = (float3) transform.position;
            var accelerate = accelerates[index];

            if (setting.avoidWalls)
            {
                var v = new float3(setting.width, position.y, position.z);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;

                v = new float3(setting.width, position.y, position.z);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;

                v = new float3(position.x, -setting.height, position.z);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;

                v = new float3(position.x, setting.height, position.z);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;

                v = new float3(position.x, position.y, -setting.depth);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;

                v = new float3(position.x, position.y, setting.depth);
                v = Avoid(position, v);
                v *= 5;
                accelerate += v;
            }

            accelerates[index] = accelerate;
            positions[index] = position;
            velocitiesForIndex[index] = velocity;
            velocities[index] = velocity;
            positionsForIndex[index] = position;
            acceleratesForIndex[index] = accelerate;
        }

        float3 Avoid(float3 pos, float3 target)
        {
            var steer = pos - target;

            return math.normalize(steer);
        }
    }

    public struct FlockJob : IJobParallelFor
    {
        public FlockSetting setting;

        public NativeArray<float3> accelerates;

        // public NativeArray<float3> velocities;
        public NativeArray<float3> positions;


        [NativeDisableParallelForRestriction] public NativeArray<float3> acceleratesForIndex;
        [NativeDisableParallelForRestriction] public NativeArray<float3> velocitiesForIndex;
        [NativeDisableParallelForRestriction] public NativeArray<float3> positionsForIndex;

        public void Execute(int index)
        {
            var accelerate = accelerates[index];
            var position = positions[index];

            if ((Vector3) setting.goal != Vector3.zero)
            {
                accelerate += Reach(position, setting.goal, 0.01f);
            }

            // accelerate += Alignment(position);
            // accelerate += Cohesion(position);
            // accelerate += Separation(position);
            accelerate += AlignmentCohesionAndSeparation(position);
            accelerates[index] = accelerate;
        }

        float3 Reach(float3 pos, float3 target, float amount)
        {
            return (target - pos) * amount;
        }

        public float3 AlignmentCohesionAndSeparation(float3 pos)
        {
            var count = 0;
            var velSum = float3.zero;
            var steer = float3.zero;
            var posSum = float3.zero;
            var posSum1 = float3.zero;

            var repulse = float3.zero;

            for (int i = 0, il = positionsForIndex.Length; i < il; i++)
            {
                if (Random.CreateFromIndex((uint) i).NextFloat(0, 1f) > 0.6f)
                    continue;
                var boidPos = positionsForIndex[i];
                var boidVel = velocitiesForIndex[i];
                var distance = math.distance(boidPos, pos);
                if (distance > 0 && distance <= setting.neighborRadius)
                {
                    velSum += boidVel;
                    posSum += boidPos;

                    repulse = math.normalize(pos - boidPos);
                    repulse /= distance;
                    posSum1 += repulse;
                    count++;
                }
            }

            if (count > 0)
            {
                velSum /= count;
                posSum /= count;
                var l = math.length(velSum);
                if (l > setting.maxSteerForce)
                {
                    velSum /= l / setting.maxSteerForce;
                }
            }

            steer = posSum - pos;
            var l1 = math.length(steer);
            if (l1 > setting.maxSteerForce)
            {
                steer /= l1 / setting.maxSteerForce;
            }


            return steer + velSum + posSum1;
        }
    }

    public struct MoveJob : IJobParallelForTransform
    {
        public FlockSetting setting;

        public NativeArray<float3> accelerates;
        public NativeArray<float3> velocities;
        public NativeArray<float3> positions;

        public void Execute(int index, TransformAccess transform)
        {
            var velocity = velocities[index];
            var position = positions[index];
            var accelerate = accelerates[index];

            velocity += accelerate;
            var l = math.length(velocity);
            if (l > setting.maxSpeed)
            {
                velocity /= l / setting.maxSpeed;
            }

            var d = Vector3.Distance(position, setting.goal);
            if (setting.stopWhenReachedGoal && d <= 25)
            {
                velocity = Vector3.zero;
                accelerate = Vector3.zero;
            }
            else
            {
                position += velocity;
                accelerate = float3.zero;
            }


            velocities[index] = velocity;
            positions[index] = position;
            accelerates[index] = accelerate;
            transform.position = position;
            var v = (Vector3) velocity;
            if (v != Vector3.zero)
                transform.localRotation = Quaternion.Lerp(transform.localRotation,
                    Quaternion.LookRotation(v, Vector3.up), 0.5f);
        }
    }
}