using Data;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Entry point in the Boot scene. Resets global state, registers services, then loads the first scene.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string _firstScene = SceneNames.MainMenu;
        [SerializeField] [Min(1)] private int _debugStartingStage = 1;
        [SerializeField] private ContentCatalog _contentCatalog;

        private void Awake()
        {
            EventBus.Clear();
            ServiceLocator.Clear();

            RegisterServices();

            SceneManager.LoadScene(_firstScene);
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(new StageProgressService(_debugStartingStage));

            if (_contentCatalog == null)
            {
                Debug.LogError("[Bootstrap] ContentCatalog is not assigned. Saves cannot resolve definitions.");
                return;
            }

            ServiceLocator.Register(_contentCatalog);
            ServiceLocator.Register(new SaveSystem(_contentCatalog));
        }
    }
}