using System;
using UdonSharp;
using UnityEngine;
using Vowgan.Contact;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Vowgan.ArcheryKit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ArrowTargetBase : UdonSharpBehaviour
    {
        [HideInInspector] public ContactAudioPlayer AudioPlayer;

        protected VRCPlayerApi localPlayer;
        protected DataList arrows;
        protected bool initialized;


        private void Start() => _Init();

        /// <summary>
        /// Initialization function called during start to validate data for inherited classes.
        /// </summary>
        public virtual void _Init()
        {
            if (initialized) return;
            initialized = true;
            localPlayer = Networking.LocalPlayer;
            arrows = new DataList();
        }

        private void OnDisable()
        {
            _Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;

            ArrowProp arrow = other.GetComponent<ArrowProp>();
            if (arrow) _OnHit(arrow);
        }

        /// <summary>
        /// Event called when the target is hit by an ArrowProp.
        /// </summary>
        protected virtual void _OnHit(ArrowProp arrow)
        {
            arrows.Add(arrow);
        }

        /// <summary>
        /// Clear all arrows this target is currently colliding with.
        /// </summary>
        public virtual void _Clear()
        {
            DataList currentArrows = arrows.ShallowClone();

            for (int i = 0; i < currentArrows.Count; i++)
            {
                ArrowProp arrow = (ArrowProp)currentArrows[i].Reference;
                if (Networking.LocalPlayer.IsOwner(arrow.gameObject))
                {
                    arrow._Clear();
                }
            }

            arrows.Clear();
        }
    }
}