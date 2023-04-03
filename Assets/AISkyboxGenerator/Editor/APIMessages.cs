namespace CatDarkGame.AISkyboxGenerator
{
    [System.Serializable]
    public static class APIMessages
    {
        [System.Serializable]
        public struct RequestGenerateSkybox
        {
            public string api_key;
            public string prompt;
            public int skybox_style_id;
            public string webhook_url;
        };

        [System.Serializable]
        public struct RequestGenerateImagine
        {
            public string api_key;
            public string generator;
            public string prompt;
            public string negative_text;
            public int seed;
            public string animation_mode;
            public string webhook_url;
        };

        [System.Serializable]
        public struct GeneratorParams
        {
            public string prompt;
            public string negative_text;
            public int seed;
            public string animation_mode;
        };

        [System.Serializable]
        public struct ResponeImaginebyID
        {
            public int id;
            public string obfuscated_id;
            public int user_id;
            public string title;
            public string prompt;
            public string username;
            public string status;
            public int queue_position;
            public string file_url;
            public string thumb_url;
            public string created_at;
            public string updated_at;
            public string error_message;
            public string pusher_channel;
            public string pusher_event;
            public string type;
            public string generator;
            public GeneratorParams generator_data;
        };
    }
}