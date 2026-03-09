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
            //Extraer el host si el usuario introdujo un prefijo (ej. ftp://)
            string host = servicio.UrlFTP.Replace("ftp://", "").Replace("ftps://", "");

            //Inicializar el cliente FTP de forma asíncrona
            using (var ftp = new AsyncFtpClient(host, servicio.UserFTP, servicio.PassFTP))
            {
                //Conectar al servidor de manera asíncrona
                Console.WriteLine($"Tratando de conectar al FTP en {host} con el usuario {servicio.UserFTP}...");
                await ftp.Connect();
                Console.WriteLine("Conectado de diez.");

                //Asegurar que la carpeta local destino existe
                if (!Directory.Exists(rutaLocalTemp))
                {
                    Directory.CreateDirectory(rutaLocalTemp);
                }

                //Asegurar que la carpeta 'Procesados' exista en la raíz del FTP
                string procesadosDir = "/Procesados";
                if (!await ftp.DirectoryExists(procesadosDir))
                {
                    Console.WriteLine("No vi la carpeta 'Procesados' en el FTP, así que la voy a crear...");
                    await ftp.CreateDirectory(procesadosDir);
                }

                //Obtener el listado de archivos de la base del FTP
                Console.WriteLine("Viendo qué onda en la raíz del FTP...");
                var items = await ftp.GetListing("/");

                //Filtrar para obtener solo archivos con extensión .mp3 o .wav
                var audios = items.Where(i => i.Type == FtpObjectType.File &&
                                              (i.Name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                               i.Name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)))
                                  .ToList();

                Console.WriteLine($"Bien ahí, encontré {audios.Count} archivos de audio para descargar.");

                foreach (var archivo in audios)
                {
                    string localFilePath = Path.Combine(rutaLocalTemp, archivo.Name);

                    //Descargar el archivo localmente
                    Console.WriteLine($"Bajando el archivo '{archivo.Name}'...");
                    var resultado = await ftp.DownloadFile(localFilePath, archivo.FullName);

                    //Si la descarga fue exitosa, mover el archivo original en el FTP
                    if (resultado == FtpStatus.Success)
                    {
                        Console.WriteLine($"Archivo '{archivo.Name}' descargado, ahora lo muevo a Procesados en el FTP para no tocarlo de nuevo...");
                        string destinoProcesado = $"{procesadosDir}/{archivo.Name}";
                        await ftp.MoveFile(archivo.FullName, destinoProcesado, FtpRemoteExists.Overwrite);
                    }
                    else
                    {
                        Console.WriteLine($"Uy, hubo drama bajando el archivo '{archivo.Name}'.");
                    }
                }

                //Desconectar explícitamente al final
                Console.WriteLine("Ya terminé con este FTP, cierro la conexión por ahora.");
                await ftp.Disconnect();
            }
        }
    }
}
