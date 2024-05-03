using System;
using UdonSharp;
using UnityEngine;

namespace Vowgan.ArcheryKit
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ArrowTarget : ArrowTargetBase
    {

        public ParticleSystem ParticleSystem;
        public AudioClip Clip;
        public float SoundRange = 50;
        
        
        protected override void _OnHit(ArrowProp arrow)
        {
            base._OnHit(arrow);
            ParticleSystem.Play();
            AudioPlayer._PlaySound(Clip, transform.position, SoundRange);
        }
    }
}