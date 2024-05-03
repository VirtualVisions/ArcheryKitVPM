
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TrajectoryPreview : UdonSharpBehaviour
    {

        [SerializeField] private LineRenderer Line;
        [SerializeField] private Vector3 Velocity = new Vector3(0, 15, 15);
        [SerializeField] private float Gravity = 5;
        [SerializeField] private float Separation = 0.05f;
        [SerializeField] private int IterationCount = 20;
        [SerializeField] private Color LineStartColor = new Color(1, 1, 1, 0.5f);

        [SerializeField] private Vector3[] Points;


        private void Start()
        {
            Points = new Vector3[IterationCount];
            Line.positionCount = IterationCount;
        }
        
        /// <summary>
        /// Enable preview and display the arch.
        /// </summary>
        /// <param name="pullValue">Intensity of line color opacity.</param>
        /// <param name="velocity">Direction and force of arch calculation.</param>
        /// <param name="gravity">Force over time for gravity multiplier.</param>
        public void _PreviewTrajectory(float pullValue, Vector3 velocity, float gravity)
        {
            Line.enabled = true;
            
            Color color = Vector4.Lerp(Color.clear, LineStartColor, pullValue);
            Line.startColor = color;
            color.a = 0;
            Line.endColor = color;
            
            Velocity = velocity;
            Gravity = gravity;
            
            for (int i = 0; i < IterationCount; i++)
            {
                float separation = Separation * i;
                Vector3 adjustedVelocity = Velocity * separation;
                Vector3 accumulatedGravity = (Vector3.down * Gravity) * 0.5f * (separation * separation);
                Points[i] = transform.position + adjustedVelocity + accumulatedGravity;
            }

            Line.SetPositions(Points);
        }

        /// <summary>
        /// Hide the current preview.
        /// </summary>
        public void _ClearPreview()
        {
            Line.enabled = false;
        }
        
    }
}