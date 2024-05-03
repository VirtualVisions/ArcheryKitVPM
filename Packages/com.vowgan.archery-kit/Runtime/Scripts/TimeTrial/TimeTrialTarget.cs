
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TimeTrialTarget : ArrowTarget
    {
        
        public TimeTrial Controller;
        public Vector3 CurrentPosition;
        
        
        protected override void _OnHit(ArrowProp arrow)
        {
            base._OnHit(arrow);
            if (localPlayer.IsOwner(Controller.gameObject))
            {
                // This is delayed by 2 frames to allow for the particle systems to play at the correct point.
                SendCustomEventDelayedFrames(nameof(_ReturnTarget), 2);
            }
        }

        public void _ReturnTarget()
        {
            Controller._HitTarget(this);
        }

        public void _Move(Vector3 pos)
        {
            if (CurrentPosition == pos) return;
            CurrentPosition = pos;
            transform.localPosition = pos;
            _Clear();
        }
        
    }
}