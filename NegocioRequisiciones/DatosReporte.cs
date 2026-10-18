using System.Data;

namespace NegocioRequisiciones
{
    // Datos ya leídos de las vistas, listos para mostrar o imprimir.
    public sealed class DatosReporte
    {
        public DataTable Encabezado { get; }
        public DataTable Detalle { get; }

        public DatosReporte(DataTable encabezado, DataTable detalle)
        {
            Encabezado = encabezado;
            Detalle = detalle;
        }
    }
}
