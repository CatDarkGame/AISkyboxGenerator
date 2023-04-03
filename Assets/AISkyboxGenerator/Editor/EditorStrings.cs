namespace CatDarkGame.AISkyboxGenerator
{
    public static class EditorStrings 
    {
        public const string MenuItemPath = "AI Tools/AI Skybox Generator";
        public const string EditorTitle = "AI Skybox Generator";

        public const string Label_SkyboxStyles = "Skybox Styles";
        public const string Label_Prompt = "Prompt";

        public const string Button_Generate = "Generate Skybox";
        public const string Button_OpenAPISettings = "Open API Settings";
        public const string Button_ApplyScene = "Apply to Skybox in Scene";

        public const string ProgressBarTitle = "AI Skybox Generator";
        public const string ProgressBar_StartSkyboxGenerating = "Request Skybox Generating...";
        public const string ProgressBar_RequestImage = "Request Imagine Process...";
        public const string ProgressBar_RequestImage_Pending = "Waiting for request...";
        public const string ProgressBar_RequestImage_Dispatched = "Dispatched request...";
        public const string ProgressBar_RequestImage_Processing = "Processing generate Image...";
        public const string ProgressBar_RequestImage_Complete = "Image Generating Comptelete...";

        public const string ProgressBar_DownloadImage = "Download Image from URL...";
        public const string ProgressBar_SaveImage = "Save Image...";

        public const string Log_StartSkyboxGenerating = "Start AI Skybox Generator";
        public const string Log_ImageDownload = "Image Download";
        public const string Log_GenerateComplete = "Generate Skybox Complete";

        public const string LogError_DownloadImageError = "Download Image Error";

        public const string Text_APISettingisNull = "Set API First \n File > Build Settings > Player Settings > AI Skybox Generator";
    }


    public static class PromptTemplate
    {
        public static string[] Templates = {"Prompt Template",
        "forest in autumn, detailed digital painting,cinematic lighting",
        "cat world, minecraft, blue sky, volumetric fog, volumetric light, hdr",

        };
    }


}