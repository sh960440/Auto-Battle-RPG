using Infrastructure;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Creates the game state machine and registers it with <see cref="ServiceLocator"/>.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameStateMachineHost : MonoBehaviour
    {
        [SerializeField] private GameState _initialState = GameState.Exploration;

        private GameStateMachine _machine;

        public GameStateMachine Machine => _machine;

        private void Awake()
        {
            _machine = new GameStateMachine();

            if (ServiceLocator.IsRegistered<GameStateMachine>())
                ServiceLocator.Unregister<GameStateMachine>();

            ServiceLocator.Register(_machine);

            if (!ServiceLocator.IsRegistered<StageProgressService>())
                ServiceLocator.Register(new StageProgressService());

            _machine.SetState(_initialState);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.IsRegistered<GameStateMachine>())
                ServiceLocator.Unregister<GameStateMachine>();
        }
    }
}