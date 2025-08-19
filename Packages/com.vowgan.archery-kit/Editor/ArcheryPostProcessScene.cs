using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Vowgan.Contact;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    public class ArcheryPostProcessScene : MonoBehaviour
    {
        // Run one step before Contact's PPS does
        [PostProcessScene(-101)]
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
            if (!audioPlayer)
            {
                // If there's no audio player, but we need one...
                if (pools.Length != 0)
                {
                    GameObject contactObj = new GameObject("Contact Audio Player");
                    audioPlayer = contactObj.AddUdonSharpComponent<ContactAudioPlayer>();

                    if (EditorApplication.isPlaying)
                    {
                        UdonBehaviour udonProxy = UdonSharpEditorUtility.GetBackingUdonBehaviour(audioPlayer);
                        UdonManager.Instance.RegisterUdonBehaviour(udonProxy);
                    }
                }
            }

            ArrowTargetBase[] targetBases = FindObjectsByType<ArrowTargetBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (ArrowTargetBase target in targetBases) target.AudioPlayer = audioPlayer;
            
            ArrowProp[] arrowProps = FindObjectsByType<ArrowProp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (ArrowProp arrow in arrowProps) arrow.AudioPlayer = audioPlayer;

            TimeTrial[] timeTrials = FindObjectsByType<TimeTrial>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TimeTrial trial in timeTrials) trial.AudioPlayer = audioPlayer;
            
        }
        
    }
}