using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataBridgeAudioUploader.Utils
{
    public class IaStudioManager
    {
        public async Task<bool> SubirAudioAsync(string rutaArchivo, string metadataJson, string apiUrl, string token)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Añadir Bearer token en los headers de autorización
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    using (var content = new MultipartFormDataContent())
                    {
                        // 1. Añadir el archivo físico
                        // Abrimos el archivo como FileStream para enviar los datos como flujo, o usamos ByteArrayContent.
                        // Usar StreamContent es más eficiente para no cargar todo a la RAM al mismo tiempo si es muy grande.
                        var fileStream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read);
                        var streamContent = new StreamContent(fileStream);

                        // Establecer Content-Type, preferiblemente application/octet-stream aunque depende del API
                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        
                        // El nombre del campo que el API espera es 'file'
                        content.Add(streamContent, "file", Path.GetFileName(rutaArchivo));

                        // 2. Añadir la metadata (JSON)
                        // El nombre del campo que el API espera es 'data'
                        var textContent = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json");
                        content.Add(textContent, "data");

                        // Realizar el POST
                        var response = await httpClient.PostAsync(apiUrl, content);

                        // Verificar que el StatusCode sea estrictamente OK (200)
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception)
            {
                // En un escenario real aquí podrías guardar logs o capturar excepciones específicas
                return false;
            }
        }
    }
}
