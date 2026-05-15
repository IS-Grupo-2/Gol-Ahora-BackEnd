using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolAhora.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competences",
                columns: table => new
                {
                    idCompetence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    regulations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    capacityTeams = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competences", x => x.idCompetence);
                });

            migrationBuilder.CreateTable(
                name: "CourtTypes",
                columns: table => new
                {
                    idTypeCourt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    superficie = table.Column<double>(type: "float", nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    pricePerHour = table.Column<double>(type: "float", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtTypes", x => x.idTypeCourt);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    registerDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    idCompetence = table.Column<int>(type: "int", nullable: false),
                    idLeague = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.idCompetence);
                    table.ForeignKey(
                        name: "FK_Leagues_Competences_idCompetence",
                        column: x => x.idCompetence,
                        principalTable: "Competences",
                        principalColumn: "idCompetence",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    idCompetence = table.Column<int>(type: "int", nullable: false),
                    idTournament = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.idCompetence);
                    table.ForeignKey(
                        name: "FK_Tournaments_Competences_idCompetence",
                        column: x => x.idCompetence,
                        principalTable: "Competences",
                        principalColumn: "idCompetence",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Courts",
                columns: table => new
                {
                    idCourt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isAvailable = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    courtTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courts", x => x.idCourt);
                    table.ForeignKey(
                        name: "FK_Courts_CourtTypes_courtTypeId",
                        column: x => x.courtTypeId,
                        principalTable: "CourtTypes",
                        principalColumn: "idTypeCourt",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonalClubs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    legajo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    turno = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalClubs", x => x.id);
                    table.ForeignKey(
                        name: "FK_PersonalClubs_Users_id",
                        column: x => x.id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disponibilities",
                columns: table => new
                {
                    idDisponibility = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    day = table.Column<int>(type: "int", nullable: false),
                    startTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    endTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    isAvailable = table.Column<bool>(type: "bit", nullable: false),
                    courtId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disponibilities", x => x.idDisponibility);
                    table.ForeignKey(
                        name: "FK_Disponibilities_Courts_courtId",
                        column: x => x.courtId,
                        principalTable: "Courts",
                        principalColumn: "idCourt",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    accessLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.id);
                    table.ForeignKey(
                        name: "FK_Admins_PersonalClubs_id",
                        column: x => x.id,
                        principalTable: "PersonalClubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    sector = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_Employees_PersonalClubs_id",
                        column: x => x.id,
                        principalTable: "PersonalClubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Professors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    specialty = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professors", x => x.id);
                    table.ForeignKey(
                        name: "FK_Professors_PersonalClubs_id",
                        column: x => x.id,
                        principalTable: "PersonalClubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    idReport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tittle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    generatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idAdmin = table.Column<int>(type: "int", nullable: false),
                    from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.idReport);
                    table.ForeignKey(
                        name: "FK_Reports_Admins_idAdmin",
                        column: x => x.idAdmin,
                        principalTable: "Admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    idCertification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    professorId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    institution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dateObtained = table.Column<DateTime>(type: "datetime2", nullable: false),
                    numberCertificate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    verified = table.Column<bool>(type: "bit", nullable: false),
                    verifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.idCertification);
                    table.ForeignKey(
                        name: "FK_Certifications_Professors_professorId",
                        column: x => x.professorId,
                        principalTable: "Professors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    idClass = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    classType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    profesorId = table.Column<int>(type: "int", nullable: false),
                    courtId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    capacityMax = table.Column<int>(type: "int", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<double>(type: "float", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.idClass);
                    table.ForeignKey(
                        name: "FK_Classes_Courts_courtId",
                        column: x => x.courtId,
                        principalTable: "Courts",
                        principalColumn: "idCourt",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Professors_profesorId",
                        column: x => x.profesorId,
                        principalTable: "Professors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assistances",
                columns: table => new
                {
                    idAssistance = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    classId = table.Column<int>(type: "int", nullable: false),
                    isAssisted = table.Column<bool>(type: "bit", nullable: false),
                    observations = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assistances", x => x.idAssistance);
                    table.ForeignKey(
                        name: "FK_Assistances_Classes_classId",
                        column: x => x.classId,
                        principalTable: "Classes",
                        principalColumn: "idClass",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    numberPartner = table.Column<int>(type: "int", nullable: false),
                    idTeam = table.Column<int>(type: "int", nullable: false),
                    ClassidClass = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_Clients_Classes_ClassidClass",
                        column: x => x.ClassidClass,
                        principalTable: "Classes",
                        principalColumn: "idClass");
                    table.ForeignKey(
                        name: "FK_Clients_Users_id",
                        column: x => x.id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    idTeam = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    clientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.idTeam);
                    table.ForeignKey(
                        name: "FK_Teams_Clients_clientId",
                        column: x => x.clientId,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    idMatch = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idCompetence = table.Column<int>(type: "int", nullable: false),
                    round = table.Column<int>(type: "int", nullable: false),
                    idTeamA = table.Column<int>(type: "int", nullable: false),
                    idTeamB = table.Column<int>(type: "int", nullable: false),
                    idCourt = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isPlayed = table.Column<bool>(type: "bit", nullable: false),
                    idResults = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.idMatch);
                    table.ForeignKey(
                        name: "FK_Matches_Competences_idCompetence",
                        column: x => x.idCompetence,
                        principalTable: "Competences",
                        principalColumn: "idCompetence",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Courts_idCourt",
                        column: x => x.idCourt,
                        principalTable: "Courts",
                        principalColumn: "idCourt",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_idTeamA",
                        column: x => x.idTeamA,
                        principalTable: "Teams",
                        principalColumn: "idTeam",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_idTeamB",
                        column: x => x.idTeamB,
                        principalTable: "Teams",
                        principalColumn: "idTeam",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    idResults = table.Column<int>(type: "int", nullable: false),
                    scoreTeamLocal = table.Column<int>(type: "int", nullable: false),
                    scoreTeamVisitor = table.Column<int>(type: "int", nullable: false),
                    penaltiesTeamLocal = table.Column<int>(type: "int", nullable: false),
                    penaltiesTeamVisitor = table.Column<int>(type: "int", nullable: false),
                    foulsTeamLocal = table.Column<int>(type: "int", nullable: false),
                    foulsTeamVisitor = table.Column<int>(type: "int", nullable: false),
                    observations = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.idResults);
                    table.ForeignKey(
                        name: "FK_Results_Matches_idResults",
                        column: x => x.idResults,
                        principalTable: "Matches",
                        principalColumn: "idMatch",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompetenceTeams",
                columns: table => new
                {
                    idCompetence = table.Column<int>(type: "int", nullable: false),
                    idTeam = table.Column<int>(type: "int", nullable: false),
                    inscription = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false),
                    idPayment = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenceTeams", x => new { x.idCompetence, x.idTeam });
                    table.ForeignKey(
                        name: "FK_CompetenceTeams_Competences_idCompetence",
                        column: x => x.idCompetence,
                        principalTable: "Competences",
                        principalColumn: "idCompetence",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetenceTeams_Teams_idTeam",
                        column: x => x.idTeam,
                        principalTable: "Teams",
                        principalColumn: "idTeam",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    idDiscount = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discountValue = table.Column<double>(type: "float", nullable: false),
                    conditions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    receiptidReceipt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.idDiscount);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    idPayment = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    clientid = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<double>(type: "float", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    paymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    idDiscount = table.Column<int>(type: "int", nullable: true),
                    discountidDiscount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.idPayment);
                    table.ForeignKey(
                        name: "FK_Payments_Clients_clientid",
                        column: x => x.clientid,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Discounts_discountidDiscount",
                        column: x => x.discountidDiscount,
                        principalTable: "Discounts",
                        principalColumn: "idDiscount");
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    idReceipt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idPayment = table.Column<int>(type: "int", nullable: false),
                    paymentidPayment = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    receiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    totalAmount = table.Column<double>(type: "float", nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.idReceipt);
                    table.ForeignKey(
                        name: "FK_Receipts_Payments_paymentidPayment",
                        column: x => x.paymentidPayment,
                        principalTable: "Payments",
                        principalColumn: "idPayment",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    idReservation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    idCourt = table.Column<int>(type: "int", nullable: false),
                    reservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    startTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    endTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    isPaid = table.Column<bool>(type: "bit", nullable: false),
                    totalPrice = table.Column<double>(type: "float", nullable: false),
                    idPayment = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.idReservation);
                    table.ForeignKey(
                        name: "FK_Reservations_Clients_idClient",
                        column: x => x.idClient,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Courts_idCourt",
                        column: x => x.idCourt,
                        principalTable: "Courts",
                        principalColumn: "idCourt",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Payments_idPayment",
                        column: x => x.idPayment,
                        principalTable: "Payments",
                        principalColumn: "idPayment",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_classId",
                table: "Assistances",
                column: "classId");

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_clientId",
                table: "Assistances",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_professorId",
                table: "Certifications",
                column: "professorId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_courtId",
                table: "Classes",
                column: "courtId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_profesorId",
                table: "Classes",
                column: "profesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClassidClass",
                table: "Clients",
                column: "ClassidClass");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_idTeam",
                table: "Clients",
                column: "idTeam");

            migrationBuilder.CreateIndex(
                name: "IX_CompetenceTeams_idPayment",
                table: "CompetenceTeams",
                column: "idPayment");

            migrationBuilder.CreateIndex(
                name: "IX_CompetenceTeams_idTeam",
                table: "CompetenceTeams",
                column: "idTeam");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_courtTypeId",
                table: "Courts",
                column: "courtTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_receiptidReceipt",
                table: "Discounts",
                column: "receiptidReceipt");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilities_courtId",
                table: "Disponibilities",
                column: "courtId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_idCompetence",
                table: "Matches",
                column: "idCompetence");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_idCourt",
                table: "Matches",
                column: "idCourt");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_idTeamA",
                table: "Matches",
                column: "idTeamA");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_idTeamB",
                table: "Matches",
                column: "idTeamB");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_clientid",
                table: "Payments",
                column: "clientid");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_discountidDiscount",
                table: "Payments",
                column: "discountidDiscount");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_paymentidPayment",
                table: "Receipts",
                column: "paymentidPayment");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_idAdmin",
                table: "Reports",
                column: "idAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_idClient",
                table: "Reservations",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_idCourt",
                table: "Reservations",
                column: "idCourt");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_idPayment",
                table: "Reservations",
                column: "idPayment",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_clientId",
                table: "Teams",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assistances_Clients_clientId",
                table: "Assistances",
                column: "clientId",
                principalTable: "Clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Teams_idTeam",
                table: "Clients",
                column: "idTeam",
                principalTable: "Teams",
                principalColumn: "idTeam",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetenceTeams_Payments_idPayment",
                table: "CompetenceTeams",
                column: "idPayment",
                principalTable: "Payments",
                principalColumn: "idPayment",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Receipts_receiptidReceipt",
                table: "Discounts",
                column: "receiptidReceipt",
                principalTable: "Receipts",
                principalColumn: "idReceipt",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Professors_PersonalClubs_id",
                table: "Professors");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Classes_ClassidClass",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Clients_clientid",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Clients_clientId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Payments_paymentidPayment",
                table: "Receipts");

            migrationBuilder.DropTable(
                name: "Assistances");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "CompetenceTeams");

            migrationBuilder.DropTable(
                name: "Disponibilities");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Competences");

            migrationBuilder.DropTable(
                name: "PersonalClubs");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Courts");

            migrationBuilder.DropTable(
                name: "Professors");

            migrationBuilder.DropTable(
                name: "CourtTypes");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "Receipts");
        }
    }
}
