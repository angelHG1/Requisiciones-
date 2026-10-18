using DatosRequisiciones;
using System.Data;

namespace NegocioRequisiciones
{
    /// Punto de entrada de la capa de negocio para la interfaz.
    /// La política de impuestos (Strategy) se elige una sola vez, al crear el servicio.

    public class ServicioRequisiciones
    {
        private readonly RepositorioRequisiciones _repositorio = new RepositorioRequisiciones();
        private readonly IPoliticaImpuesto _politicaImpuesto;

        public ServicioRequisiciones(IPoliticaImpuesto politicaImpuesto)
        {
            _politicaImpuesto = politicaImpuesto
                ?? throw new ArgumentNullException(nameof(politicaImpuesto));
        }

        public RequisicionBuilder NuevoBuilder() => new RequisicionBuilder(_politicaImpuesto);

        public DataTable ObtenerEmpleados() => _repositorio.ObtenerEmpleados();

        public void Guardar(Requisicion requisicion)
        {
            if (requisicion.Guardada)
                throw new InvalidOperationException("La requisición ya fue guardada.");

            if (!requisicion.PuedeModificar)
                throw new InvalidOperationException("No se puede modificar una requisición en este estado.");

            if (requisicion.Detalles.Count == 0)
                throw new InvalidOperationException("La requisición debe tener al menos un artículo.");

            if (_repositorio.Existe(requisicion.Id))
                throw new InvalidOperationException("Ya existe una requisición con ese código.");

            IEnumerable<LineaDetalle> lineas = requisicion.Detalles.Select(d =>
                new LineaDetalle(d.Descripcion, d.Precio, d.Cantidad));

            _repositorio.Guardar(
                requisicion.Id,
                requisicion.Fecha,
                requisicion.Id_Empleado,
                requisicion.Estado,
                requisicion.Subtotal,   
                requisicion.Impuesto,   
                requisicion.Total,       
        lineas);

            requisicion.Guardada = true;
        }

        public void EnviarRevision(Requisicion requisicion) =>
            Transicionar(requisicion, r => r.EnviarRevision());

        public void Aprobar(Requisicion requisicion) =>
            Transicionar(requisicion, r => r.Aprobar());

        public void Rechazar(Requisicion requisicion) =>
            Transicionar(requisicion, r => r.Rechazar());

        public void Cancelar(Requisicion requisicion) =>
            Transicionar(requisicion, r => r.Cancelar());

        // Valida la transición en memoria, la guarda con el procedimiento y,
        // si la BD falla, devuelve la requisición a su estado anterior.
        private void Transicionar(Requisicion requisicion, Action<Requisicion> transicion)
        {
            if (!requisicion.Guardada)
                throw new InvalidOperationException(
                    "Guarda la requisición antes de cambiar su estado.");

            IEstadoRequisicion anterior = requisicion.EstadoActual;

            transicion(requisicion);

            try
            {
                _repositorio.CambiarEstado(requisicion.Id, requisicion.Estado);
            }
            catch
            {
                requisicion.EstadoActual = anterior;
                throw;
            }
        }

        // Regla de negocio: el reporte final solo existe para requisiciones aprobadas.
        public DatosReporte ObtenerDatosReporte(Requisicion requisicion)
        {
            if (requisicion.Estado != EstadosRequisicion.Aprobada)
                throw new InvalidOperationException(
                    "Solo se puede generar el reporte de una requisición aprobada.");

            if (!requisicion.Guardada)
                throw new InvalidOperationException("La requisición no ha sido guardada.");

            DataTable encabezado = _repositorio.ObtenerEncabezadoReporte(requisicion.Id);

            if (encabezado.Rows.Count == 0)
                throw new InvalidOperationException(
                    "No se encontraron datos de esta requisición en VistaReportes.");

            DataTable detalle = _repositorio.ObtenerDetalleReporte(requisicion.Id);

            return new DatosReporte(encabezado, detalle);
        }
    }
}
