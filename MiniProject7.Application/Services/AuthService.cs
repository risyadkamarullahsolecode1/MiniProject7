using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniProject7.Application.Dtos.Account;
using MiniProject7.Application.Interfaces;
using MiniProject7.Domain.Entities;
using MiniProject7.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniProject7.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;

        public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmployeeRepository employeeRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _employeeRepository = employeeRepository;
        }

        //Sign Up The User
        public async Task<ResponseModel> SignUpAsync(RegisterEmployee registerEmployee)
        {
            var userExists = await _userManager.FindByNameAsync(registerEmployee.Fname+""+registerEmployee.Lname);
            if (userExists != null)

                return new ResponseModel { Status = "Error", Message = "User already exists!" };
            AppUser user = new AppUser()
            {
                Email = registerEmployee.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerEmployee.Fname + "" + registerEmployee.Lname
            };
            var result = await _userManager.CreateAsync(user, registerEmployee.Password);
            if (!result.Succeeded) return new ResponseModel
            {
                Status = "Error",
                Message = "User creation failed! Please check user details and try again."
            };

            var userId = user.Id;

            Employee employees = new Employee()
            {
                Empno = registerEmployee.Empno,
                Fname = registerEmployee.Fname,
                Lname = registerEmployee.Lname,
                Email = registerEmployee.Email,
                Address = registerEmployee.Address,
                Dob = registerEmployee.Dob,
                Sex = registerEmployee.Sex,
                Phonenumber = registerEmployee.Phonenumber,
                Position = registerEmployee.Position,
                Deptno = registerEmployee.Deptno,
                Employeetype = registerEmployee.Employeetype,
                Level = registerEmployee.Level,
                Lastupdateddate = registerEmployee.Lastupdateddate,
                Nik = registerEmployee.Nik,
                Salary = registerEmployee.Salary,
                Status = registerEmployee.Status,
                Statusreason = registerEmployee.Statusreason,
                UserId = userId,
            };
            await _employeeRepository.AddEmployee(employees);
            await _employeeRepository.SaveChangesAsync();
            return new ResponseModel { Status = "Success", Message = "User created succesfully!" };
        }

        //Login user
        public async Task<ResponseModel> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole.ToString()));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                var refreshTokenExpiryDate = user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(8);

                await _userManager.UpdateAsync(user);

                await _userManager.UpdateAsync(user);

                return new ResponseModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = refreshTokenExpiryDate,
                    ExpiredOn = token.ValidTo,
                    Message = "User successfully login!",
                    Roles = userRoles.ToList(),
                    Status = "Success",
                    Username = user.UserName,
                };
            }
            return new ResponseModel { Status = "Error", Message = "Password Not valid!" };
        }
        // update user
        public async Task<ResponseModel> UpdateUserAsync(string username, RegisterModel model)
        {
            // Find the user by userName
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new ResponseModel { Status = "Error", Message = "User not found!" };
            }

            user.Email = model.Email;
            user.UserName = model.Username;
            user.SecurityStamp = model.Password;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new ResponseModel
                {
                    Status = "Error",
                    Message = "User update failed! Please check user details and try again."
                };
            }
            return new ResponseModel { Status = "Success", Message = "User updated successfully!" };
        }

        //Delete User
        public async Task<ResponseModel> DeleteAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            await _userManager.DeleteAsync(user);
            return new ResponseModel { Status = "Success", Message = "User deleted succesfully!" };
        }
        // Create Role
        public async Task<ResponseModel> CreateRoleAsync(string rolename)
        {
            if (!await _roleManager.RoleExistsAsync(rolename))
                await _roleManager.CreateAsync(new IdentityRole(rolename));
            return new ResponseModel { Status = "Success", Message = "Role Created successfully!" };
        }

        // Assign user to role that already created before
        public async Task<ResponseModel> AssignToRoleAsync(string userName, string rolename)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (await _roleManager.RoleExistsAsync($"{rolename}"))
            {
                await _userManager.AddToRoleAsync(user, rolename);
            }
            return new ResponseModel { Status = "Success", Message = "User created succesfully!" };
        }

        // update role for user
        public async Task<ResponseModel> UpdateToRoleAsync(string userName, string rolename)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (!await _roleManager.RoleExistsAsync($"{rolename}"))
            {
                return new ResponseModel { Status = "Error", Message = "User roles not found!" };
            }

            var removeRole = await _userManager.GetRolesAsync(user);

            var updateRoleUser = await _userManager.AddToRoleAsync(user, rolename);
            return new ResponseModel { Status = "Success", Message = "User roles updated succesfully!" };
        }

        //update role
        public async Task<ResponseModel> UpdateRoleAsync(string rolename)
        {
            var role = await _roleManager.FindByNameAsync(rolename);
            if (role == null)
            {
                return new ResponseModel { Status = "Error", Message = "User roles not found!" };
            }
            var result = await _roleManager.UpdateAsync(role);
            return new ResponseModel { Status = "Success", Message = "Roles updated succesfully!" };
        }

        //Delete Role
        public async Task<ResponseModel> DeleteRoleAsync(string rolename)
        {
            var role = await _roleManager.FindByNameAsync(rolename);
            if (role == null)
            {
                return new ResponseModel { Status = "Error", Message = "User roles not found!" };
            }
            var result = await _roleManager.DeleteAsync(role);
            return new ResponseModel { Status = "Success", Message = "Roles deleted succesfully!" };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        
    }
}
