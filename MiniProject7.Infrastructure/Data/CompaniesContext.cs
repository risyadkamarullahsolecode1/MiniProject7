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

            modelBuilder.Entity<Process>(entity =>
            {
                entity.HasOne(p => p.Workflow)
                     .WithMany(w => w.Processes)
                     .HasForeignKey(p => p.WorkflowId)
                     .HasConstraintName("FK_Process_Workflow");

                entity.HasOne(p => p.Requester)
                     .WithMany(r => r.Processes)
                     .HasForeignKey(p => p.RequesterId)
                     .HasConstraintName("FK_Process_Requester");
            });
            modelBuilder.Entity<WorkflowSequence>(entity =>
            {
                entity.HasOne(wfs => wfs.Workflow).WithMany(w => w.WorkflowSequence)
                     .HasForeignKey(wfs => wfs.WorkflowId)
                     .HasConstraintName("workflow_sequence_id_workflow_fkey");
            });
            modelBuilder.Entity<WorkflowAction>(entity =>
            {
                entity.HasOne(wf => wf.Process).WithMany(p => p.WorkflowActions)
                     .HasForeignKey(wf => wf.ProcessId)
                     .HasConstraintName("workflow_action_id_request_fkey");

                entity.HasOne(e => e.Actor).WithMany(u => u.WorkflowActions)
                     .HasForeignKey(e => e.ActorId)
                     .HasConstraintName("FK_WorkflowAction_User");
            });
            modelBuilder.Entity<NextStepRule>(entity =>
            {
                entity.HasOne(d => d.CurrentStep)
                     .WithMany()
                     .HasForeignKey(d => d.CurrentStepId)
                     .HasConstraintName("next_step_rule_id_currentstep_fkey")
                     .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.NextStep)
                     .WithMany()
                     .HasForeignKey(d => d.NextStepId)
                     .HasConstraintName("next_step_rule_id_nextstep_fkey")
                     .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
