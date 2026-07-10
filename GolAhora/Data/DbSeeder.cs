using GolAhora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GolAhora.Data
{
    public static class DbSeeder
    {
        private const string Password = "123456";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            await context.Database.MigrateAsync();
            await EnsureRoles(roleManager);

            var admin = await EnsureUser(userManager, "admin@golahora.com", "franco.admin", "Franco", "Diaz", "30111222", "1123456789", "Admin");
            var employee = await EnsureUser(userManager, "empleado@golahora.com", "carla.emp", "Carla", "Gomez", "30222333", "1187654321", "PersonalClubProfile", "Employee");
            var professor = await EnsureUser(userManager, "profe@golahora.com", "rodrigo.profe", "Rodrigo", "Perez", "28333444", "1122221111", "PersonalClubProfile", "Professor");
            var client1 = await EnsureUser(userManager, "cliente@golahora.com", "lucia.cliente", "Lucia", "Martinez", "33788901", "1199998888", "Client");
            var client2 = await EnsureUser(userManager, "tomas.herrera@example.com", "tomas.herrera", "Tomas", "Herrera", "28455903", "1155223344", "Client");
            var client3 = await EnsureUser(userManager, "camila.torres@example.com", "camila.torres", "Camila", "Torres", "38999111", "1161234567", "Client");
            var client4 = await EnsureUser(userManager, "diego.vega@example.com", "diego.vega", "Diego", "Vega", "29500782", "1153384477", "Client");

            var adminProfile = await EnsureAdminProfile(context, admin);
            var employeeProfile = await EnsureEmployeeProfile(context, employee);
            var professorProfile = await EnsureProfessorProfile(context, professor);
            var clientProfile1 = await EnsureClientProfile(context, client1, 1);
            var clientProfile2 = await EnsureClientProfile(context, client2, 2);
            var clientProfile3 = await EnsureClientProfile(context, client3, 3);
            var clientProfile4 = await EnsureClientProfile(context, client4, 4);

            await EnsureCertification(context, professorProfile);
            var courtTypes = await EnsureCourtTypes(context);
            var courts = await EnsureCourts(context, courtTypes);
            await EnsureDisponibilities(context, courts);
            var discount = await EnsureDiscount(context);
            var payments = await EnsurePayments(context, clientProfile1, clientProfile2, discount);
            await EnsureReservations(context, clientProfile1, clientProfile2, courts, payments);
            await EnsureReceipts(context, payments);
            await EnsureClasses(context, professorProfile, courts[0], clientProfile1, clientProfile2);
            var teams = await EnsureTeams(context, clientProfile1, clientProfile2, clientProfile3, clientProfile4);
            var competence = await EnsureCompetence(context, teams, payments[0]);
            await EnsureMatches(context, competence, courts[0], teams);

            _ = adminProfile;
            _ = employeeProfile;
        }

        private static async Task EnsureRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in new[] { "Client", "PersonalClubProfile", "PersonalClub", "Admin", "Employee", "Professor" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }

        private static async Task<User> EnsureUser(UserManager<User> userManager, string email, string userName, string name, string lastName, string dni, string phone, params string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email) ?? await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = userName,
                    name = name,
                    lastName = lastName,
                    DNI = dni,
                    PhoneNumber = phone,
                    isActive = true,
                    registerDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            return user;
        }

        private static async Task<AdminProfile> EnsureAdminProfile(AppContext context, User user)
        {
            var personal = await EnsurePersonalClubProfile(context, user, "ADM-001", "Manana");
            var admin = await context.AdminProfiles.FirstOrDefaultAsync(a => a.idPersonalClub == personal.idPersonalClub);
            if (admin != null) return admin;

            admin = new AdminProfile { idPersonalClub = personal.idPersonalClub, accessLevel = 10 };
            context.AdminProfiles.Add(admin);
            await context.SaveChangesAsync();
            return admin;
        }

        private static async Task<EmployeeProfile> EnsureEmployeeProfile(AppContext context, User user)
        {
            var personal = await EnsurePersonalClubProfile(context, user, "EMP-014", "Tarde");
            var employee = await context.EmployeeProfiles.FirstOrDefaultAsync(e => e.idPersonalClub == personal.idPersonalClub);
            if (employee != null) return employee;

            employee = new EmployeeProfile { idPersonalClub = personal.idPersonalClub, sector = "Recepcion" };
            context.EmployeeProfiles.Add(employee);
            await context.SaveChangesAsync();
            return employee;
        }

        private static async Task<ProfessorProfile> EnsureProfessorProfile(AppContext context, User user)
        {
            var personal = await EnsurePersonalClubProfile(context, user, "PROF-021", "Manana");
            var professor = await context.ProfessorProfiles.FirstOrDefaultAsync(p => p.idPersonalClub == personal.idPersonalClub);
            if (professor != null) return professor;

            professor = new ProfessorProfile
            {
                idPersonalClub = personal.idPersonalClub,
                speciality = "Entrenamiento ofensivo",
                certification = "Licencia AFA Nivel 1"
            };
            context.ProfessorProfiles.Add(professor);
            await context.SaveChangesAsync();
            return professor;
        }

        private static async Task<PersonalClubProfile> EnsurePersonalClubProfile(AppContext context, User user, string legajo, string turno)
        {
            var personal = await context.PersonalClubProfiles.FirstOrDefaultAsync(p => p.idUser == user.Id);
            if (personal != null) return personal;

            personal = new PersonalClubProfile
            {
                idUser = user.Id,
                legajo = legajo,
                startDate = DateTime.UtcNow.AddMonths(-8),
                turno = turno
            };
            context.PersonalClubProfiles.Add(personal);
            await context.SaveChangesAsync();
            return personal;
        }

        private static async Task<ClientProfile> EnsureClientProfile(AppContext context, User user, int numberPartner)
        {
            var client = await context.ClientProfiles.FirstOrDefaultAsync(c => c.idUser == user.Id);
            if (client != null) return client;

            client = new ClientProfile { idUser = user.Id, numberPartner = numberPartner };
            context.ClientProfiles.Add(client);
            await context.SaveChangesAsync();
            return client;
        }

        private static async Task EnsureCertification(AppContext context, ProfessorProfile professor)
        {
            if (await context.Certifications.AnyAsync(c => c.professorId == professor.idProfessor)) return;

            context.Certifications.Add(new Certification
            {
                professorId = professor.idProfessor,
                name = "Preparacion fisica aplicada al futbol",
                institution = "AFA Campus",
                dateObtained = DateTime.UtcNow.AddYears(-2),
                numberCertificate = "AFA-2024-001",
                verified = true,
                verifiedBy = "Franco Diaz"
            });
            await context.SaveChangesAsync();
        }

        private static async Task<List<CourtType>> EnsureCourtTypes(AppContext context)
        {
            var seed = new[]
            {
                new CourtType { name = "Futbol 5", superficie = 600, capacity = 10, pricePerHour = 15000, description = "Cancha reducida de cesped sintetico." },
                new CourtType { name = "Futbol 7", superficie = 1100, capacity = 14, pricePerHour = 22000, description = "Cancha intermedia de cesped sintetico." },
                new CourtType { name = "Futbol 11", superficie = 7140, capacity = 22, pricePerHour = 38000, description = "Cancha reglamentaria de cesped natural." }
            };

            foreach (var item in seed)
            {
                if (!await context.CourtTypes.AnyAsync(t => t.name == item.name))
                {
                    context.CourtTypes.Add(item);
                }
            }
            await context.SaveChangesAsync();
            return await context.CourtTypes.Where(t => seed.Select(s => s.name).Contains(t.name)).OrderBy(t => t.idTypeCourt).ToListAsync();
        }

        private static async Task<List<Court>> EnsureCourts(AppContext context, List<CourtType> courtTypes)
        {
            foreach (var type in courtTypes)
            {
                var name = $"Cancha {type.name}";
                if (!await context.Courts.AnyAsync(c => c.name == name))
                {
                    context.Courts.Add(new Court
                    {
                        name = name,
                        description = $"Cancha demo para {type.name}.",
                        imageUrl = "",
                        isAvailable = true,
                        courtTypeId = type.idTypeCourt
                    });
                }
            }
            await context.SaveChangesAsync();
            return await context.Courts.Include(c => c.courtType).Where(c => c.name.StartsWith("Cancha Futbol")).OrderBy(c => c.idCourt).ToListAsync();
        }

        private static async Task EnsureDisponibilities(AppContext context, List<Court> courts)
        {
            foreach (var court in courts)
            {
                foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
                {
                    if (!await context.Disponibilities.AnyAsync(d => d.courtId == court.idCourt && d.day == day))
                    {
                        context.Disponibilities.Add(new Disponibility
                        {
                            courtId = court.idCourt,
                            day = day,
                            startTime = new TimeSpan(9, 0, 0),
                            endTime = new TimeSpan(23, 0, 0),
                            isAvailable = true
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task<Discounts> EnsureDiscount(AppContext context)
        {
            var discount = await context.Discounts.FirstOrDefaultAsync(d => d.discountType == "GOL10");
            if (discount != null) return discount;

            discount = new Discounts
            {
                nombre = "Promo bienvenida",
                discountType = "GOL10",
                discountValue = 10,
                conditions = "Descuento inicial para reservas y cobros.",
                startDate = DateTime.UtcNow.AddDays(-30),
                endDate = DateTime.UtcNow.AddYears(1)
            };
            context.Discounts.Add(discount);
            await context.SaveChangesAsync();
            return discount;
        }

        private static async Task<List<Payments>> EnsurePayments(AppContext context, ClientProfile client1, ClientProfile client2, Discounts discount)
        {
            if (!await context.Payments.AnyAsync(p => p.paymentMethod == "MercadoPago Seed"))
            {
                context.Payments.Add(new Payments
                {
                    idClient = client1.idClient,
                    amount = 15000,
                    paymentDate = DateTime.UtcNow.AddDays(-2),
                    paymentMethod = "MercadoPago Seed",
                    isSuccessful = true,
                    idDiscount = discount.idDiscount
                });
            }

            if (!await context.Payments.AnyAsync(p => p.paymentMethod == "Efectivo Seed"))
            {
                context.Payments.Add(new Payments
                {
                    idClient = client2.idClient,
                    amount = 22000,
                    paymentDate = DateTime.UtcNow.AddDays(-1),
                    paymentMethod = "Efectivo Seed",
                    isSuccessful = false
                });
            }

            await context.SaveChangesAsync();
            return await context.Payments.Where(p => p.paymentMethod.EndsWith("Seed")).OrderBy(p => p.idPayment).ToListAsync();
        }

        private static async Task EnsureReservations(AppContext context, ClientProfile client1, ClientProfile client2, List<Court> courts, List<Payments> payments)
        {
            if (courts.Count == 0 || payments.Count < 2) return;

            if (!await context.Reservations.AnyAsync(r => r.idPayment == payments[0].idPayment))
            {
                context.Reservations.Add(new Reservation
                {
                    idClient = client1.idClient,
                    idCourt = courts[0].idCourt,
                    reservationDate = DateTime.Today.AddDays(2),
                    startTime = new TimeSpan(18, 0, 0),
                    endTime = new TimeSpan(19, 0, 0),
                    status = "confirmada",
                    createdAt = DateTime.UtcNow.AddDays(-2),
                    isPaid = true,
                    totalPrice = payments[0].amount,
                    idPayment = payments[0].idPayment
                });
            }

            if (!await context.Reservations.AnyAsync(r => r.idPayment == payments[1].idPayment))
            {
                context.Reservations.Add(new Reservation
                {
                    idClient = client2.idClient,
                    idCourt = courts[Math.Min(1, courts.Count - 1)].idCourt,
                    reservationDate = DateTime.Today.AddDays(3),
                    startTime = new TimeSpan(20, 0, 0),
                    endTime = new TimeSpan(21, 30, 0),
                    status = "pendiente",
                    createdAt = DateTime.UtcNow.AddDays(-1),
                    isPaid = false,
                    totalPrice = payments[1].amount,
                    idPayment = payments[1].idPayment
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureReceipts(AppContext context, List<Payments> payments)
        {
            var paid = payments.FirstOrDefault(p => p.isSuccessful);
            if (paid == null) return;
            if (await context.Receipts.AnyAsync(r => r.idPayment == paid.idPayment)) return;

            context.Receipts.Add(new Receipts
            {
                idPayment = paid.idPayment,
                date = paid.paymentDate,
                receiptNumber = $"0001-{paid.idPayment:00000000}",
                totalAmount = paid.amount,
                details = "Recibo seed por reserva confirmada."
            });
            await context.SaveChangesAsync();
        }

        private static async Task EnsureClasses(AppContext context, ProfessorProfile professor, Court court, ClientProfile client1, ClientProfile client2)
        {
            var clas = await context.Classes.Include(c => c.clients).FirstOrDefaultAsync(c => c.name == "Entrenamiento Intensivo Seed");
            if (clas == null)
            {
                clas = new Class
                {
                    name = "Entrenamiento Intensivo Seed",
                    description = "Clase de entrenamiento fisico y tecnico.",
                    classType = "Entrenamiento",
                    profesorId = professor.idProfessor,
                    courtId = court.idCourt,
                    date = DateTime.Today.AddDays(4).AddHours(18),
                    capacityMax = 10,
                    duration = 60,
                    price = 5000,
                    isActive = true
                };
                context.Classes.Add(clas);
                await context.SaveChangesAsync();
            }

            if (!clas.clients.Any(c => c.idClient == client1.idClient)) clas.clients.Add(client1);
            if (!clas.clients.Any(c => c.idClient == client2.idClient)) clas.clients.Add(client2);
            await context.SaveChangesAsync();

            if (!await context.Assistances.AnyAsync(a => a.classId == clas.idClass))
            {
                context.Assistances.AddRange(
                    new Assistance { classId = clas.idClass, clientId = client1.idClient, date = DateTime.UtcNow, isAssisted = true, observations = "Presente" },
                    new Assistance { classId = clas.idClass, clientId = client2.idClient, date = DateTime.UtcNow, isAssisted = false, observations = "Ausente con aviso" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<List<Team>> EnsureTeams(AppContext context, params ClientProfile[] clients)
        {
            var names = new[] { "Los Halcones FC", "Barrio Norte", "Las Torres", "Vega United" };
            for (var i = 0; i < names.Length && i < clients.Length; i++)
            {
                if (!await context.Teams.AnyAsync(t => t.name == names[i]))
                {
                    var team = new Team { name = names[i], clientId = clients[i].idClient };
                    team.players.Add(clients[i]);
                    context.Teams.Add(team);
                }
            }
            await context.SaveChangesAsync();
            return await context.Teams.Where(t => names.Contains(t.name)).OrderBy(t => t.idTeam).ToListAsync();
        }

        private static async Task<Competence> EnsureCompetence(AppContext context, List<Team> teams, Payments payment)
        {
            var competence = await context.Competences.Include(c => c.teams).FirstOrDefaultAsync(c => c.name == "Liga Apertura Seed");
            if (competence == null)
            {
                competence = new League
                {
                    name = "Liga Apertura Seed",
                    description = "Competencia precargada para pruebas.",
                    startDate = DateTime.Today.AddDays(5),
                    endDate = DateTime.Today.AddDays(20),
                    isActive = true,
                    regulations = "Todos contra todos.",
                    capacityTeams = 8
                };
                context.Competences.Add(competence);
                await context.SaveChangesAsync();
            }

            foreach (var team in teams.Take(4))
            {
                if (!await context.CompetenceTeams.AnyAsync(ct => ct.idCompetence == competence.idCompetence && ct.idTeam == team.idTeam))
                {
                    context.CompetenceTeams.Add(new CompetenceTeam
                    {
                        idCompetence = competence.idCompetence,
                        idTeam = team.idTeam,
                        inscription = DateTime.UtcNow,
                        status = true,
                        idPayment = payment.idPayment
                    });
                }
            }
            await context.SaveChangesAsync();
            return competence;
        }

        private static async Task EnsureMatches(AppContext context, Competence competence, Court court, List<Team> teams)
        {
            if (teams.Count < 2 || await context.Matches.AnyAsync(m => m.idCompetence == competence.idCompetence)) return;

            var match = new Match
            {
                idCompetence = competence.idCompetence,
                round = 1,
                idTeamA = teams[0].idTeam,
                idTeamB = teams[1].idTeam,
                idCourt = court.idCourt,
                date = DateTime.Today.AddDays(6).AddHours(20),
                isPlayed = true
            };
            context.Matches.Add(match);
            await context.SaveChangesAsync();

            match.idResults = match.idMatch;
            context.Results.Add(new Result
            {
                idResults = match.idMatch,
                idMatch = match.idMatch,
                scoreTeamLocal = 3,
                scoreTeamVisitor = 2
            });
            await context.SaveChangesAsync();
        }
    }
}
