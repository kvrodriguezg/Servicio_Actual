using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBridgeAudioUploader.Models
{
    [Table("TBL_TCONFIGURACION_SERVICIOS_IA")]
    public class Servicio
    {
        [Key]
        [Column("CONF_NID")]
        public long IdServicio { get; set; }
        
        [Column("CONF_CNOMBRE_SERVICIO")]
        public string Descripcion { get; set; }
        
        [Column("CONF_CUSUARIO_FTP")]
        public string UserFTP { get; set; }
        
        [Column("CONF_CPASSWORD_FTP")]
        public string PassFTP { get; set; }
        
        [Column("CONF_CFTP")]
        public string UrlFTP { get; set; }
        
        [Column("CONF_BESTADO")]
        public bool Activo { get; set; }
        
        [Column("CONF_CDETALLE1")]
        public string ContenedorAzure { get; set; }
    }
}
