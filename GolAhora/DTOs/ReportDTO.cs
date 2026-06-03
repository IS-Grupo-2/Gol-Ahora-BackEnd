namespace GolAhora.DTOs
{
    public class ReporteDTO
    {
        public int id { get; set; }
        public string titulo { get; set; } = null!;
        public DateTime fechaGeneracion { get; set; }
        public string generadoPor { get; set; } = null!;
        public DateTime periodoDesde { get; set; }
        public DateTime periodoHasta { get; set; }
    }

    public class ReporteIngresoDTO : ReporteDTO
    {
        public double totalIngresos { get; set; }
        public Dictionary<string, double> ingresosPorConcepto { get; set; } = new();
        public List<CobrosDTO> cobros { get; set; } = new();
    }

    public class ReporteAsistenciaDTO : ReporteDTO
    {
        public int totalAsistencias { get; set; }
        public Dictionary<string, int> asistenciasPorClase { get; set; } = new();
        public List<AsistenciaDTO> asistencias { get; set; } = new();
    }

    public class ReporteReservaDTO : ReporteDTO
    {
        public int totalReservas { get; set; }
        public Dictionary<string, int> reservasPorCanchas { get; set; } = new();
        public Dictionary<string, int> reservasPorEstado { get; set; } = new();
        public List<ReservaResumenDTO> reservas { get; set; } = new();
    }

    public class ReporteRequestDTO
    {
        public DateTime periodoDesde { get; set; }
        public DateTime periodoHasta { get; set; }
    }

    public class ReservaResumenDTO
    {
        public int id { get; set; }
        public string clienteNombre { get; set; } = null!;
        public string canchaNombre { get; set; } = null!;
        public DateTime fechaReserva { get; set; }
        public TimeSpan horaInicio { get; set; }
        public TimeSpan horaFin { get; set; }
        public bool pagado { get; set; }
        public double precioTotal { get; set; }
    }

    public class AsistenciaDTO
    {
        public int id { get; set; }
        public string clienteNombre { get; set; } = null!;
        public string claseNombre { get; set; } = null!;
        public bool presente { get; set; }
        public string observaciones { get; set; } = null!;
    }

    public class CobrosDTO
    {
        public int idCobro { get; set; }
        public string clienteNombre { get; set; } = null!;
        public double monto { get; set; }
        public DateTime fechaPago { get; set; }
        public string metodoPago { get; set; } = null!;
        public bool exitoso { get; set; }
    }
}