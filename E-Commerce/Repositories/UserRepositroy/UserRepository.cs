using E_Commerce.Models;
using E_Commerce.Services;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class UserRepository : MongoRepository<User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRoles> _roleManager;
        private readonly IJwtProvider _jwtProvider;

        public UserRepository(UserManager<User> userManager,
                              RoleManager<UserRoles> roleManager, 
                              IJwtProvider jwtProvider, 
                              IMongoDatabase database,
                              IUnitOfWork unitOfWork)
            : base(database, unitOfWork, "Users")
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtProvider = jwtProvider;
        }

        public async Task<OperationResult<UserAuth>> LoginAsync(LoginModel model )
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var token = _jwtProvider.Generate(user, userRoles);
                var returnedUser= new UserAuth
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    token=token,
                    Role= userRoles.FirstOrDefault(),
                    Email= user.Email
                  
                };
                return OperationResult<UserAuth>.SuccessResult(returnedUser);
            }

            return OperationResult<UserAuth>.FailureResult(401,"Wrong email or password"); ;
        }

        public async Task<OperationResult<UserAuth>> SignupAsync(UserModel model)
        {
                
            var userExsist = await _userManager.FindByEmailAsync(model.Email!);
            if (userExsist != null)
            {
                return OperationResult<UserAuth>.FailureResult(409,"This user is already exsisted");
            }
            User newuser = new()
            {
                Email = model.Email,
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(newuser, model.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return OperationResult<UserAuth>.FailureResult(400,errors);
            }
            await _userManager.AddToRoleAsync(newuser, model.Role!);
            var returnedUser = new UserAuth
            {
                Id = newuser.Id,
                UserName = newuser.UserName!,
                Email = newuser.Email,
                Role=model.Role,
            };

            return OperationResult<UserAuth>.SuccessResult(returnedUser);
        }
        public async Task<List<SelectListItem>> GetAllRolesAsSelectListAsync()
        {
            return await Task.Run(() =>
                _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name!,
                    Value = r.Name!
                }).ToList()
            );
        }
        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }


    }
}
