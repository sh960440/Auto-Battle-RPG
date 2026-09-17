using System;
using System.IO;
using Data;
using Infrastructure;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Saves and loads player progress.
    /// </summary>
    public class SaveSystem
    {
        public const string DefaultFileName = "save.json";

        private readonly ContentCatalog _catalog;
        private readonly string _filePath;

        /// <summary>
        /// Creates a save system bound to a content catalog.
        /// </summary>
        public SaveSystem(ContentCatalog catalog, string filePath = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? Path.Combine(Application.persistentDataPath, DefaultFileName)
                : filePath;
        }

        public string FilePath => _filePath;

        /// <summary>
        /// Whether a save file exists on disk.
        /// </summary>
        public bool Exists() => File.Exists(_filePath);

        /// <summary>
        /// Deletes the save file when present.
        /// </summary>
        public void Delete()
        {
            if (!Exists())
                return;

            File.Delete(_filePath);
        }

        /// <summary>
        /// Writes the live profile and stage from the service locator.
        /// </summary>
        public void SaveCurrent()
        {
            if (!ServiceLocator.TryGet(out PlayerProfileService profileService) ||
                profileService.Profile == null)
            {
                return;
            }

            var stage = 1;
            if (ServiceLocator.TryGet(out StageProgressService progress))
                stage = progress.CurrentStage;

            Save(profileService.Profile, stage);
        }

        /// <summary>
        /// Writes <paramref name="profile"/> and <paramref name="currentStage"/>.
        /// </summary>
        public void Save(PlayerProfile profile, int currentStage)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var data = GameSaveCodec.ToSaveData(profile, currentStage);
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_filePath, json);
        }

        /// <summary>
        /// Loads a profile and stage.
        /// </summary>
        public bool TryLoad(out PlayerProfile profile, out int currentStage)
        {
            profile = null;
            currentStage = 1;

            if (!Exists())
                return false;

            try
            {
                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                var data = JsonUtility.FromJson<GameSaveData>(json);
                if (data == null)
                    return false;

                profile = GameSaveCodec.FromSaveData(data, _catalog);
                currentStage = Math.Max(1, data.currentStage);
                return profile != null && profile.Characters.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Failed to load save: {ex.Message}");
                profile = null;
                currentStage = 1;
                return false;
            }
        }
    }
}