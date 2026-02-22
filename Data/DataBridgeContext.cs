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
        }

        public DbSet<Servicio> Servicios { get; set; }

        public List<Servicio> GetServiciosActivos()
        {
            return Servicios.Where(s => s.Activo).ToList();
        }
    }
}
