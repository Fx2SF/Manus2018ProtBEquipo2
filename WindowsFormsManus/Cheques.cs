namespace WindowsFormsManus
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cheques
    {
        [Key]
        public int idCheque { get; set; }

        [StringLength(150)]
        public string imagePath { get; set; }

        [StringLength(150)]
        public string monto { get; set; }

        [StringLength(150)]
        public string cliente { get; set; }

        [StringLength(150)]
        public string montoEscrito { get; set; }

        [StringLength(150)]
        public string serie { get; set; }

        [StringLength(150)]
        public string numero { get; set; }

        [StringLength(150)]
        public string tipoCheque { get; set; }

        [StringLength(150)]
        public string fecha { get; set; }
    }
}
