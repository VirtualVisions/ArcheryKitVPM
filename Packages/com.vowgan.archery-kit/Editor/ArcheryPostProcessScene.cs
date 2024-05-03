using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using Vowgan.Contact;

namespace Vowgan.ArcheryKit
{
    public class ArcheryPostProcessScene : MonoBehaviour
    {
        
        [PostProcessScene(-100)]
        public static void PostProcessScene()
        {
            ContactAudioPlayer audioPlayer = FindObjectOfType<ContactAudioPlayer>();
            if (audioPlayer)
            {
                ArrowTargetBase[] targetBases = FindObjectsByType<ArrowTargetBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (ArrowTargetBase target in targetBases) target.AudioPlayer = audioPlayer;
                
                ArrowProp[] arrowProps = FindObjectsByType<ArrowProp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (ArrowProp arrow in arrowProps) arrow.AudioPlayer = audioPlayer;

                TimeTrial timeTrial = FindObjectOfType<TimeTrial>();
                if (timeTrial) timeTrial.AudioPlayer = audioPlayer;

            }
        }
        
    }
}