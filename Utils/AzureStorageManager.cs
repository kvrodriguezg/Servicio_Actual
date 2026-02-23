using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DataBridgeAudioUploader.Utils
{
    public class AzureStorageManager
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=sausaistudiocl;AccountKey=[PEGA TU KEY AQUÍ];EndpointSuffix=core.windows.net";

        public async Task<bool> SubirAudioAzureAsync(string rutaLocal, string nombreContenedor, Dictionary<string, string> metadata)
        {
            try
            {
                if (!File.Exists(rutaLocal))
                {
                    Console.WriteLine($"El archivo {rutaLocal} no existe.");
                    return false;
                }

                // Obtener el nombre del archivo de la ruta local
                string nombreArchivo = Path.GetFileName(rutaLocal);
                
                // Crear el cliente del servicio de blobs
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                
                // Obtener la referencia al contenedor
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(nombreContenedor);
                
                // Crear el contenedor si no existe (opcional)
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                // La ruta destino dentro del contenedor, en la carpeta 'audio'
                string blobName = $"audio/{nombreArchivo}";
                
                // Obtener el cliente del blob
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Configurar las opciones de subida y añadir la metadata
                BlobUploadOptions options = new BlobUploadOptions
                {
                    Metadata = metadata
                };

                // Subir el archivo asignando la metadata
                using (FileStream uploadFileStream = File.OpenRead(rutaLocal))
                {
                    await blobClient.UploadAsync(uploadFileStream, options);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Manejar error (puedes querer usar un logger aquí en lugar de Console.WriteLine)
                Console.WriteLine($"Error al subir audio a Azure: {ex.Message}");
                return false;
            }
        }
    }
}
