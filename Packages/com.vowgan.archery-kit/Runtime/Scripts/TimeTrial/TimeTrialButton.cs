
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{

    public enum TrialTrigger
    {
        StartGame,
        EndGame,
    }
    
    public class TimeTrialButton : ArrowTargetBase
    {
        
        public TrialTrigger Trigger;
        public TimeTrial Trial;
        
        
        protected override void _OnHit(ArrowProp arrow)
        {
            base._OnHit(arrow);
            if (!localPlayer.IsOwner(arrow.gameObject)) return;
            switch (Trigger)
            {
                case TrialTrigger.StartGame:
                    Trial._StartGame();
                    break;
                case TrialTrigger.EndGame:
                    Trial._EndGame();
                    break;
            }
        }
    }
}