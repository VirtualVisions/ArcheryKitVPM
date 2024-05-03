
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using Vowgan.Contact;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Vowgan.ArcheryKit
{

    public enum TimeTrialState
    {
        Idle,
        Countdown,
        MidGame,
        PostGame
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TimeTrial : UdonSharpBehaviour
    {

        [Header("Networking")]
        [UdonSynced] public TimeTrialState State = TimeTrialState.Idle;
        [UdonSynced] public int Score;

        [Header("Parameters")]
        [SerializeField] private float CountdownTimer = 3;
        [SerializeField] private float EndTimer = 4;
        [SerializeField] private float RoundTime = 30;
        [SerializeField] private float CurrentTime;

        [Header("References")]
        [HideInInspector] public ContactAudioPlayer AudioPlayer;
        [SerializeField] private GameObject NonGameEnvironment;
        [SerializeField] private TrialTargetPool TargetPool;
        [SerializeField] private TextMeshProUGUI LabelTimeRemaining;
        [SerializeField] private TextMeshProUGUI LabelScore;
        [SerializeField] private TextMeshProUGUI LabelOwner;
        [SerializeField] private AudioClip ClipCountdown;
        [SerializeField] private Transform CountdownPoint;

        private VRCPlayerApi localPlayer;
        private TimeTrialState oldState = TimeTrialState.Idle;
        private float stateLoadTime;
        private bool triggerCountdown;


        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            ReceiveNetworking(Time.realtimeSinceStartup);
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            ReceiveNetworking(result.sendTime);
        }

        private void ReceiveNetworking(float sendTime)
        {
            LabelScore.text = $"Score: {Score}";
            LabelOwner.text = $"Only {Networking.GetOwner(gameObject).displayName} can end the game.";
            
            if (oldState != State)
            {
                oldState = State;
                triggerCountdown = false;
                stateLoadTime = sendTime;
                
                switch (State)
                {
                    case TimeTrialState.Idle:
                        NonGameEnvironment.SetActive(true);
                        break;
                    case TimeTrialState.Countdown:
                        NonGameEnvironment.SetActive(false);
                        break;
                    case TimeTrialState.MidGame:
                        NonGameEnvironment.SetActive(false);
                        break;
                    case TimeTrialState.PostGame:
                        NonGameEnvironment.SetActive(true);
                        LabelTimeRemaining.text = "Time: 0";
                        break;
                }
            }
        }
        
        private void Update()
        {
            switch (State)
            {
                case TimeTrialState.Idle:
                    break;
                case TimeTrialState.Countdown:
                    LabelScore.text = $"Score: {Score}";
                    LabelTimeRemaining.text = $"Time: {RoundTime:F2}";

                    CurrentTime = TimeUntil(CountdownTimer);

                    if (!triggerCountdown && CurrentTime <= CountdownTimer)
                    {
                        triggerCountdown = true;
                        AudioPlayer._PlaySound(ClipCountdown, CountdownPoint.position);
                    }
                    
                    if (CurrentTime <= 0 && localPlayer.IsOwner(gameObject))
                    {
                        _SetState(TimeTrialState.MidGame);
                        TargetPool._PlaceAll();
                    }
                    break;
                case TimeTrialState.MidGame:
                    LabelScore.text = $"Score: {Score}";

                    CurrentTime = TimeUntil(RoundTime);
                    LabelTimeRemaining.text = $"Time: {CurrentTime:F2}";

                    if (!triggerCountdown && CurrentTime <= EndTimer)
                    {
                        triggerCountdown = true;
                        AudioPlayer._PlaySound(ClipCountdown, CountdownPoint.position);
                    }

                    if (CurrentTime <= 0 && localPlayer.IsOwner(gameObject))
                    {
                        _SetState(TimeTrialState.PostGame);
                    }
                    
                    break;
                case TimeTrialState.PostGame:
                    
                    break;
            }
        }

        private float TimeUntil(float time)
        {
            float timeSpan = Time.realtimeSinceStartup - stateLoadTime;
            return time - timeSpan;
        }
        
        public void _StartGame()
        {
            if (State == TimeTrialState.Countdown || State == TimeTrialState.MidGame) return;
            
            TargetPool._ClaimOwnership(localPlayer);
            Score = 0;
            _SetState(TimeTrialState.Countdown);
        }
        
        public void _EndGame()
        {
            if (State == TimeTrialState.Idle || State == TimeTrialState.PostGame) return;
            if (!localPlayer.IsOwner(gameObject)) return;
            
            _SetState(TimeTrialState.PostGame);
        }
        
        public void _SetState(TimeTrialState state)
        {
            if (State == state) return;
            
            Networking.SetOwner(localPlayer, gameObject);

            switch (state)
            {
                case TimeTrialState.Idle:
                    break;
                case TimeTrialState.Countdown:
                    break;
                case TimeTrialState.MidGame:
                    break;
                case TimeTrialState.PostGame:
                    TargetPool._ClaimOwnership(localPlayer);
                    TargetPool._ReturnAll();
                    break;
            }

            State = state;
            RequestSerialization();
            ReceiveNetworking(Time.realtimeSinceStartup);
        }

        public void _HitTarget(TimeTrialTarget target)
        {
            TargetPool._PlaceNext(target);
            
            Score++;
            RequestSerialization();
            // We do not need to run a deserialize locally for an updated score.
        }
        
    }
}