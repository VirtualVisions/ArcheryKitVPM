
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TrialTargetPool : UdonSharpBehaviour
    {

        [UdonSynced] public Vector3[] Positions = new Vector3[1];
        public TimeTrialTarget[] Targets;
        public Vector2Int XRange = new Vector2Int(-3, 3);
        public Vector2Int YRange = new Vector2Int(0, 3);
        public Vector2Int ZRange = new Vector2Int(0, 20);

        private VRCPlayerApi localPlayer;
        private Vector3 resetPosition = new Vector3(0, -10, 0);


#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnDrawGizmosSelected()
        {
            Vector3 position = new Vector3(
                (XRange.x + XRange.y) / 2f,
                (YRange.x + YRange.y) / 2f,
                (ZRange.x + ZRange.y) / 2f
            );

            Vector3 size = new Vector3(
                XRange.y - XRange.x,
                YRange.y - YRange.x,
                ZRange.y - ZRange.x);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.TransformPoint(position), size);
            Gizmos.color = new Color(0, 0, 1, 0.25f);
            Gizmos.DrawCube(transform.TransformPoint(position), size);
        }
#endif

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;

            if (localPlayer.IsOwner(gameObject))
            {
                Positions = new Vector3[Targets.Length];
                for (int i = 0; i < Positions.Length; i++)
                {
                    Positions[i] = resetPosition;
                }
                _ReturnAll();
            }
        }

        public override void OnDeserialization()
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                Targets[i]._Move(Positions[i]);
            }
        }

        public void _PlaceAll()
        {
            foreach (TimeTrialTarget target in Targets)
            {
                _PlaceNext(target, true);
            }
            RequestSerialization();
            OnDeserialization();
        }

        public void _PlaceNext(TimeTrialTarget target, bool skipSerialize = false)
        {
            int index = Array.IndexOf(Targets, target);
            if (index == -1) return;

            Vector3 position = resetPosition;

            for (int i = 0; i < 50; i++)
            {
                position = new Vector3(
                    UnityEngine.Random.Range(XRange.x, XRange.y),
                    UnityEngine.Random.Range(YRange.x, YRange.y),
                    UnityEngine.Random.Range(ZRange.x, ZRange.y));
                if (Array.IndexOf(Positions, position) == -1) break;
            }
            
            Positions[index] = position;

            if (skipSerialize) return;
            
            RequestSerialization();
            OnDeserialization();
        }

        public void _ReturnAll()
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                Positions[i] = resetPosition;
            }

            RequestSerialization();
            OnDeserialization();
        }

        public void _ClaimOwnership(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.isLocal) return;

            Networking.SetOwner(player, gameObject);
        }
    }
}