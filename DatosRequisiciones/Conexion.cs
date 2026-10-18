using Microsoft.Data.SqlClient;
using System.Data;

namespace DatosRequisiciones
{
    public sealed class Conexion
    {
        private static readonly Lazy<Conexion> _instancia =
            new Lazy<Conexion>(() => new Conexion());

        public static Conexion Instancia => _instancia.Value;

        private readonly string _cadenaConexion =
            "Server=ANGELH;" +
            "Database=Gestion_Requisiciones;" +
            "User ID=angel;" +
            "Password=Diciembre2006;" +
            "TrustServerCertificate=True;" +
            "Encrypt=True;";

        public Conexion() { }

        // Consultas de solo lectura (SELECT sobre tablas o vistas), con parámetros.
        public DataTable EjecutarConsulta(string consulta, params SqlParameter[] parametros)
        {
            DataTable resultado = new DataTable();

            try
            {
                using (SqlConnection cn = new SqlConnection(_cadenaConexion))
                using (SqlCommand cmd = new SqlCommand(consulta, cn))
                {
                    cmd.Parameters.AddRange(parametros);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultado);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar la consulta: " + ex.Message, ex);
            }

            return resultado;
        }

        // Toda escritura pasa por un procedimiento almacenado.
        public bool EjecutarProceso(string procedimiento, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    EjecutarProcedimiento(cn, null, procedimiento, parametros);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar el procedimiento: " + ex.Message, ex);
            }
        }

        // Varias operaciones como una sola unidad: o se guarda todo o no se guarda nada.
        public void EjecutarEnTransaccion(Action<SqlConnection, SqlTransaction> operacion)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    using (SqlTransaction tx = cn.BeginTransaction())
                    {
                        try
                        {
                            operacion(cn, tx);
                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los datos: " + ex.Message, ex);
            }
        }

        internal static void EjecutarProcedimiento(
            SqlConnection cn,
            SqlTransaction? tx,
            string procedimiento,
            params SqlParameter[] parametros)
        {
            using (SqlCommand cmd = new SqlCommand(procedimiento, cn, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
