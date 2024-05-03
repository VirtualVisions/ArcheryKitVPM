
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.ArcheryKit
{
    
    public enum PickupHand : byte
    {
        None,
        Left,
        Right,
    }
    
    [DefaultExecutionOrder(0)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class BowBase : UdonSharpBehaviour
    {

        [Header("Networking")] 
        [UdonSynced] public bool Held;
        [UdonSynced] public bool Nocked;
        [UdonSynced] public float BowScale = 1;
        [UdonSynced] public PickupHand BowHand = PickupHand.Right;
        [UdonSynced] public PickupHand NockHand;
        
        [Header("Parameters")] 
        [SerializeField] private float LaunchStrength = 40;
        [SerializeField] private float LaunchOffset = 0.75f;
        [SerializeField] private float PullVolumeMultiplier = 1;
        [SerializeField] private float EffectVolume = 1;
        [SerializeField] private int NockClickPoints = 4;
        [SerializeField] private bool LocalUserIsVR;
        [SerializeField] private bool OwnerIsVR;
        [SerializeField] private Vector2 ScaleMinMax = new Vector2(0.1f, 2);

        [Header("References")]
        public Transform NockBone;
        public Transform PickupPivot;
        public Vector3 NockStartPosition;

        [SerializeField] private Transform ScaleRoot;
        [SerializeField] private VRCPickup BowPickup;
        [SerializeField] private ArrowPool Pool;
        [SerializeField] private TrajectoryPreview PreviewLine;
        [SerializeField] private Animator BowAnim;
        [SerializeField] private AudioSource NockPullSource;
        [SerializeField] private AudioSource EffectSource;
        [SerializeField] private AudioClip RatchetClip;

        private VRCPlayerApi localPlayer;
        private VRCPlayerApi owner;
        private float stringPull;
        private float lastStringPull;
        private float stringPullVolume;
        private int nockAudioPoint = -1;
        private bool scaleAdjusting;
        private float localBowScale;

        private int hashStringPull;
        private int hashNocked;


        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            LocalUserIsVR = localPlayer.IsUserInVR();
            
            hashStringPull = Animator.StringToHash("StringPull");
            hashNocked = Animator.StringToHash("Nocked");
            NockStartPosition = NockBone.localPosition;
            
            if (!localPlayer.IsUserInVR()) BowPickup.proximity = 1;
            
            Nock.gameObject.SetActive(false);
        }

        public override void OnDeserialization()
        {
            owner = Networking.GetOwner(gameObject);
            OwnerIsVR = owner.IsUserInVR();

            localBowScale = BowScale;
            
            ApplyBowScale();
            
            if (OwnerIsVR) DeserializeVR();
            else DeserializePC();
        }

        public override void PostLateUpdate()
        {
            if (!Held) return;
            
            owner = Networking.GetOwner(gameObject);
            if (OwnerIsVR) UpdateVR();
            else UpdatePC();
        }

        public void _OnPickup()
        {
            if (LocalUserIsVR) OnPickupVR();
            else OnPickupPC();
            _SetHeld(true);
        }

        public void _OnPickupUseDown()
        {
            if (LocalUserIsVR) OnPickupUseDownVR();
            else OnPickupUseDownPC();
        }

        public void _OnPickupUseUp()
        {
            if (LocalUserIsVR) OnPickupUseUpVR();
            else OnPickupUseUpPC();
        }

        public void _OnDrop()
        {
            if (LocalUserIsVR) OnDropVR();
            else OnDropPC();
            
            _SetHeld(false);
        }

        private void _SetHeld(bool value)
        {
            Held = value;
            RequestSerialization();
            OnDeserialization();
        }

        private void _SetNocked(bool value)
        {
            Nocked = value;
            RequestSerialization();
            OnDeserialization();
        }

        private void _SetNockHand(PickupHand hand)
        {
            NockHand = hand;
            RequestSerialization();
            OnDeserialization();
        }

        private void WriteCurrentBowScale()
        {
            BowScale = localBowScale;
            RequestSerialization();
        }

        /// <summary>
        /// A value is passed purely for validation. If a custom client calls this method, they will not be able to
        /// pass a valid VRCPlayerApi object, let alone the local one.
        /// </summary>
        public void _Claim(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.isLocal) return;
            
            Networking.SetOwner(player, gameObject);
            Networking.SetOwner(player, Nock.gameObject);
            Pool._ClaimOwnership(player);
        }

        public void _StartPulling()
        {
            if (!NockPullSource.isPlaying)
            {
                NockPullSource.Play();
                NockPullSource.time = UnityEngine.Random.Range(0, NockPullSource.clip.length);
            }
        }

        public void _StopPulling()
        {
            NockPullSource.Stop();
            BowAnim.SetFloat(hashStringPull, 0);
            if (owner.isLocal) PreviewLine._ClearPreview();

            stringPull = 0;
            lastStringPull = 0;
            nockAudioPoint = -1;
            NockBone.localPosition = NockStartPosition;
        }

        private void NockArrow(bool value)
        {
            BowAnim.SetBool(hashNocked, value);
        }

        private void FireArrow(Vector3 direction)
        {
            float launchStrength = stringPull * LaunchStrength;
            Pool._Launch(PreviewLine.transform.position + (direction.normalized * LaunchOffset), direction * launchStrength);
        }

        private void UpdateBow(float pullValue, bool displayPreview, Vector3 direction)
        {
            stringPull = pullValue;

            HandleStringPullVolume();

            BowAnim.SetFloat(hashStringPull, stringPull);

            if (owner.isLocal)
            {
                if (displayPreview)
                {
                    PreviewLine._PreviewTrajectory(pullValue, direction * stringPull * LaunchStrength, Pool.ArrowGravity);
                }
                else
                {
                    PreviewLine._ClearPreview();
                }
            }

            int currentNockAudioPoint = Mathf.RoundToInt(stringPull * NockClickPoints);
            if (nockAudioPoint != currentNockAudioPoint)
            {
                nockAudioPoint = currentNockAudioPoint;
                EffectSource.pitch = Mathf.Lerp(0.9f, 1.1f, stringPull);
                EffectSource.PlayOneShot(RatchetClip, EffectVolume);
            }
        }

        private void HandleStringPullVolume()
        {
            if (lastStringPull < stringPull)
            {
                stringPullVolume += Mathf.Abs(stringPull - lastStringPull);
            }
            else
            {
                stringPullVolume -= Time.deltaTime;
            }

            lastStringPull = stringPull;

            stringPullVolume = Mathf.Clamp01(stringPullVolume);
            NockPullSource.volume = stringPullVolume * PullVolumeMultiplier;
        }

        /// <summary>
        /// Adjust the bow scale by a delta.
        /// </summary>
        /// <param name="value"></param>
        private void AdjustBowScale(float value)
        {
            localBowScale =  Mathf.Clamp(localBowScale + value, ScaleMinMax.x, ScaleMinMax.y);
            ApplyBowScale();
        }
        /// <summary>
        /// Set the current scale of the bow.
        /// </summary>
        /// <param name="value">Total bow scale, clamped between</param>
        private void SetBowScale(float value)
        {
            localBowScale =  Mathf.Clamp(value, ScaleMinMax.x, ScaleMinMax.y);
            ApplyBowScale();
        }

        /// <summary>
        /// Apply localBowScale onto the bow visual.
        /// </summary>
        private void ApplyBowScale()
        {
            Vector3 scale = Vector3.one * localBowScale;
            scale.x *= BowHand == PickupHand.Left ? 1 : -1;
            ScaleRoot.localScale = scale;
        }
    }
}