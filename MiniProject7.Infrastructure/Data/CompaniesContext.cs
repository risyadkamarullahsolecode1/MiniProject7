using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Infrastructure.Data
{
    public partial class CompaniesContext:IdentityDbContext<AppUser>
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Workson> Worksons { get; set; }
        public DbSet<Dependent> Dependents { get; set; }
        public DbSet<Location> Locations { get; set; }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowSequence> WorkflowSequences { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<WorkflowAction> WorkflowActions { get; set; }
        public DbSet<NextStepRule> NextStepRules { get; set; }
        public CompaniesContext(DbContextOptions<CompaniesContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Deptno).HasName("departments_pkey");

                entity.HasOne(d => d.LocationNavigation).WithMany(p => p.Departments).HasConstraintName("fk_location");

                entity.HasOne(d => d.MgrempnoNavigation).WithMany(p => p.DepartmentMgrempnoNavigations)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("departments_mgrempno_fkey");

                entity.HasOne(d => d.SpvempnoNavigation).WithMany(p => p.DepartmentSpvempnoNavigations).HasConstraintName("fk_spvempno");
            });

            modelBuilder.Entity<Dependent>(entity =>
            {
                entity.HasKey(e => e.Dependentno).HasName("dependents_pkey");

                entity.HasOne(d => d.EmpnoNavigation).WithMany(p => p.Dependents)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("dependents_empno_fkey");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Empno).HasName("employees_pkey");

                entity.Property(e => e.Lastupdateddate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.DeptnoNavigation).WithMany(p => p.Employees).HasConstraintName("fk_deptno");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Locations).HasName("location_pkey");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Projno).HasName("projects_pkey");

                entity.HasOne(d => d.DeptnoNavigation).WithMany(p => p.Projects)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("projects_deptno_fkey");
            });

            modelBuilder.Entity<Workson>(entity =>
            {
                entity.HasKey(e => new { e.Empno, e.Projno }).HasName("workson_pkey");

                entity.HasOne(d => d.EmpnoNavigation).WithMany(p => p.Worksons).HasConstraintName("workson_empno_fkey");

                entity.HasOne(d => d.ProjnoNavigation).WithMany(p => p.Worksons).HasConstraintName("workson_projno_fkey");
            });

            modelBuilder.Entity<Employee>()
            .HasOne(e => e.AppUser)
            .WithMany(a => a.Employees)
            .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Workflow>()
           .HasMany(w => w.WorkflowSequence)
           .WithOne(ws => ws.Workflow)
           .HasForeignKey(ws => ws.WorkflowId);

            modelBuilder.Entity<WorkflowSequence>()
                .HasOne(ws => ws.RequiredRole)
                .WithMany()
                .HasForeignKey(ws => ws.RequiredRoleId);

            modelBuilder.Entity<Process>()
                .HasOne(p => p.Workflow)
                .WithMany(w => w.Processes)
                .HasForeignKey(p => p.WorkflowId);

            modelBuilder.Entity<Process>()
                .HasOne(p => p.Requester)
                .WithMany(u => u.Processes)
                .HasForeignKey(p => p.RequesterId);

            modelBuilder.Entity<NextStepRule>()
                .HasOne(nsr => nsr.CurrentStep)
                .WithMany(ws => ws.NextStepRules)
                .HasForeignKey(nsr => nsr.CurrentStepId);

            modelBuilder.Entity<NextStepRule>()
                .HasOne(nsr => nsr.NextStep)
                .WithMany()
                .HasForeignKey(nsr => nsr.NextStepId);

            modelBuilder.Entity<LeaveRequest>()
                .HasKey(lr => lr.RequestId);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(rb => rb.Process)
                .WithMany(p => p.LeaveRequests)
                .HasForeignKey(rb => rb.ProcessId);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(rb => rb.Employee)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(rb => rb.EmployeeId);

            modelBuilder.Entity<WorkflowAction>()
                .HasOne(wa => wa.Process)
                .WithMany(rb => rb.WorkflowActions)
                .HasForeignKey(wa => wa.ProcessId);

            modelBuilder.Entity<WorkflowAction>()
                .HasOne(wa => wa.Step)
                .WithMany(ws => ws.WorkflowActions)
                .HasForeignKey(wa => wa.StepId);

            modelBuilder.Entity<WorkflowAction>()
                .HasOne(wa => wa.Actor)
                .WithMany(u => u.WorkflowActions)
                .HasForeignKey(wa => wa.ActorId);

            modelBuilder.Entity<NextStepRule>()
                .HasKey(lr => lr.RuleId);
        }
    }
}
