
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [RequireComponent(typeof(VRCPickup))]
    [RequireComponent(typeof(SphereCollider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BowPickupProxy : UdonSharpBehaviour
    {

        [SerializeField] private BowBase Bow;

        private VRCPlayerApi localPlayer;
        private SphereCollider capsule;


        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            capsule = GetComponent<SphereCollider>();
        }

        public override void OnPickup()
        {
            Bow._Claim(localPlayer);

            capsule.enabled = false;
            Bow._OnPickup();
        }

        public override void OnPickupUseDown()
        {
            Bow._OnPickupUseDown();
        }

        public override void OnPickupUseUp()
        {
            Bow._OnPickupUseUp();
        }

        public override void OnDrop()
        {
            capsule.enabled = true;
            Bow._OnDrop();
        }
    }
}