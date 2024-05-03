using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Vowgan.ArcheryKit
{
    public partial class BowBase
    {
        
        [Header("VR Settings")]
        [SerializeField] private float MinRequiredPull = 0.2f;
        [SerializeField] private float MaxStringPull = 1;
        [SerializeField] private Vector3 VRGripAngle = new Vector3(-105, 0, -90);
        
        [Header("VR References")] 
        [SerializeField] private NockPickup Nock;
        [SerializeField] private Transform LookAtPivot;
        [SerializeField] private Transform AimingPivot;

        private float lastClickTime;
        private float scaleAdjustStartHeight;
        
        
        private void DeserializeVR()
        {
            if (Held)
            {
                _StartPulling();
                Vector3 rotation = VRGripAngle;
                rotation.y *= BowHand == PickupHand.Left ? 1 : -1;
                PickupPivot.localRotation = Quaternion.Euler(rotation);
            }
            else
            {
                _StopPulling();
                LookAtPivot.localRotation = Quaternion.Euler(0, 180, 0);
                NockBone.localPosition = NockStartPosition;
                NockBone.localRotation = Quaternion.identity;
            }

            NockArrow(Nocked);
        }
        
        private void UpdateVR()
        {
            Vector3 gripPosition = Vector3.zero;
            
            if (owner.isLocal && scaleAdjusting)
            {
                SetBowScale(((transform.position.y - scaleAdjustStartHeight) * 2) + BowScale);
            }
            
            switch (NockHand)
            {
                default:
                case PickupHand.None:
                    break;
                case PickupHand.Left:
                    gripPosition = owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    break;
                case PickupHand.Right:
                    gripPosition = owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    break;
            }

            switch (NockHand)
            {
                default:
                case PickupHand.None:
                    LookAtPivot.localRotation = Quaternion.Euler(0, 180, 0);
                    UpdateBow(0, false, transform.forward);
                    NockBone.localPosition = NockStartPosition;
                    NockBone.localRotation = Quaternion.identity;
                    break;
                case PickupHand.Left:
                case PickupHand.Right:
                    LookAtPivot.LookAt(gripPosition, transform.up);
                    Vector3 lookAtRotation = LookAtPivot.localRotation.eulerAngles;
                    lookAtRotation.x = 0;
                    LookAtPivot.localRotation = Quaternion.Euler(lookAtRotation);

                    NockBone.position = gripPosition;

                    Vector3 nockBoneLocalPosition = NockBone.localPosition;
                    float nockHeightOffset = Mathf.Abs(nockBoneLocalPosition.y / 2f);
                    nockBoneLocalPosition = Vector3.ClampMagnitude(nockBoneLocalPosition, MaxStringPull - nockHeightOffset);
                    NockBone.localPosition = nockBoneLocalPosition;
                    NockBone.LookAt(AimingPivot.position);

                    Quaternion targetRotation = Quaternion.LookRotation(AimingPivot.position - NockBone.position);
                    UpdateBow(Mathf.Abs(nockBoneLocalPosition.z), Nocked, targetRotation * Vector3.forward);
                    break;
            }
        }

        private void OnPickupVR()
        {
            BowHand = (PickupHand)BowPickup.currentHand;
            RequestSerialization();
            OnDeserialization();
            
            Nock.gameObject.SetActive(true);
        }

        private void OnPickupUseDownVR()
        {
            if (scaleAdjusting) return;
            
            if (Time.timeSinceLevelLoad - lastClickTime <= 0.5f)
            {
                scaleAdjusting = true;
                scaleAdjustStartHeight = transform.position.y;
            }
            else
            {
                lastClickTime = Time.timeSinceLevelLoad;
            }
        }

        private void OnPickupUseUpVR()
        {
            if (!scaleAdjusting) return;
            scaleAdjusting = false;
            WriteCurrentBowScale();
        }

        private void OnDropVR()
        {
            Nock.gameObject.SetActive(false);
            Nock._ForceDrop();
        }
        
        public void _OnPickupNock(VRC_Pickup.PickupHand hand)
        {
            _SetNockHand((PickupHand)hand);
        }

        public void _ToggleNockArrow()
        {
            _SetNocked(!Nocked);
        }

        public void _OnDropNock()
        {
            NockHand = PickupHand.None;

            if (Nocked && stringPull >= MinRequiredPull)
            {
                Quaternion targetRotation = Quaternion.LookRotation(AimingPivot.position - NockBone.position);
                FireArrow(targetRotation * Vector3.forward);
            }

            _SetNocked(false);
        }
    }
}