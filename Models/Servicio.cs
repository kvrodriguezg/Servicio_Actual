using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBridgeAudioUploader.Models
{
    [Table("Servicio")]
    public class Servicio
    {
        [Key]
        public int IdServicio { get; set; }
        
        public string Descripcion { get; set; }
        
        public string UserFTP { get; set; }
        
        public string PassFTP { get; set; }
        
        public string UrlFTP { get; set; }
        
        public bool Activo { get; set; }
    }
}
