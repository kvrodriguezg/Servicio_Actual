using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;
using DataBridgeAudioUploader.Models;

namespace DataBridgeAudioUploader.Utils
{
    public class FtpManager
    {
        public async Task DescargarAudiosAsync(Servicio servicio, string rutaLocalTemp)
        {
            // Extraer el host si el usuario introdujo un prefijo (ej. ftp://)
            string host = servicio.UrlFTP.Replace("ftp://", "").Replace("ftps://", "");

            // Inicializar el cliente FTP de forma asíncrona
            using (var ftp = new AsyncFtpClient(host, servicio.UserFTP, servicio.PassFTP))
            {
                // Conectar al servidor de manera asíncrona
                await ftp.Connect();

                // Asegurar que la carpeta local destino existe
                if (!Directory.Exists(rutaLocalTemp))
                {
                    Directory.CreateDirectory(rutaLocalTemp);
                }

                // Asegurar que la carpeta 'Procesados' exista en la raíz del FTP
                string procesadosDir = "/Procesados";
                if (!await ftp.DirectoryExists(procesadosDir))
                {
                    await ftp.CreateDirectory(procesadosDir);
                }

                // Obtener el listado de archivos de la base del FTP
                var items = await ftp.GetListing("/");

                // Filtrar para obtener solo archivos con extensión .mp3 o .wav
                var audios = items.Where(i => i.Type == FtpObjectType.File &&
                                              (i.Name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                               i.Name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)))
                                  .ToList();

                foreach (var archivo in audios)
                {
                    string localFilePath = Path.Combine(rutaLocalTemp, archivo.Name);

                    // Descargar el archivo localmente
                    var resultado = await ftp.DownloadFile(localFilePath, archivo.FullName);

                    // Si la descarga fue exitosa, mover el archivo original en el FTP
                    if (resultado == FtpStatus.Success)
                    {
                        string destinoProcesado = $"{procesadosDir}/{archivo.Name}";
                        await ftp.MoveFile(archivo.FullName, destinoProcesado, FtpRemoteExists.Overwrite);
                    }
                }

                // Desconectar explícitamente al final
                await ftp.Disconnect();
            }
        }
    }
}
