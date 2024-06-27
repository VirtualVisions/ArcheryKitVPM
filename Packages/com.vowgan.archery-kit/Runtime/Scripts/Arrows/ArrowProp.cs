using System;
using UdonSharp;
using UnityEngine;
using Vowgan.Contact;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Vowgan.ArcheryKit
{
    [RequireComponent(typeof(CapsuleCollider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ArrowProp : UdonSharpBehaviour
    {

        [Header("Networking")]
        [UdonSynced] public bool Active;
        [UdonSynced] public Vector3 StartPosition;
        [UdonSynced] public Vector3 StartVelocity;

        [Header("Parameters")]
        public float Gravity = 5;
        [SerializeField] private bool Launched;
        [SerializeField] private float SoundRange = 30;
        [SerializeField] private float FlightTimeout = 30;
        [SerializeField] private bool TimeAccurate;
        [SerializeField] private LayerMask CollisionMask;
        
        [Header("References")] 
        [HideInInspector] public ContactAudioPlayer AudioPlayer;
        [HideInInspector] public ArrowPool Pool;
        
        [SerializeField] private GameObject Visual;
        [SerializeField] private TrailRenderer Trail;
        [SerializeField] private AudioClip ClipShoot;
        [SerializeField] private AudioClip ClipHit;

        protected CapsuleCollider capsule;
        protected float startTime;


        private void Start()
        {
            capsule = GetComponent<CapsuleCollider>();
            DisableInteractive = !Networking.IsOwner(gameObject);
            ReceiveNetworking();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            DisableInteractive = !player.isLocal;
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            if (TimeAccurate)
            {
                startTime = result.sendTime;
            }
            else
            {
                startTime = Time.realtimeSinceStartup;
            }
            ReceiveNetworking();
        }

        private void ReceiveNetworking()
        {
            Visual.SetActive(Active);
            capsule.enabled = Active;

            if (Active)
            {
                Launched = true;
                // ClearTarget();
                transform.position = StartPosition;
                transform.forward = StartVelocity.normalized;

                if (startTime == Time.realtimeSinceStartup)
                {
                    CalculateArc(0);
                }
                else
                {
                    SimulateFromSendTime();
                }

                AudioPlayer._PlaySoundChilded(ClipShoot, transform, transform.position, SoundRange, 1,
                    UnityEngine.Random.Range(0.97f, 1.03f));

                Trail.Clear();
            }
        }

        private void SimulateFromSendTime()
        {
            int iterations = Mathf.FloorToInt(Time.realtimeSinceStartup - startTime);
            for (int i = 0; i < iterations; i++)
            {
                CalculateArc(Time.fixedDeltaTime * i, true);
            }
        }

        private void FixedUpdate()
        {
            if (Launched) CalculateArc(Time.realtimeSinceStartup - startTime);
        }

        public override void Interact()
        {
            _Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) return;
            _Stop();
        }

        private Vector3 CalculateArc(float timeSinceStart, bool lineCast = false)
        {
            if (timeSinceStart >= FlightTimeout)
            {
                Launched = false;
                return transform.position;
            }

            Vector3 adjustedVelocity = StartVelocity * timeSinceStart;
            Vector3 gravity = Vector3.down * (Gravity * (0.5f * (timeSinceStart * timeSinceStart)));

            adjustedVelocity += gravity;
            Vector3 newPosition = StartPosition + adjustedVelocity;
            Vector3 direction = Vector3.Normalize(StartVelocity + Vector3.down * (Gravity * timeSinceStart));

            if (lineCast)
            {
                if (Physics.Linecast(transform.position, newPosition, CollisionMask, QueryTriggerInteraction.Ignore))
                {
                    _Stop();
                }
            }

            transform.position = newPosition;
            transform.forward = direction;

            return newPosition;
        }
        
        public void _Launch(Vector3 position, Vector3 velocity)
        {
            Active = true;
            StartPosition = position;
            StartVelocity = velocity;

            RequestSerialization();

            startTime = Time.realtimeSinceStartup;
            ReceiveNetworking();
        }
        
        public void _Stop()
        {
            Launched = false;
            AudioPlayer._PlaySound(ClipHit, transform.position, SoundRange);
        }
        
        public void _Clear()
        {
            Active = false;
            Pool._Return(this);
            RequestSerialization();
            ReceiveNetworking();
        }
    }
}