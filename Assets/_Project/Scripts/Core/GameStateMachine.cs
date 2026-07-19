using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Manages the state of the game.
    /// </summary>
    public class GameStateMachine
    {
        private readonly Dictionary<GameState, List<Action>> _enterCallbacks = new();
        private readonly Dictionary<GameState, List<Action>> _exitCallbacks = new();

        private GameState _currentState;
        private bool _hasCurrentState;

        /// <summary>
        /// The current state.
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Whether the current state has been set.
        /// </summary>
        public bool HasCurrentState => _hasCurrentState;

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
        /// Registers a callback that runs when entering a given state.
        /// </summary>
        /// <param name="state">State to enter.</param>
        /// <param name="callback">Callback to invoke on enter.</param>
        public void OnEnter(GameState state, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!_enterCallbacks.TryGetValue(state, out var handlers))
            {
                handlers = new List<Action>();
                _enterCallbacks[state] = handlers;
            }

            handlers.Add(callback);
        }

        /// <summary>
        /// Registers a callback that runs when leaving a given state.
        /// </summary>
        /// <param name="state">State to exit.</param>
        /// <param name="callback">Callback to invoke on exit.</param>
        public void OnExit(GameState state, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!_exitCallbacks.TryGetValue(state, out var handlers))
            {
                handlers = new List<Action>();
                _exitCallbacks[state] = handlers;
            }

            handlers.Add(callback);
        }

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
                InvokeExit(previous);
                _currentState = newState;
                InvokeEnter(_currentState);
                StateChanged?.Invoke(previous, _currentState);
                return;
            }

            _currentState = newState;
            _hasCurrentState = true;
            InvokeEnter(_currentState);
        }

        private void InvokeEnter(GameState state)
        {
            StateEntered?.Invoke(state);

            if (!_enterCallbacks.TryGetValue(state, out var handlers))
                return;

            foreach (var handler in handlers)
                handler?.Invoke();
        }

        private void InvokeExit(GameState state)
        {
            StateExited?.Invoke(state);

            if (!_exitCallbacks.TryGetValue(state, out var handlers))
                return;

            foreach (var handler in handlers)
                handler?.Invoke();
        }

        // Hooks for future features.
        // Transition guards, state history, and async enter/exit will be added.
    }
}