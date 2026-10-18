namespace NegocioRequisiciones
{
    public readonly record struct Montos(decimal Subtotal, decimal Impuesto, decimal Total);

    /// <summary>
    /// Patrón Builder: única vía para crear una Requisicion. Valida los ítems
    /// (cantidad y precio > 0, sin duplicados) y calcula los montos con la
    /// política de impuestos recibida (Strategy).
    /// </summary>
    public class RequisicionBuilder
    {
        private readonly IPoliticaImpuesto _politicaImpuesto;
        private readonly List<DetalleRequisicion> _items = new List<DetalleRequisicion>();

        private int _id;
        private int _idEmpleado;
        private DateTime _fecha = DateTime.Now;

        public RequisicionBuilder(IPoliticaImpuesto politicaImpuesto)
        {
            _politicaImpuesto = politicaImpuesto
                ?? throw new ArgumentNullException(nameof(politicaImpuesto));
        }

        public IReadOnlyList<DetalleRequisicion> Items => _items;

        public RequisicionBuilder ConId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El código de la requisición no es válido.");
            _id = id;
            return this;
        }

        public RequisicionBuilder ConEmpleado(int idEmpleado)
        {
            if (idEmpleado <= 0)
                throw new ArgumentException("Empleado no válido.");
            _idEmpleado = idEmpleado;
            return this;
        }

        public RequisicionBuilder ConFecha(DateTime fecha)
        {
            _fecha = fecha;
            return this;
        }

        public RequisicionBuilder AgregarItem(string descripcion, int cantidad, decimal precio)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción es obligatoria.");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero.");

            if (precio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");

            string limpia = descripcion.Trim();

            bool duplicado = _items.Any(item =>
                string.Equals(item.Descripcion, limpia, StringComparison.OrdinalIgnoreCase));

            if (duplicado)
                throw new ArgumentException("El artículo ya existe en la requisición.");

            _items.Add(new DetalleRequisicion
            {
                Descripcion = limpia,
                Cantidad = cantidad,
                Precio = precio
            });

            return this;
        }

        public RequisicionBuilder QuitarItem(string descripcion)
        {
            string limpia = (descripcion ?? "").Trim();

            DetalleRequisicion? item = _items.FirstOrDefault(i =>
                string.Equals(i.Descripcion, limpia, StringComparison.OrdinalIgnoreCase));

            if (item != null)
                _items.Remove(item);

            return this;
        }

        // Cálculo automático; la interfaz solo lo muestra, nunca lo edita.
        public Montos CalcularMontos()
        {
            decimal subtotal = Math.Round(_items.Sum(i => i.Subtotal), 2);
            decimal impuesto = Math.Round(_politicaImpuesto.CalcularImpuesto(subtotal), 2);
            decimal total = Math.Round(subtotal + impuesto, 2);

            return new Montos(subtotal, impuesto, total);
        }

        public Requisicion Construir()
        {
            if (_id <= 0)
                throw new InvalidOperationException("Falta el código de la requisición.");

            if (_idEmpleado <= 0)
                throw new InvalidOperationException("Falta el empleado solicitante.");

            if (_items.Count == 0)
                throw new InvalidOperationException("La requisición debe tener al menos un artículo.");

            Requisicion requisicion = new Requisicion
            {
                Id = _id,
                Id_Empleado = _idEmpleado,
                Fecha = _fecha
            };

            foreach (DetalleRequisicion item in _items)
            {
                requisicion.AgregarDetalle(new DetalleRequisicion
                {
                    Id_Requisicion = _id,
                    Descripcion = item.Descripcion,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio
                });
            }

            Montos m = CalcularMontos();
            requisicion.AsignarMontos(m.Subtotal, m.Impuesto, m.Total);

            return requisicion;
        }
    }
}
