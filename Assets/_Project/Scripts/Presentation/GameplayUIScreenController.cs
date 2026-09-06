using Core;
using Infrastructure;
using UnityEngine;

namespace Presentation
{
    /// <summary>
    /// Shows and hides UI roots based on the active <see cref="GameState"/>.
    /// </summary>
    public class GameplayUIScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject _mainHUDRoot;
        [SerializeField] private GameObject _combatHUDRoot;

        private GameStateMachine _stateMachine;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _stateMachine))
            {
                Debug.LogWarning($"{nameof(GameplayUIScreenController)}: {nameof(GameStateMachine)} is not registered.");
                return;
            }

            _stateMachine.StateChanged += HandleStateChanged;
            ApplyScreen(_stateMachine.CurrentState);
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            ApplyScreen(current);
        }

        private void ApplyScreen(GameState state)
        {
            if (_mainHUDRoot != null)
                _mainHUDRoot.SetActive(true);

            if (_combatHUDRoot != null)
                _combatHUDRoot.SetActive(state == GameState.Combat || state == GameState.Result);
        }
    }
}