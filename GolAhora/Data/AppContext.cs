using GolAhora.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace GolAhora.Data
{
    public class AppContext : DbContext
    {
        public DbSet<User> USers => Set<User>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<PersonalClub> PersonalClubs => Set<PersonalClub>();
        public DbSet<Professor> Professors => Set<Professor>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Class> Classes => Set<Class>();
        public DbSet<Assistance> Assistances => Set<Assistance>();
        public DbSet<Certification> Certifications => Set<Certification>();
        public DbSet<Court> Courts => Set<Court>();
        public DbSet<CourtType> CourtTypes => Set<CourtType>(); 
        public DbSet<Disponibility> Disponibilities => Set<Disponibility>();
        public DbSet<Competence> Competences => Set<Competence>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<Tournament> Tournaments => Set<Tournament>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<CompetenceTeam> CompetenceTeams => Set<CompetenceTeam>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Payments> Payments => Set<Payments>();
        public DbSet<Receipts> Receipts => Set<Receipts>();
        public DbSet<Discounts> Discounts => Set<Discounts>();
        public DbSet<Reports> Reports => Set<Reports>();

        public AppContext(DbContextOptions<AppContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<PersonalClub>().ToTable("PersonalClubs");
            modelBuilder.Entity<Professor>().ToTable("Professors");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Employee>().ToTable("Employees");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<Competence>().ToTable("Competences");
            modelBuilder.Entity<League>().ToTable("Leagues");
            modelBuilder.Entity<Tournament>().ToTable("Tournaments");

            modelBuilder.Entity<User>().HasKey(u => u.id);
            modelBuilder.Entity<Assistance>().HasKey(a => a.idAssistance);
            modelBuilder.Entity<Certification>().HasKey(c => c.idCertification);
            modelBuilder.Entity<Class>().HasKey(c => c.idClass);
            modelBuilder.Entity<Court>().HasKey(c => c.idCourt);
            modelBuilder.Entity<CourtType>().HasKey(ct => ct.idTypeCourt);
            modelBuilder.Entity<Disponibility>().HasKey(d => d.idDisponibility);
            modelBuilder.Entity<Competence>().HasKey(c => c.idCompetence);
            modelBuilder.Entity<Team>().HasKey(t => t.idTeam);
            modelBuilder.Entity<CompetenceTeam>().HasKey(ct => new { ct.idCompetence, ct.idTeam });
            modelBuilder.Entity<Match>().HasKey(m => m.idMatch);
            modelBuilder.Entity<Result>().HasKey(r => r.idResults);
            modelBuilder.Entity<Reservation>().HasKey(r => r.idReservation);
            modelBuilder.Entity<Payments>().HasKey(p => p.idPayment);
            modelBuilder.Entity<Receipts>().HasKey(r => r.idReceipt);
            modelBuilder.Entity<Discounts>().HasKey(d => d.idDiscount);
            modelBuilder.Entity<Reports>().HasKey(r => r.idReport);


            // Profesor 1 - N Certificaciones
            modelBuilder.Entity<Certification>()
                .HasOne(c => c.professor)
                .WithMany(p => p.certifications)
                .HasForeignKey(c => c.professorId)
                // La siguiente linea impide que se borre un profesor si tiene certificaciones asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Profesor 1 - N Clases
            modelBuilder.Entity<Class>()
                .HasOne(c => c.profesor)
                .WithMany(p => p.classes)
                .HasForeignKey(c => c.profesorId)
                // La sisguente linea impide que se borre un profesor si tiene clases asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente 1 - N Asistencias
            modelBuilder.Entity<Assistance>()
                .HasOne(a => a.client)
                .WithMany(c => c.assistances)
                .HasForeignKey(a => a.clientId)
                // La siguiente linea impide que se borre un cliente si tiene asistencias asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Clase 1 - N Asistencias
            modelBuilder.Entity<Assistance>()
                .HasOne(a => a.clas)
                .WithMany(c => c.assistances)
                .HasForeignKey(a => a.classId)
                // La siguiente linea impide que se borre una clase si tiene asistencias asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cancha 1 - N Clases
            modelBuilder.Entity<Class>()
                .HasOne(c => c.court)
                .WithMany(co => co.classes)
                .HasForeignKey(c => c.courtId)
                // La siguiente linea impide que se borre una cancha si tiene clases asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cancha 1 - N Disponibilidades
            modelBuilder.Entity<Disponibility>()
                .HasOne(d => d.court)
                .WithMany(c => c.disponibilities)
                .HasForeignKey(d => d.courtId)
                // La siguiente linea impide que se borre una cancha si tiene disponibilidades asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Tipo de cancha 1 - N Canchas
            modelBuilder.Entity<Court>()
                .HasOne(c => c.courtType)
                .WithMany(ct => ct.courts)
                .HasForeignKey(c => c.courtTypeId)
                // La siguiente linea impide que se borre un tipo de cancha si tiene canchas asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cancha 1 - N Partidos
            modelBuilder.Entity<Match>()
                .HasOne(m => m.court)
                .WithMany(c => c.matches)
                .HasForeignKey(m => m.idCourt)
                // La siguiente linea impide que se borre una cancha si tiene partidos asignados
                .OnDelete(DeleteBehavior.Restrict);

            // partido 1 - 1 resultado
            modelBuilder.Entity<Result>()
                .HasOne(r => r.match) //LINEA MODIFICADA POR NAHUEL, ANTES ESTABA ASI: .HasOne(r => r.idgame)
                .WithOne(m => m.result)
                .HasForeignKey<Result>(r => r.idResults)
                // La siguiente linea impide que se borre un partido si tiene un resultado asignado
                .OnDelete(DeleteBehavior.Restrict);

            // Competencia 1 - N Partidos
            modelBuilder.Entity<Match>()
                .HasOne(m => m.competence)
                .WithMany(c => c.games)
                .HasForeignKey(m => m.idCompetence)
                // La siguiente linea impide que se borre una competencia si tiene partidos asignados
                .OnDelete(DeleteBehavior.Restrict);

            // Equipo 1 - N Partidos como local
            modelBuilder.Entity<Match>()
                .HasOne(m => m.teamA)
                .WithMany(t => t.localMatches)
                .HasForeignKey(m => m.idTeamA)
                // La siguiente linea impide que se borre un equipo si tiene partidos asignados como local
                .OnDelete(DeleteBehavior.Restrict);

            // Equipo 1 - N Partidos como visitante
            modelBuilder.Entity<Match>()
                .HasOne(m => m.teamB)
                .WithMany(t => t.visitorMatches)
                .HasForeignKey(m => m.idTeamB)
                // La siguiente linea impide que se borre un equipo si tiene partidos asignados como visitante
                .OnDelete(DeleteBehavior.Restrict);

            // Comptencia N - N Equipos
            modelBuilder.Entity<CompetenceTeam>()
                .HasKey(ct => new { ct.idCompetence, ct.idTeam });

            // Competencia 1 - N CompetenceTeam
            modelBuilder.Entity<CompetenceTeam>()
                .HasOne(ct => ct.competence)
                .WithMany(c => c.teams)
                .HasForeignKey(ct => ct.idCompetence)
                // La siguiente linea impide que se borre una competencia si tiene equipos asignados
                .OnDelete(DeleteBehavior.Restrict);

            // Equipo 1 - N CompetenceTeam
            modelBuilder.Entity<CompetenceTeam>()
                .HasOne(ct => ct.team)
                .WithMany(t => t.competences)
                .HasForeignKey(ct => ct.idTeam)
                // La siguiente linea impide que se borre un equipo si tiene competencias asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente 1 - 1 Equipos como capitán
            modelBuilder.Entity<Team>()
                .HasOne(t => t.captain)
                .WithMany(c => c.teamsCaptain)
                .HasForeignKey(t => t.clientId)
                // La siguiente linea impide que se borre un cliente si es capitán de un equipo
                .OnDelete(DeleteBehavior.Restrict);

            // Equipo 1 - N Clientes como jugadores
            modelBuilder.Entity<Team>()
                .HasMany(t => t.players)
                .WithOne(c => c.team)
                .HasForeignKey(c => c.idTeam)
                // La siguiente linea impide que se borre un equipo si tiene jugadores asignados
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente 1 - N Reservas
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.client)
                .WithMany(c => c.reservations)
                .HasForeignKey(r => r.idClient)
                // La siguiente linea impide que se borre un cliente si tiene reservas asignadas
                .OnDelete(DeleteBehavior.Restrict);

            // Cancha 1 - N Reservas
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.court)
                .WithMany(c => c.reservations)
                .HasForeignKey(r => r.idCourt)
                // La siguiente linea impide que se borre una cancha si tiene reservas asignadas
                .OnDelete(DeleteBehavior.Restrict);
            
            // Pago 1 - 1 Reservas
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.payment)
                .WithOne(p => p.reservation)
                .HasForeignKey<Reservation>(r => r.idPayment)
                // La siguiente linea impide que se borre una reserva si tiene un pago asignado
                .OnDelete(DeleteBehavior.Restrict);

            //  Pago 1 - N CompetenceTeam
            modelBuilder.Entity<CompetenceTeam>()
                .HasOne(c => c.payments)
                .WithMany(p => p.competenceTeams)
                .HasForeignKey(c => c.idPayment)
                // La siguiente linea impide que se borre un pago si tiene equipos asignados
                .OnDelete(DeleteBehavior.Restrict);

            // Admin 1 - N Reportes
            modelBuilder.Entity<Reports>()
                .HasOne(r => r.admin)
                .WithMany(a => a.reports)
                .HasForeignKey(r => r.idAdmin)
                // La siguiente linea impide que se borre un admin si tiene reportes asignados
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
