using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
            ArrowPool[] pools = FindObjectsByType<ArrowPool>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (ArrowPool pool in pools)
            {
                List<ArrowProp> arrows = new();
                for (int i = 0; i < pool.ArrowCount; i++)
                {
                    GameObject arrow = (GameObject)PrefabUtility.InstantiatePrefab(pool.ArrowPrefab, pool.transform);
                    arrow.name = $"Arrow {i}";

                    ArrowProp arrowProp = arrow.GetComponent<ArrowProp>();
                    arrowProp.Pool = pool;
                    arrows.Add(arrowProp);
                }
                
                pool.Arrows = arrows.ToArray();
            }
            
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