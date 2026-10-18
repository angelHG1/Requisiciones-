namespace NegocioRequisiciones
{
    // Patrón Strategy: la política de impuestos se puede cambiar sin tocar la lógica de negocio.
    public interface IPoliticaImpuesto
    {
        decimal CalcularImpuesto(decimal subtotal);
    }

    public class Impuesto18 : IPoliticaImpuesto
    {
        public decimal CalcularImpuesto(decimal subtotal) => subtotal * 0.18m;
    }

    public class Impuesto0 : IPoliticaImpuesto
    {
        public decimal CalcularImpuesto(decimal subtotal) => 0m;
    }
}
