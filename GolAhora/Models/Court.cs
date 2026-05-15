namespace GolAhora.Models
{
    public class Court
    {
        public int idCourt { get; set; }
        public string name { get; set; } = string.Empty;
        public bool isAvailable { get; set; }
        public string description { get; set; } = null!;
        public string imageUrl { get; set; } = null!;

        public int courtTypeId { get; set; }
        public CourtType courtType { get; set; } = null!;

        public ICollection<Disponibility> disponibilities { get; set; } = new List<Disponibility>();
        public ICollection<Match> matches { get; set; } = new List<Match>();
        public ICollection<Reservation> reservations { get; set; } = new List<Reservation>();
        public ICollection<Class> classes { get; set; } = new List<Class>();
    }

    public class CourtType
    {
        public int idTypeCourt { get; set; }
        public string name { get; set; } = null!;
        public double superficie { get; set; }
        public int capacity { get; set; }
        public double pricePerHour { get; set; }
        public string description { get; set; } = null!;

        public ICollection<Court> courts { get; set; } = new List<Court>();
    }

    public class Disponibility
    {
        public int idDisponibility { get; set; }
        public DayOfWeek day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }

        public int courtId { get; set; }
        public Court court { get; set; } = null!;
    }
}
