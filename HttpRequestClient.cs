using System.Linq;
using Newtonsoft.Json.Linq;

 
    using System.Linq;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Godot;

    namespace Helpers
    {
        public class HttpRequestClient
        {
            private string url = "";
            public enum RequestContentType
            {
                Json,
                FormData
            }

            public delegate void ResponseCallback(string responseBody);

            // Méthode asynchrone pour effectuer une requête HTTP GET
            public async Task GetRequest(string endpoint, ResponseCallback callback)
            {
               
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + endpoint);
                request.Method = "GET";
                await SendRequest(request, callback, null, RequestContentType.Json);
            }

            // Méthode asynchrone mise à jour pour supporter Json ou FormData
            public async Task PostRequest(string endpoint, string data, RequestContentType contentType,
                ResponseCallback callback)
            {
                 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + endpoint);
                request.Method = "POST";

                // Définit le ContentType en fonction du type de contenu spécifié
                switch (contentType)
                {
                    case RequestContentType.Json:
                        request.ContentType = "application/json";
                        break;
                    case RequestContentType.FormData:
                        request.ContentType = "application/x-www-form-urlencoded";
                        break;
                }

                await SendRequest(request, callback, data, contentType);
            }

            // Ajoute un paramètre pour le type de contenu à SendRequest
            private async Task SendRequest(HttpWebRequest request, ResponseCallback callback, string data, RequestContentType contentType)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                // Ajoute les headers personnalisés à la requête
                request.Accept = "application/json";
                

                // Ajoute le corps de la requête pour POST
                if (data != null)
                {
                    using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
                    {
                        streamWriter.Write(data);
                    }
                }

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var responseBody = await reader.ReadToEndAsync();
                        callback?.Invoke(responseBody);
                    }
                }
                catch (WebException e)
                {
                    GD.Print("Error in HTTP request: ", e.Message);
                    if (e.Response is HttpWebResponse errorResponse)
                    {
                        using (Stream responseStream = errorResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            
                            string errorText = await reader.ReadToEndAsync();
                            var json = JToken.Parse(errorText);
                            var errorMessage = (string)json["payload"]["message"]; 
                             callback?.Invoke(json.ToString());
                             return;
                        }
                    }
                    GD.PrintErr("Erreur de requête HTTP: ", e.Message);
                    callback?.Invoke(null);
                }
            }
        }
    }
