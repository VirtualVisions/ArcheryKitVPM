
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TargetDistanceButton : ArrowTargetBase
    {

        public float Distance = 5;
        public TargetDistanceSetter DistanceSetter;
        public Material SignOff;
        public Material SignOn;

        private MeshRenderer displayRenderer;
        
        
        public override void _Init()
        {
            base._Init();
            displayRenderer = GetComponent<MeshRenderer>();
        }

        protected override void _OnHit(ArrowProp arrow)
        {
            base._OnHit(arrow);
            
            if (!localPlayer.IsOwner(arrow.gameObject)) return;
            DistanceSetter._SetDistance(this);
        }

        public void _DisplaySign(bool value)
        {
            displayRenderer.sharedMaterial = value ? SignOn : SignOff;
        }
        
    }
}