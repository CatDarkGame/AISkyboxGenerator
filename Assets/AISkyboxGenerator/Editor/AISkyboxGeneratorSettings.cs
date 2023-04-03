using UnityEditor;

namespace CatDarkGame.AISkyboxGenerator
{
    /// <summary>
    /// Set and save the "Blockadelabs" API key value in Project Settings
    /// </summary>
    [FilePath("UserSettings/AISkyboxGeneratorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class AISkyboxGeneratorSettings : ScriptableSingleton<AISkyboxGeneratorSettings>
    {
        public string API_Key = null;
        public string API_Secret = null;
        public void Save() => Save(true);
        void OnDisable() => Save();
    }

    public sealed class AISkyboxGeneratorSettingsProvider : SettingsProvider
    {
        public AISkyboxGeneratorSettingsProvider() : base("Project/AI Skybox Generator", SettingsScope.Project) { }

        public override void OnGUI(string search)
        {
            var settings = AISkyboxGeneratorSettings.instance;
            var api_Key = settings.API_Key;
            var api_Secret = settings.API_Secret;

            EditorGUI.BeginChangeCheck();
            api_Key = EditorGUILayout.TextField("API Key", api_Key);
            api_Secret = EditorGUILayout.TextField("API Secret", api_Secret);
            if (EditorGUI.EndChangeCheck())
            {
                settings.API_Key = api_Key;
                settings.API_Secret = api_Secret;
                settings.Save();
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("You can to generate an API key - Blockadelabs Skybox");
            EditorGUILayout.LinkButton("https://skybox.blockadelabs.com/");

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Created Package by CatDarkGame");
            EditorGUILayout.LinkButton("https://twitter.com/CatDarkGame");
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new AISkyboxGeneratorSettingsProvider();
    }
}
 