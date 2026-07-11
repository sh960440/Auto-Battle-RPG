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

        private void Awake()
        {
            EventBus.Clear();
            ServiceLocator.Clear();

            RegisterServices();

            SceneManager.LoadScene(_firstScene);
        }

        private static void RegisterServices()
        {
            // ServiceLocator.Register<ISomeService>(...)
        }
    }
}