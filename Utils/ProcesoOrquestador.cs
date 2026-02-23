using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DataBridgeAudioUploader.Data;
using Newtonsoft.Json;

namespace DataBridgeAudioUploader.Utils
{
    public class ProcesoOrquestador
    {
        private readonly string _eventLogSource = "DataBridgeAudioUploader";
        private readonly string _eventLogName = "Application";

        public ProcesoOrquestador()
        {
            try
            {
                // Verifica si el origen para el log de eventos existe, si no lo crea. 
                // Nota: para crear orígenes usualmente se requiere permisos de Administrador.
                if (!EventLog.SourceExists(_eventLogSource))
                {
                    EventLog.CreateEventSource(_eventLogSource, _eventLogName);
                }
            }
            catch
            {
                // Si ejecutamos sin privilegios, fallaría la verificación de existencia o creación
                // Se continúa usando el origen para intentar reportar de todos modos
            }
        }

        // Se usa Async para poder llamar a métodos que son Task asíncronos y no bloquear el hilo
        public async Task Ejecutar()
        {
            try
            {
                // 1) Instanciar el contexto usando Entity Framework para ubicar los servicios
                using (var context = new DataBridgeContext())
                {
                    var servicios = context.GetServiciosActivos();
                    var ftpManager = new FtpManager();
                    var azureStorageManager = new AzureStorageManager();

                    foreach (var servicio in servicios)
                    {
                        // Leer el campo de metadata de la BD y deserializar a diccionario
                        Dictionary<string, string> metadata = new Dictionary<string, string>();
                        if (!string.IsNullOrWhiteSpace(servicio.MetadataJson))
                        {
                            try
                            {
                                metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(servicio.MetadataJson) 
                                           ?? new Dictionary<string, string>();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al deserializar JSON de metadata para servicio {servicio.IdServicio}: {ex.Message}");
                            }
                        }

                        // Agregar o sobreescribir las llaves solicitadas
                        metadata["agentId"] = "00";
                        metadata["datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Creamos una carpeta temporal única por servicio descargado
                        string tempRuta = Path.Combine(Path.GetTempPath(), "DataBridgeUploads", servicio.IdServicio.ToString());

                        // 2) Llamar a FtpManager para descargar los audios a una carpeta temporal
                        await ftpManager.DescargarAudiosAsync(servicio, tempRuta);

                        // Si la descarga resultó con la creación y poblamiento de la carpeta iteramos:
                        if (Directory.Exists(tempRuta))
                        {
                            var archivos = Directory.GetFiles(tempRuta);

                            foreach (var archivo in archivos)
                            {
                                // 3) Llama a AzureStorageManager pasando la ruta del archivo y el contenedor (de la BD)
                                string nombreContenedor = string.IsNullOrWhiteSpace(servicio.NombreContenedor) ? "default-container" : servicio.NombreContenedor;
                                
                                bool exito = await azureStorageManager.SubirAudioAzureAsync(archivo, nombreContenedor, metadata);

                                // 4) Si la subida es exitosa, eliminar o limpiar archivo temporal local
                                if (exito)
                                {
                                    File.Delete(archivo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorEnEventLog(ex);
            }
        }

        private void RegistrarErrorEnEventLog(Exception ex)
        {
            // Registramos finalmente el error localizando el Exception y su traza
            try
            {
                using (EventLog eventLog = new EventLog(_eventLogName))
                {
                    eventLog.Source = _eventLogSource;
                    eventLog.WriteEntry($"Excepción grave en ProcesoOrquestador de DataBridge:\n\n{ex.Message}\n\nStacktrace:\n{ex.StackTrace}", EventLogEntryType.Error, 101);
                }
            }
            catch
            {
                // Catch silencioso en caso extremo, ya que tratar con los ErrorLogs a veces falla por falta de permisos.
            }
        }
    }
}
