namespace NegocioRequisiciones
{
    // Patrón State: cada estado decide qué transiciones son válidas.
    public interface IEstadoRequisicion
    {
        int Id { get; }
        string Nombre { get; }
        bool PuedeModificar { get; }
        IEstadoRequisicion EnviarRevision();
        IEstadoRequisicion Aprobar();
        IEstadoRequisicion Rechazar();
        IEstadoRequisicion Cancelar();
    }

    public class EstadoCreada : IEstadoRequisicion
    {
        public int Id => EstadosRequisicion.Creada;
        public string Nombre => "Creada";
        public bool PuedeModificar => true;

        public IEstadoRequisicion EnviarRevision() => new EstadoEnRevision();

        public IEstadoRequisicion Aprobar() =>
            throw new InvalidOperationException("La requisición debe estar en revisión para aprobarse.");

        public IEstadoRequisicion Rechazar() =>
            throw new InvalidOperationException("La requisición debe estar en revisión para rechazarse.");

        public IEstadoRequisicion Cancelar() => new EstadoCancelada();
    }

    public class EstadoEnRevision : IEstadoRequisicion
    {
        public int Id => EstadosRequisicion.EnRevision;
        public string Nombre => "En Revisión";

        // El enunciado solo prohíbe modificar en Aprobada, Rechazada o Cancelada.
        public bool PuedeModificar => true;

        public IEstadoRequisicion EnviarRevision() =>
            throw new InvalidOperationException("La requisición ya está en revisión.");

        public IEstadoRequisicion Aprobar() => new EstadoAprobada();
        public IEstadoRequisicion Rechazar() => new EstadoRechazada();
        public IEstadoRequisicion Cancelar() => new EstadoCancelada();
    }

    public class EstadoAprobada : IEstadoRequisicion
    {
        public int Id => EstadosRequisicion.Aprobada;
        public string Nombre => "Aprobada";
        public bool PuedeModificar => false;

        public IEstadoRequisicion EnviarRevision() => throw Error();
        public IEstadoRequisicion Aprobar() => throw Error();
        public IEstadoRequisicion Rechazar() => throw Error();
        public IEstadoRequisicion Cancelar() => throw Error();

        private static InvalidOperationException Error() =>
            new InvalidOperationException("La requisición ya fue aprobada y no admite cambios.");
    }

    public class EstadoRechazada : IEstadoRequisicion
    {
        public int Id => EstadosRequisicion.Rechazada;
        public string Nombre => "Rechazada";
        public bool PuedeModificar => false;

        public IEstadoRequisicion EnviarRevision() => throw Error();
        public IEstadoRequisicion Aprobar() => throw Error();
        public IEstadoRequisicion Rechazar() => throw Error();
        public IEstadoRequisicion Cancelar() => throw Error();

        private static InvalidOperationException Error() =>
            new InvalidOperationException("La requisición fue rechazada y no admite cambios.");
    }

    public class EstadoCancelada : IEstadoRequisicion
    {
        public int Id => EstadosRequisicion.Cancelada;
        public string Nombre => "Cancelada";
        public bool PuedeModificar => false;

        public IEstadoRequisicion EnviarRevision() => throw Error();
        public IEstadoRequisicion Aprobar() => throw Error();
        public IEstadoRequisicion Rechazar() => throw Error();
        public IEstadoRequisicion Cancelar() => throw Error();

        private static InvalidOperationException Error() =>
            new InvalidOperationException("La requisición fue cancelada y no admite cambios.");
    }
}
