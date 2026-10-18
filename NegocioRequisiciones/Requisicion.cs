namespace NegocioRequisiciones
{
    /// <summary>
    /// Contexto del patrón State. Solo se puede crear con RequisicionBuilder
    /// (constructor internal) y los montos no se pueden asignar desde fuera.
    /// </summary>
    public class Requisicion
    {
        private readonly List<DetalleRequisicion> _detalles = new List<DetalleRequisicion>();

        internal IEstadoRequisicion EstadoActual { get; set; } = new EstadoCreada();

        public int Id { get; internal set; }
        public int Id_Empleado { get; internal set; }
        public DateTime Fecha { get; internal set; }
        public bool Guardada { get; internal set; }

        public IReadOnlyList<DetalleRequisicion> Detalles => _detalles;

        public decimal Subtotal { get; private set; }
        public decimal Impuesto { get; private set; }
        public decimal Total { get; private set; }

        public int Estado => EstadoActual.Id;
        public string NombreEstado => EstadoActual.Nombre;
        public bool PuedeModificar => EstadoActual.PuedeModificar;

        internal Requisicion() { }

        internal void AgregarDetalle(DetalleRequisicion detalle)
        {
            if (!PuedeModificar)
                throw new InvalidOperationException(
                    "No se puede modificar una requisición " + NombreEstado.ToLower() + ".");

            _detalles.Add(detalle);
        }

        internal void AsignarMontos(decimal subtotal, decimal impuesto, decimal total)
        {
            Subtotal = subtotal;
            Impuesto = impuesto;
            Total = total;
        }

        // Transiciones: delegan en el estado actual, que decide si son válidas.
        public void EnviarRevision() => EstadoActual = EstadoActual.EnviarRevision();
        public void Aprobar() => EstadoActual = EstadoActual.Aprobar();
        public void Rechazar() => EstadoActual = EstadoActual.Rechazar();
        public void Cancelar() => EstadoActual = EstadoActual.Cancelar();
    }
}
