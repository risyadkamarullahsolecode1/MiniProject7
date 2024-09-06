using MiniProject7.Application.Dtos.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseModel> SignUpAsync(RegisterEmployee registerEmployee);
        Task<ResponseModel> LoginAsync(LoginModel model);
        Task<ResponseModel> CreateRoleAsync(string rolename);
        Task<ResponseModel> UpdateUserAsync(string username, RegisterModel model);
        Task<ResponseModel> AssignToRoleAsync(string userName, string rolename);
        Task<ResponseModel> UpdateToRoleAsync(string userName, string rolename);
        Task<ResponseModel> DeleteRoleAsync(string rolename);
        string GenerateRefreshToken();
        Task<ResponseModel> UpdateRoleAsync(string rolename);
        Task<ResponseModel> DeleteAsync(string userName);
    }
}
