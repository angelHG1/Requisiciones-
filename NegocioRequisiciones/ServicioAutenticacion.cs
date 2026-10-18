using DatosRequisiciones;

namespace NegocioRequisiciones
{
    public class ServicioAutenticacion
    {
        private readonly RepositorioUsuarios _repositorio = new RepositorioUsuarios();

        public bool ValidarCredenciales(string usuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
                return false;

            return _repositorio.ValidarCredenciales(usuario.Trim(), clave);
        }
    }
}
