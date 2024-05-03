
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TargetDistanceSetter : UdonSharpBehaviour
    {

        [UdonSynced] public int DistanceButtonIndex;

        [SerializeField] private Transform TargetParent;
        [SerializeField] private TargetDistanceButton[] Buttons;

        private VRCPlayerApi localPlayer;
        private ArrowTargetBase[] targets;


        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            targets = TargetParent.GetComponentsInChildren<ArrowTargetBase>();
        }

        public override void OnDeserialization()
        {
            foreach (ArrowTargetBase target in targets) target._Clear();
            
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i]._DisplaySign(i == DistanceButtonIndex);
            }
            
            TargetParent.localPosition = new Vector3(0, 0, Buttons[DistanceButtonIndex].Distance);
        }

        public void _SetDistance(TargetDistanceButton distanceButton)
        {
            Networking.SetOwner(localPlayer, gameObject);
            DistanceButtonIndex = Array.IndexOf(Buttons, distanceButton);
            RequestSerialization();
            OnDeserialization();
        }

    }
}