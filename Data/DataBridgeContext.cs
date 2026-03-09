using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataBridgeAudioUploader.Models;

namespace DataBridgeAudioUploader.Data
{
    public class DataBridgeContext : DbContext
    {
        public DataBridgeContext() : base("name=DataBridgeConnection")
        {
            //Desactivar el chequeo de "migrations" para omitir si la base ya estaba creada y es diferente
            Database.SetInitializer<DataBridgeContext>(null);
        }

        public DbSet<Servicio> Servicios { get; set; }

        public List<Servicio> GetServiciosActivos()
        {
            return Servicios.Where(s => s.Activo).ToList();
        }
    }
}
