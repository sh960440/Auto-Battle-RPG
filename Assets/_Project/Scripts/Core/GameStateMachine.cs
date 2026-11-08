using System;

namespace Core
{
    /// <summary>
    /// Manages the state of the game.
    /// </summary>
    public class GameStateMachine
    {
        private GameState _currentState;
        private bool _hasCurrentState;

        /// <summary>
        /// The current state.
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Fired when a state is entered.
        /// </summary>
        public event Action<GameState> StateEntered;

        /// <summary>
        /// Fired when a state is exited.
        /// </summary>
        public event Action<GameState> StateExited;

        /// <summary>
        /// Fired when the state changes.
        /// </summary>
        public event Action<GameState, GameState> StateChanged;

        /// <summary>
        /// Switches to a new state and invokes exit/enter callbacks.
        /// </summary>
        /// <param name="newState">State to enter.</param>
        public void SetState(GameState newState)
        {
            if (_hasCurrentState && _currentState == newState)
                return;

            if (_hasCurrentState)
            {
                var previous = _currentState;
                StateExited?.Invoke(previous);
                _currentState = newState;
                StateEntered?.Invoke(_currentState);
                StateChanged?.Invoke(previous, _currentState);
                return;
            }

            _currentState = newState;
            _hasCurrentState = true;
            StateEntered?.Invoke(_currentState);
        }
    }
}