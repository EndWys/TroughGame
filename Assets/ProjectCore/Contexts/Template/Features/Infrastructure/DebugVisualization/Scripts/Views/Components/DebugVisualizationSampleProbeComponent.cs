using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugVisualizationSampleProbeComponent : BaseDebugInspectableComponent
    {
        private float _startTime;
        private int _frameCounter;

        [DebugValue("ConsoleTest/System", Label = "Uptime (s)")]
        private float Uptime => Time.realtimeSinceStartup - _startTime;

        [DebugValue("ConsoleTest/System", Label = "FPS")]
        private int Fps => Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));

        [DebugValue("ConsoleTest/Gameplay/Player", Label = "Health")]
        private int Health => 100 - (int)((Time.realtimeSinceStartup * 7f) % 55f);

        [DebugValue("ConsoleTest/Gameplay/Player", Label = "Speed")]
        private float Speed => 3f + Mathf.PingPong(Time.realtimeSinceStartup * 0.8f, 4f);

        [DebugValue("ConsoleTest/Gameplay/Session", Label = "State")]
        private string SessionState => (Mathf.FloorToInt(Time.realtimeSinceStartup / 4f) % 2) == 0 ? "Running" : "Paused";

        [DebugValue("ConsoleTest/Network", Label = "Ping (ms)")]
        private int Ping => 40 + Mathf.RoundToInt(Mathf.PingPong(Time.realtimeSinceStartup * 20f, 80f));

        [DebugValue("ConsoleTest/Network", Label = "Packets")]
        private int Packets => _frameCounter;

        private void Awake()
        {
            _startTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            _frameCounter++;
        }
    }
}
