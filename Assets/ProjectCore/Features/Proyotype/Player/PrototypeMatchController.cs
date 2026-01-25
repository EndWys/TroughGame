using Fusion;
using ProjectCore.Domain.Scripts.NetworkUtilities;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player
{
    public class PrototypeMatchController : BaseNetworkCallbacksBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField] private BotNetworkSpawnController _botNetworkSpawnController;

        [Header("SETTINGS")] [SerializeField] private int _enemiesCount = 100;
        
        [Header("NETWORKED")]
        [Networked, OnChangedRender(nameof(OnMatchStateChanged))]
        [UnitySerializeField] private bool IsMatchRunning { get; set; }
        
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            base.Spawned();
            
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void Render()
        {
            if (!IsMatchRunning && Input.GetKeyDown(KeyCode.R))
            {
                IsMatchRunning = true;

                _botNetworkSpawnController.SpawnBot(_enemiesCount);
            }
        }

        public override void FixedUpdateNetwork()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(IsMatchRunning))
                {
                    Debug.Log($"Match state : {IsMatchRunning}");
                }
            }
        }

        private void OnMatchStateChanged()
        {
            Debug.Log($"Match state : {IsMatchRunning}.{_enemiesCount} enemies spawned.");
        }
        
        private void OnGUI()
        {
            if (Object == null || !Object.IsValid) return;
            
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            
            if (IsMatchRunning) return;

            GUI.Label(new Rect(10, 10, 500, 30),
                HasStateAuthority ? "Press 'R' to start the match" : "Waiting for host to start the match", 
                style);
        }
    }
}