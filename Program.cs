using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DataBridgeAudioUploader
{
    internal static class Program
    {
        static void Main()
        {
            #if DEBUG
                //MODO CONSOLA
                Console.WriteLine("Iniciando RPA en modo prueba (Consola)...");
        
                var orquestador = new DataBridgeAudioUploader.Utils.ProcesoOrquestador();
        
                orquestador.Ejecutar(); 
        
                Console.WriteLine("Proceso finalizado. Presiona Enter para salir.");
                Console.ReadLine();
            #else
                //MODO PRODUCCIÓN
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1() 
                };
                ServiceBase.Run(ServicesToRun);
            #endif
        }   
    }
}
