namespace NegocioRequisiciones
{
    // Identificadores de estado (deben coincidir con la tabla de estados de la BD).
    public static class EstadosRequisicion
    {
        public const int Creada = 1;
        public const int EnRevision = 2;
        public const int Aprobada = 3;
        public const int Rechazada = 4;
        public const int Cancelada = 5;
    }
}
