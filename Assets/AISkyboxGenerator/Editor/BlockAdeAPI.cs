using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CatDarkGame.AISkyboxGenerator
{
    public static class BlockAdeAPI
    {
        public enum SkyboxStyles
        {
            FantasyLandscape = 2,
            AnimeArtStyle = 3,
            SurealStyle = 4,
            DigitalPainting = 5,
            Scenic = 6,
            Nebula = 7,
            Realistic = 9,
            SciFi = 10,
            Dreamlike = 11,
            InteriorViews = 15,
            Sky = 16,
            DutchMasters = 17,
            ModernComputerAnimation = 20,
            Infrared = 18,
            LoyPoly = 19,
            NoStylewords = 13,
        };

        public enum APIErrors
        {
            InvalidGeneratorData = 400,
            InvalidAccount = 403,
        };

        public static class URL
        {
            public static string GenerateSkybox(string api_Key)
            {
                return string.Format("https://backend.blockadelabs.com/api/v1/skybox?api_key={0}", api_Key);
            }

            public static string ImagineRequest(string api_Key, string id)
            {
                return string.Format("https://backend.blockadelabs.com/api/v1/imagine/requests/{1}?api_key={0}", api_Key, id);
            }
        }


        public static readonly int APIRequestDelayTime_First = 5000;    // millisecond Time
        public static readonly int APIRequestDelayTime_Loop = 5000;     // If you make too many calls to the API too quickly, it will be throttled.


        public struct Result
        {
            public string Value;
            public string Error;
            public long ReponseCode;

            public Result(string value = "", string error = "", long responseCode = 0)
            {
                Value = value;
                Error = error;
                ReponseCode = responseCode;
            }
        }

        public static string GetErrorMesage(long ReponseCode)
        {
            switch ((APIErrors)ReponseCode)
            {
                case APIErrors.InvalidGeneratorData:
                    return "Various invalid generator data related errors";
                case APIErrors.InvalidAccount:
                    return "Account does not have access to this method";
            }
            return "Error";
        }


        public static string CreateGenerateSkyboxMessage(string api_Key, int skyboxStyleID, string prompt)
        {
            var msg = new APIMessages.RequestGenerateSkybox();
            msg.api_key = api_Key;
            msg.prompt = prompt;
            msg.skybox_style_id = skyboxStyleID;
            msg.webhook_url = "https://example.com/webhook_endpoint";

            return JsonUtility.ToJson(msg);
        }

        public static async Task<Result> SendRequestSkyboxGenerateAsync(string url, string requestMessage)
        {
            Result result = new Result();
            
            UnityWebRequest request = UnityWebRequest.PostWwwForm(url, requestMessage);
            byte[] rawBody = Encoding.UTF8.GetBytes(requestMessage);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

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
                result.Error = asyncReq.webRequest.error;
                return result;
            }

            result.ReponseCode = asyncReq.webRequest.responseCode;
            result.Value = request.downloadHandler.text;

            request.Dispose();
            request = null;

            return result;
        }

        /// <summary>
        /// Repeatedly call the API until image generation is complete
        //      this is not recommended as it could potentially trigger our API rate limiter
        //      recommend using the Pusher Library or Webhook
        /// </summary>
        public static async Task<Result> SendGetImagebyIDAsync(string url, int loopDelayTime, Action<string> statusMessage)
        {
            Result result = new Result();
            
            do
            {
                UnityWebRequest request = UnityWebRequest.Get(url);
                var asyncReq = request.SendWebRequest();
                while (!asyncReq.webRequest.isDone)
                    await Task.Yield();

                result.ReponseCode = asyncReq.webRequest.responseCode;
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(asyncReq.webRequest.result + ": " + asyncReq.webRequest.error + "\n" +
                            "Url : " + asyncReq.webRequest.url + "\n" +
                            asyncReq.webRequest.downloadHandler.text);
                    result.Error = asyncReq.webRequest.error;
                    break;
                }

                var responeJson = request.downloadHandler.text;
                responeJson = responeJson.Substring(11, responeJson.Length - 12);
                var responeData = JsonUtility.FromJson<APIMessages.ResponeImaginebyID>(responeJson);

                statusMessage(responeData.status);

                if (responeData.status == "complete")
                {
                    result.Value = responeData.file_url;
                    break;
                }
                else if(responeData.status == "error")
                {
                    result.Error = "Status Error";
                    break;
                }
                request.Dispose();
                request = null;

                await Task.Delay(loopDelayTime);
            } while (true);
            
            return result;
        }
    }

}
