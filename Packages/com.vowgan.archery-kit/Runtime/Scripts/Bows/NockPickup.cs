
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [RequireComponent(typeof(VRCPickup))]
    [DefaultExecutionOrder(1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NockPickup : UdonSharpBehaviour
    {

        public BowBase Bow;
        private VRCPickup pickup;

        private void OnEnable()
        {
            pickup = GetComponent<VRCPickup>();
        }
        
        public override void OnPickup()
        {
            Bow._OnPickupNock(pickup.currentHand);
        }

        public override void OnPickupUseDown()
        {
            if (Bow.Nocked)
            {
                _ForceDrop();
            }
            else
            {
                Bow._ToggleNockArrow();
            }
        }

        public void _ForceDrop()
        {
            pickup.Drop();
        }

        public override void OnDrop()
        {
            if (!Bow.Held) return;
            Bow._OnDropNock();
            transform.localPosition = Bow.NockStartPosition;
        }
    }
}