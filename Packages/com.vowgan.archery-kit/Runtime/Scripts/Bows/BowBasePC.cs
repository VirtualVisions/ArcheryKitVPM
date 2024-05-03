
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

namespace Vowgan.ArcheryKit
{
    public partial class BowBase
    {
        
        [Header("PC Settings")]
        public AnimationCurve PullCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 1);
        public Vector3 PCGripAngle = new Vector3(-90, 0, -90);
        
        private float pullTime;
        
        
        private void DeserializePC()
        {
            if (Nocked)
            {
                PickupPivot.localRotation = Quaternion.Euler(PCGripAngle);
                pullTime = Time.timeSinceLevelLoad;
                _StartPulling();
            }
            else
            {
                _StopPulling();
                NockBone.localPosition = NockStartPosition;
            }
            
            NockArrow(Nocked);
        }
        
        private void UpdatePC()
        {
            if (Nocked)
            {
                float pullCurve = PullCurve.Evaluate(Time.timeSinceLevelLoad - pullTime);
                NockBone.localPosition = new Vector3(0, 0, -pullCurve);
                UpdateBow(pullCurve, true, transform.forward);
            }

            if (owner.isLocal)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    scaleAdjusting = true;
                    localBowScale = BowScale;
                }
                if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
                {
                    scaleAdjusting = false;
                    WriteCurrentBowScale();
                }
                
                if (scaleAdjusting)
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        AdjustBowScale(Time.deltaTime);
                    }
                    else if (Input.GetKey(KeyCode.DownArrow))
                    {
                        AdjustBowScale(-Time.deltaTime);
                    }
                }
            }
        }

        private void OnPickupPC() { }

        private void OnPickupUseDownPC()
        {
            _SetNocked(true);
        }

        private void OnPickupUseUpPC()
        {
            FireArrow(transform.forward);
            _SetNocked(false);
        }

        private void OnDropPC()
        {
            _StopPulling();
            _SetNocked(false);
        }
        
    }
}