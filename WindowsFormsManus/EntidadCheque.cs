using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsManus
{
    public class EntidadCheque
    {

        public EntidadCheque(string imagePath, string monto, string cliente, string montoEscrito, string serie, string numero, string tipoCheque, string fecha)
        {
            ImagePath = imagePath;
            Monto = monto;
            Cliente = cliente;
            MontoEscrito = montoEscrito;
            Serie = serie;
            Numero = numero;
            TipoCheque = tipoCheque;
            Fecha = fecha;
        }

        public string ImagePath { get; set; }
        public string Monto { get; set; }
        public string Cliente { get; set; }
        public string MontoEscrito { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string TipoCheque { get; set; }
        public string Fecha { get; set; }

    }
}
