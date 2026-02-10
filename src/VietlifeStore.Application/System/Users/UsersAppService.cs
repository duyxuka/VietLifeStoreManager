using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp;
using VietlifeStore.Entity.TaiKhoans;
using Volo.Abp.ObjectMapping;

namespace VietlifeStore.System.Users
{
    public class UsersAppService : CrudAppService<TaiKhoan, UserDto, Guid, PagedResultRequestDto,
                        CreateUserDto, UpdateUserDto>, IUsersAppService
    {
        private readonly TaiKhoanManager _taiKhoanManager;

        public UsersAppService(IRepository<TaiKhoan, Guid> repository,
            TaiKhoanManager taiKhoanManager) : base(repository)
        {
            _taiKhoanManager = taiKhoanManager;

            GetPolicyName = ExtendedIdentityPermissions.Users.View;
            GetListPolicyName = ExtendedIdentityPermissions.Users.View;
            CreatePolicyName = IdentityPermissions.Users.Create;
            UpdatePolicyName = IdentityPermissions.Users.Update;
            DeletePolicyName = IdentityPermissions.Users.Delete;
        }

        [Authorize(IdentityPermissions.Users.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        [Authorize(ExtendedIdentityPermissions.Users.View)]
        public async Task<List<UserInListDto>> GetListAllAsync(string filterKeyword)
        {
            var query = await Repository.GetQueryableAsync();
            if (!string.IsNullOrEmpty(filterKeyword))
            {
                query = query.Where(o => o.Name.ToLower().Contains(filterKeyword)
              || o.Email.ToLower().Contains(filterKeyword)
              || o.PhoneNumber.ToLower().Contains(filterKeyword));
            }

            var data = await AsyncExecuter.ToListAsync(query);
            return ObjectMapper.Map<List<TaiKhoan>, List<UserInListDto>>(data);
        }

        [Authorize(ExtendedIdentityPermissions.Users.View)]
        public async Task<List<UserInListDto>> GetCustomersAsync(string? filterKeyword = null)
        {
            var identityUsers = await _taiKhoanManager.GetUsersInRoleAsync("customer");

            var taiKhoans = identityUsers
                .OfType<TaiKhoan>()          
                .ToList();

            if (!string.IsNullOrWhiteSpace(filterKeyword))
            {
                filterKeyword = filterKeyword.ToLowerInvariant();
                taiKhoans = taiKhoans.Where(u =>
                    (u.Name?.ToLowerInvariant().Contains(filterKeyword) ?? false) ||
                    (u.Email?.ToLowerInvariant().Contains(filterKeyword) ?? false) ||
                    (u.PhoneNumber?.ToLowerInvariant().Contains(filterKeyword) ?? false)
                ).ToList();
            }

            return ObjectMapper.Map<List<TaiKhoan>, List<UserInListDto>>(taiKhoans);
        }

        [Authorize(ExtendedIdentityPermissions.Users.View)]
        public async Task<PagedResultDto<UserInListDto>> GetListWithFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (!input.Keyword.IsNullOrWhiteSpace())
            {
                input.Keyword = input.Keyword.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(input.Keyword)
                || o.Email.ToLower().Contains(input.Keyword)
                || o.PhoneNumber.ToLower().Contains(input.Keyword));
            }
            query = query.OrderByDescending(x => x.CreationTime);

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var data = await AsyncExecuter.ToListAsync(query);
            var users = ObjectMapper.Map<List<TaiKhoan>, List<UserInListDto>>(data);
            return new PagedResultDto<UserInListDto>(totalCount, users);
        }

        [Authorize(IdentityPermissions.Users.Create)]
        public async override Task<UserDto> CreateAsync(CreateUserDto input)
        {
            var query = await Repository.GetQueryableAsync();
            var isUserNameExisted = query.Any(x => x.UserName == input.UserName);
            if (isUserNameExisted)
            {
                throw new UserFriendlyException("Tài khoản đã tồn tại");
            }

            var isUserEmailExisted = query.Any(x => x.Email == input.Email);
            if (isUserEmailExisted)
            {
                throw new UserFriendlyException("Email đã tồn tại");
            }
            var userId = Guid.NewGuid();

            var user = new TaiKhoan(userId, input.UserName, input.Email, CurrentTenant.Id)
            {
                Name = input.Name,
                Surname = input.Surname,
                IsCustomer = input.IsCustomer,
                Status = input.Status
            };

            user.SetPhoneNumber(input.PhoneNumber, true);

            var result = await _taiKhoanManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                return ObjectMapper.Map<IdentityUser, UserDto>(user);
            }
            else
            {
                List<Microsoft.AspNetCore.Identity.IdentityError> errorList = result.Errors.ToList();
                string errors = "";

                foreach (var error in errorList)
                {
                    errors = errors + error.Description.ToString();
                }
                throw new UserFriendlyException(errors);
            }
        }

        [Authorize(IdentityPermissions.Users.Update)]
        public async override Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
        {
            var user = await _taiKhoanManager.FindByIdAsync(id.ToString()) as TaiKhoan;
            if (user == null)
            {
                throw new EntityNotFoundException(typeof(TaiKhoan), id);
            }
            user.Name = input.Name;
            user.SetPhoneNumber(input.PhoneNumber, true);
            user.Surname = input.Surname;

            user.IsCustomer = input.IsCustomer;
            user.Status = input.Status;

            var result = await _taiKhoanManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return ObjectMapper.Map<TaiKhoan, UserDto>(user);
            }
            else
            {
                List<Microsoft.AspNetCore.Identity.IdentityError> errorList = result.Errors.ToList();
                string errors = "";

                foreach (var error in errorList)
                {
                    errors = errors + error.Description.ToString();
                }
                throw new UserFriendlyException(errors);
            }
        }

        [Authorize(ExtendedIdentityPermissions.Users.View)]
        public async override Task<UserDto> GetAsync(Guid id)
        {
            var user = await _taiKhoanManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new EntityNotFoundException(typeof(IdentityUser), id);
            }
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);

            //Get roles from users
            var roles = await _taiKhoanManager.GetRolesAsync(user);
            userDto.Roles = roles;
            return userDto;
        }

        [Authorize(IdentityPermissions.Users.Update)]
        public async Task AssignRolesAsync(Guid userId, string[] roleNames)
        {
            var user = await _taiKhoanManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new EntityNotFoundException(typeof(IdentityUser), userId);
            }
            var currentRoles = await _taiKhoanManager.GetRolesAsync(user);
            var removedResult = await _taiKhoanManager.RemoveFromRolesAsync(user, currentRoles);
            var addedResult = await _taiKhoanManager.AddToRolesAsync(user, roleNames);
            if (!addedResult.Succeeded || !removedResult.Succeeded)
            {
                List<Microsoft.AspNetCore.Identity.IdentityError> addedErrorList = addedResult.Errors.ToList();
                List<Microsoft.AspNetCore.Identity.IdentityError> removedErrorList = removedResult.Errors.ToList();
                var errorList = new List<Microsoft.AspNetCore.Identity.IdentityError>();
                errorList.AddRange(addedErrorList);
                errorList.AddRange(removedErrorList);
                string errors = "";

                foreach (var error in errorList)
                {
                    errors = errors + error.Description.ToString();
                }
                throw new UserFriendlyException(errors);
            }
        }

        [Authorize(IdentityPermissions.Users.Update)]
        public async Task SetPasswordAsync(Guid userId, SetPasswordDto input)
        {
            var user = await _taiKhoanManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new EntityNotFoundException(typeof(IdentityUser), userId);
            }
            var token = await _taiKhoanManager.GeneratePasswordResetTokenAsync(user);
            var result = await _taiKhoanManager.ResetPasswordAsync(user, token, input.NewPassword);
            if (!result.Succeeded)
            {
                List<Microsoft.AspNetCore.Identity.IdentityError> errorList = result.Errors.ToList();
                string errors = "";

                foreach (var error in errorList)
                {
                    errors = errors + error.Description.ToString();
                }
                throw new UserFriendlyException(errors);
            }
        }
    }
}
