using System;
using System.ServiceProcess;
using DataBridgeAudioUploader.Utils; //Ajusta según tu namespace real

namespace DataBridgeAudioUploader
{
    static class Program
    {
        static void Main()
        {
            //Consola
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Acá empezando a correr el RPA en modo consola, dale que te sigo...");
                try
                {
                    SincronizadorAudios orquestador = new SincronizadorAudios();
                    orquestador.Ejecutar().Wait(); //Esperamos a que termine ya que ahora es async (o bloqueará la app).
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Uhhh chabón, tiró un error re feo: " + ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine("El chisme completo: " + ex.InnerException.Message);
                }

                Console.ResetColor();
                Console.WriteLine("\nBueno, ya terminó todo por hoy. Apretá cualquier tecla para salir...");
                Console.ReadKey();
            }
            else
            {
                //Servicio
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new Service1() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}