using UnityEngine;
using VRC.SDKBase;

namespace Vowgan.ArcheryKit
{
    public class ArrowCheckpoint : ArrowTarget
    {

        [SerializeField] private Transform Point;
        
        
        protected override void _OnHit(ArrowProp arrow)
        {
            base._OnHit(arrow);

            if (!localPlayer.IsOwner(arrow.gameObject)) return;
            localPlayer.TeleportTo(Point.position, localPlayer.GetRotation());
            arrow._Clear();
        }
    }
}