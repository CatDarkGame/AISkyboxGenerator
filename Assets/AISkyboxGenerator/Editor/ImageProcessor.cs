using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace CatDarkGame.AISkyboxGenerator
{
    public static class ImageProcessor 
    {
        private const string FolderName = "AISkyboxGenerator_Results";

        public static async Task<Texture2D> DownloadImage(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, false);
            var asyncReq = request.SendWebRequest();
            while (!asyncReq.webRequest.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError ||
               request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(asyncReq.webRequest.result + ": " + asyncReq.webRequest.error + "\n" +
                      "Url : " + asyncReq.webRequest.url + "\n" +
                      asyncReq.webRequest.downloadHandler.text);
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(asyncReq.webRequest);
            request.Dispose();
            request = null;

            return texture;
        }

        public static Texture2D SaveImageFile(string path, Texture2D texture2D)
        {
            byte[] _bytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(path, _bytes);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (asset != null)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 8192;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                }
            }
           
            return asset;
        }

        public static string GetActiveSceneFolderPath()
        {
            string scenePath = EditorSceneManager.GetActiveScene().path;
            string sceneFolderPath = scenePath.Substring(0, scenePath.LastIndexOf("/"));
            string resultPath = sceneFolderPath + "/" + FolderName;
            if (AssetDatabase.IsValidFolder(resultPath) == false)
                AssetDatabase.CreateFolder(sceneFolderPath, FolderName);

            return resultPath;
        }
    }
}