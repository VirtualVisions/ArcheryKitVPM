
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ArrowPool : UdonSharpBehaviour
    {

        public GameObject ArrowPrefab;
        public int ArrowCount = 10;
        public ArrowProp[] Arrows;
        public float ArrowGravity = 5;

        private DataList freeArrows;
        private DataList activeArrows;
        
        
        private void Start()
        {
            freeArrows = new DataList();
            activeArrows = new DataList();
            
            foreach (ArrowProp arrow in Arrows)
            {
                freeArrows.Add(arrow);
            }
        }
        
        /// <summary>
        /// Launch a new arrow at a position and velocity.
        /// </summary>
        public void _Launch(Vector3 position, Vector3 velocity)
        {
            ArrowProp arrow = _GetArrow();
            arrow.Gravity = ArrowGravity;
            arrow._Launch(position, velocity);
        }

        /// <summary>
        /// Get an arrow from the pool.
        /// </summary>
        public ArrowProp _GetArrow()
        {
            if (freeArrows.Count > 0)
            {
                ArrowProp arrow = (ArrowProp)freeArrows[0].Reference;
                freeArrows.Remove(arrow);
                activeArrows.Add(arrow);
                return arrow;
            }
            else
            {
                ArrowProp arrow = (ArrowProp)activeArrows[0].Reference;
                activeArrows.Remove(arrow);
                activeArrows.Add(arrow);
                return arrow;
            }
        }
        
        /// <summary>
        /// Return an arrow into the pool of free arrows.
        /// If inheriting from ArrowProp, you must cast it as ArrowProp before passing it into this function to avoid some U# nulling issues.
        /// </summary>
        public void _Return(ArrowProp arrow)
        {
            activeArrows.Remove(arrow);
            freeArrows.Add(arrow);
        }

        /// <summary>
        /// Claim ownership of the pool.
        /// </summary>
        /// <param name="player">Must be a valid Local Player.</param>
        public void _ClaimOwnership(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.isLocal) return;

            Networking.SetOwner(player, gameObject);

            foreach (ArrowProp arrow in Arrows)
            {
                Networking.SetOwner(player, arrow.gameObject);
            }

        }

    }
}