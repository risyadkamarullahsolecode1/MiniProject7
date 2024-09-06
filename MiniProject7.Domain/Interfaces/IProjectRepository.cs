using MiniProject7.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProject();
        Task<Project> GetProjectById(int projNo);
        Task<Project> AddProject(Project project);
        Task<Project> UpdateProject(Project project);
        Task<bool> DeleteProject(int projNo);
    }
}
