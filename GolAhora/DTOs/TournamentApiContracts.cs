namespace GolAhora.DTOs
{
    public class CompetenciaRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public int? MaxEquipos { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Regulations { get; set; }
        public double? PrecioInscripcion { get; set; }
    }

    public class InscripcionEquipoRequest
    {
        public int? EquipoId { get; set; }
        public int? IdEquipo { get; set; }
    }

    public class EquipoApiRequest
    {
        public string? Nombre { get; set; }
        public string? Name { get; set; }
        public int? ClientId { get; set; }
        public UsuarioEquipoRequest? CreadoPor { get; set; }
    }

    public class UsuarioEquipoRequest
    {
        public int? IdClient { get; set; }
        public int? IdCliente { get; set; }
        public int? IdUsuario { get; set; }
    }

    public class ResultadoPartidoRequest
    {
        public ResultadoMarcadorRequest? Resultado { get; set; }
        public int? Local { get; set; }
        public int? Visitante { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public int? ScoreTeamLocal { get; set; }
        public int? ScoreTeamVisitor { get; set; }
    }

    public class ResultadoMarcadorRequest
    {
        public int? Local { get; set; }
        public int? Visitante { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public int? ScoreTeamLocal { get; set; }
        public int? ScoreTeamVisitor { get; set; }
    }
}
