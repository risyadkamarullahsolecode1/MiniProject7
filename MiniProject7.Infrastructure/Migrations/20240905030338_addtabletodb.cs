using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiniProject7.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtabletodb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("location_pkey", x => x.location);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    deptno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deptname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mgrempno = table.Column<int>(type: "integer", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    spvempno = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("departments_pkey", x => x.deptno);
                    table.ForeignKey(
                        name: "fk_location",
                        column: x => x.location,
                        principalTable: "Locations",
                        principalColumn: "location");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    empno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "character varying", nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: false),
                    sex = table.Column<string>(type: "character varying", nullable: true),
                    phonenumber = table.Column<int>(type: "integer", nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deptno = table.Column<int>(type: "integer", nullable: true),
                    employeetype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true),
                    lastupdateddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    nik = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying", nullable: true),
                    statusreason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    salary = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("employees_pkey", x => x.empno);
                    table.ForeignKey(
                        name: "fk_deptno",
                        column: x => x.deptno,
                        principalTable: "Departments",
                        principalColumn: "deptno");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    projno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    projname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    deptno = table.Column<int>(type: "integer", nullable: true),
                    projectlocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("projects_pkey", x => x.projno);
                    table.ForeignKey(
                        name: "projects_deptno_fkey",
                        column: x => x.deptno,
                        principalTable: "Departments",
                        principalColumn: "deptno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dependents",
                columns: table => new
                {
                    dependentno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sex = table.Column<string>(type: "character varying", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: false),
                    relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    empno = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("dependents_pkey", x => x.dependentno);
                    table.ForeignKey(
                        name: "dependents_empno_fkey",
                        column: x => x.empno,
                        principalTable: "Employees",
                        principalColumn: "empno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Worksons",
                columns: table => new
                {
                    empno = table.Column<int>(type: "integer", nullable: false),
                    projno = table.Column<int>(type: "integer", nullable: false),
                    dateworked = table.Column<DateOnly>(type: "date", nullable: false),
                    hoursworked = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("workson_pkey", x => new { x.empno, x.projno });
                    table.ForeignKey(
                        name: "workson_empno_fkey",
                        column: x => x.empno,
                        principalTable: "Employees",
                        principalColumn: "empno",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "workson_projno_fkey",
                        column: x => x.projno,
                        principalTable: "Projects",
                        principalColumn: "projno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_location",
                table: "Departments",
                column: "location");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_mgrempno",
                table: "Departments",
                column: "mgrempno");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_spvempno",
                table: "Departments",
                column: "spvempno");

            migrationBuilder.CreateIndex(
                name: "IX_Dependents_empno",
                table: "Dependents",
                column: "empno");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_deptno",
                table: "Employees",
                column: "deptno");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_deptno",
                table: "Projects",
                column: "deptno");

            migrationBuilder.CreateIndex(
                name: "IX_Worksons_projno",
                table: "Worksons",
                column: "projno");

            migrationBuilder.AddForeignKey(
                name: "departments_mgrempno_fkey",
                table: "Departments",
                column: "mgrempno",
                principalTable: "Employees",
                principalColumn: "empno",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_spvempno",
                table: "Departments",
                column: "spvempno",
                principalTable: "Employees",
                principalColumn: "empno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "departments_mgrempno_fkey",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "fk_spvempno",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "Dependents");

            migrationBuilder.DropTable(
                name: "Worksons");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
