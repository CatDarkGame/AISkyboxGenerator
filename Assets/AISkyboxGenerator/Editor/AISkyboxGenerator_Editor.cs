using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace CatDarkGame.AISkyboxGenerator
{
    public class AISkyboxGenerator_Editor : EditorWindow
    {
        public static readonly Vector2 WindowSize = new Vector2(650.0f, 560.0f);
        public static readonly int WindowPaddingSize = 10;
        public static readonly int GenerateButtonHeight = 30;
        public static readonly int ApplySkyboxButtonHeight = 30;

        [MenuItem(EditorStrings.MenuItemPath, false, 0)]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<AISkyboxGenerator_Editor>(true, EditorStrings.EditorTitle, true);
            window.minSize = WindowSize;
            window.maxSize = window.minSize;
        }

        private string _prompt;
        private Vector2 _promptScrollPos = Vector2.zero;
        private BlockAdeAPI.SkyboxStyles _skyboxStyles = BlockAdeAPI.SkyboxStyles.FantasyLandscape;

        private bool IsProgress = false;
        private float Progress_Amount = 0.0f;
        private string Progress_Msg = string.Empty;

        private Texture2D _previewTexture;

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(WindowPaddingSize, WindowPaddingSize, WindowPaddingSize, WindowPaddingSize) });

            DrawPromptField();
            DrawGenerateAndProgress();
            DrawTexturePreview();

            EditorGUILayout.EndVertical();
        }

      

        private void DrawPromptField()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorStrings.Label_SkyboxStyles + "  ");
            _skyboxStyles = (BlockAdeAPI.SkyboxStyles)EditorGUILayout.EnumPopup(_skyboxStyles, GUILayout.Width(150));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorStrings.Button_OpenAPISettings))
            {
                SettingsService.OpenProjectSettings("Project/AI Skybox Generator");
            }
       
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorStrings.Label_Prompt);
            GUILayout.FlexibleSpace();
          
            EditorGUI.BeginChangeCheck();
            int promptTemplateIndex = 0;
            promptTemplateIndex = EditorGUILayout.Popup(promptTemplateIndex, PromptTemplate.Templates);
            if (EditorGUI.EndChangeCheck())
            {
                if (promptTemplateIndex>0) _prompt = PromptTemplate.Templates[promptTemplateIndex];
            }
            EditorGUILayout.EndHorizontal();

            _promptScrollPos = EditorGUILayout.BeginScrollView(_promptScrollPos, GUILayout.Height(100));
            bool isAPISetting = CheckAPISettings();
            EditorGUI.BeginDisabledGroup(!isAPISetting);
            if (!isAPISetting) _prompt = EditorStrings.Text_APISettingisNull;
            if (isAPISetting && _prompt == EditorStrings.Text_APISettingisNull) _prompt = string.Empty;
            _prompt = GUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }

        private void DrawGenerateAndProgress()
        {
            Rect progressBarRect = new Rect(WindowPaddingSize, GUILayoutUtility.GetRect(0f, 0f).y, position.width - WindowPaddingSize * 2, GenerateButtonHeight);
            if (!IsProgress)
            {
                bool isAPISetting = CheckAPISettings();
                EditorGUI.BeginDisabledGroup(!isAPISetting);
                if (GUILayout.Button(EditorStrings.Button_Generate, GUILayout.Width(progressBarRect.width), GUILayout.Height(progressBarRect.height)))
                {
                    GenerateSkybox();
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.ProgressBar(progressBarRect, Progress_Amount, Progress_Msg);
                GUILayout.Space(progressBarRect.height);
            }
        }

        private void DrawTexturePreview()
        {
            GUILayout.Space(10);
            HorizontalLine(Color.gray, 1, Vector2.one * 2);
            EditorGUILayout.Space(5);
            float width = position.width - 20;
            Rect rect = new Rect(WindowPaddingSize, GUILayoutUtility.GetRect(0f, 0f).y, width, width / 2);
            if (_previewTexture)
            {
                EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, ScaleMode.ScaleToFit);
            }
            else
            {
                var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 30 };
                EditorGUI.LabelField(rect, "", style);
            }
            GUILayout.Space(rect.height + 5);

            EditorGUI.BeginDisabledGroup(!_previewTexture);
            if (GUILayout.Button(EditorStrings.Button_ApplyScene, GUILayout.Width(rect.width), GUILayout.Height(ApplySkyboxButtonHeight)))
            {
                ApplySkybox(_previewTexture);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void HorizontalLine(Color color, float height, Vector2 margin)
        {
            GUILayout.Space(margin.x);
            Rect rect = new Rect(WindowPaddingSize, GUILayoutUtility.GetRect(0f, 0f).y, position.width - WindowPaddingSize * 2, height);
            EditorGUI.DrawRect(rect, color);
            GUILayout.Space(margin.y);
        }

        private void UpdateProgress(string msg, float amount)
        {
            Progress_Amount = amount;
            Progress_Msg = msg;
        }

        private async void GenerateSkybox()
        {
            IsProgress = true;
            _previewTexture = null;
            int requestDelayTime_First = BlockAdeAPI.APIRequestDelayTime_First;
            int requestDelayTime_Loop = BlockAdeAPI.APIRequestDelayTime_Loop;
            string saveImageNameHeader = "AISkybox_";
            string apiKey = AISkyboxGeneratorSettings.instance.API_Key;
            string msg = BlockAdeAPI.CreateGenerateSkyboxMessage(apiKey, (int)_skyboxStyles, _prompt);
            int requestID = -1;
            string imageURL = string.Empty;

            Debug.Log(EditorStrings.Log_StartSkyboxGenerating + "\n" + msg);

            {
                UpdateProgress(EditorStrings.ProgressBar_StartSkyboxGenerating, 0.1f);

                string url = BlockAdeAPI.URL.GenerateSkybox(apiKey);
                BlockAdeAPI.Result result = await BlockAdeAPI.SendRequestSkyboxGenerateAsync(url, msg);

                bool isError = result.Error != null;
                string errorMsg = BlockAdeAPI.GetErrorMesage(result.ReponseCode);
                if (isError)
                {
                    Debug.LogError(errorMsg);
                    ProgressClear();
                    return;
                }

                var responeData = JsonUtility.FromJson<APIMessages.ResponeImaginebyID>(result.Value);
                requestID = responeData.id;
            }

            {
                UpdateProgress(EditorStrings.ProgressBar_RequestImage, 0.2f);
                await Task.Delay(requestDelayTime_First);
                string url = BlockAdeAPI.URL.ImagineRequest(apiKey, requestID.ToString());
                BlockAdeAPI.Result result = await BlockAdeAPI.SendGetImagebyIDAsync(url, requestDelayTime_Loop, (statusMessage) =>
                {
                    switch(statusMessage)
                    {
                        case "pending":
                            UpdateProgress(EditorStrings.ProgressBar_RequestImage_Pending, 0.3f);
                            break;
                        case "dispatched":
                            UpdateProgress(EditorStrings.ProgressBar_RequestImage_Dispatched, 0.55f);
                            break;
                        case "processing":
                            UpdateProgress(EditorStrings.ProgressBar_RequestImage_Processing, 0.6f);
                            break;
                    }
                });
                bool isError = result.Error != null;
                string errorMsg = BlockAdeAPI.GetErrorMesage(result.ReponseCode);
                if (isError)
                {
                    Debug.LogError(errorMsg);
                    ProgressClear();
                    return;
                }

                imageURL = result.Value;
                UpdateProgress(EditorStrings.ProgressBar_RequestImage_Complete, 0.8f);
                await Task.Delay(1000);
            }

            {
                Debug.Log(EditorStrings.Log_ImageDownload + " - " + imageURL);
                UpdateProgress(EditorStrings.ProgressBar_DownloadImage, 0.85f);
                Texture2D downloadImage = await ImageProcessor.DownloadImage(imageURL);
                if(downloadImage == null)
                {
                    Debug.LogError(EditorStrings.LogError_DownloadImageError + " - " + imageURL);
                    ProgressClear();
                    return;
                }

                UpdateProgress(EditorStrings.ProgressBar_SaveImage, 0.95f);
                string randomIndex = System.DateTime.Now.ToString("HHmmss");
                string sceneFolderPath = ImageProcessor.GetActiveSceneFolderPath();
                string fileName = saveImageNameHeader + randomIndex + ".png";
                string assetPath = sceneFolderPath + "/" + fileName;
                Texture2D imageAsset = ImageProcessor.SaveImageFile(assetPath, downloadImage);
                EditorGUIUtility.PingObject(imageAsset);

                _previewTexture = imageAsset;

                Debug.Log(EditorStrings.Log_GenerateComplete + " - " + assetPath);
            }

            ProgressClear();
        }

        private bool CheckAPISettings()
        {
            if(AISkyboxGeneratorSettings.instance.API_Key.Length<=0) return false;
            return true;
        }
        
        private void ProgressClear()
        {
            IsProgress = false;
        }

        private void ApplySkybox(Texture2D texture)
        {
            string sceneFolderPath = ImageProcessor.GetActiveSceneFolderPath();
            string assetPath = sceneFolderPath + "/" + "M_AISkybox.mat";
            
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (!material)
            {
                material = new Material(Shader.Find("Skybox/Panoramic"));
                AssetDatabase.CreateAsset(material, assetPath);
            }
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_Exposure", 1.18f);

            RenderSettings.skybox = material;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}