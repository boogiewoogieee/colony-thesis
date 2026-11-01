using Colony.Core;
using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// Composition root: creates the simulation when the scene loads and advances it every frame.
    /// Put exactly one in the gameplay scene. Systems are registered here as they are built.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField, Min(0.01f), Tooltip("Simulated seconds per tick.")]
        private float _tickSeconds = Simulation.DefaultTickSeconds;

        [SerializeField, Min(1f), Tooltip("Length of one in-game day, in seconds of play.")]
        private float _dayLengthSeconds = (float)Simulation.DefaultDayLengthSeconds;

        public Simulation Simulation { get; private set; }

        private void Awake()
        {
            Simulation = new Simulation(_tickSeconds, _dayLengthSeconds);
        }

        private void Update()
        {
            // Unscaled time: game speed and pause are handled by the simulation clock.
            Simulation.Update(Time.unscaledDeltaTime);
        }
    }
}
