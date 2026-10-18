namespace NegocioRequisiciones
{
    public class DetalleRequisicion
    {
        public int Id_Detalle { get; internal set; }
        public int Id_Requisicion { get; internal set; }
        public string Descripcion { get; internal set; } = "";
        public decimal Precio { get; internal set; }
        public int Cantidad { get; internal set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
