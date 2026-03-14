using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.CamNangsList.CamNangComments;
using VietlifeStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp;
using VietlifeStore.Entity.CamNangsList.DanhMucCamNangs;
using VietlifeStore.Entity.SanPhamsList.QuaTangs;
using Volo.Abp.ObjectMapping;

namespace VietlifeStore.Entity.CamNangs
{
    public class CamNangCommentAppService :
        CrudAppService<
            CamNangComment,
            CamNangCommentDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateCamNangCommentDto,
            CreateUpdateCamNangCommentDto>,
        ICamNangCommentAppService
    {
        private readonly IRepository<CamNang, Guid> _camNangRepository;
        public CamNangCommentAppService(
            IRepository<CamNangComment, Guid> repository, IRepository<CamNang, Guid> camNangRepository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.CamNang.View;
            GetListPolicyName = VietlifeStorePermissions.CamNang.View;
            CreatePolicyName = VietlifeStorePermissions.CamNang.Create;
            UpdatePolicyName = VietlifeStorePermissions.CamNang.Update;
            DeletePolicyName = VietlifeStorePermissions.CamNang.Delete;
            _camNangRepository = camNangRepository;
        }

        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<List<CamNangCommentInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync(x => x.TrangThai);
            return ObjectMapper.Map<List<CamNangComment>, List<CamNangCommentInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.CamNang.View)]
        public async Task<PagedResultDto<CamNangCommentInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var commentQuery = await Repository.GetQueryableAsync();
            var camNangQuery = await _camNangRepository.GetQueryableAsync();

            var query =
                from comment in commentQuery
                join camNang in camNangQuery
                    on comment.CamNangId equals camNang.Id
                select new { comment, camNang };

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.comment.Email.Contains(input.Keyword))
                .WhereIf(input.Id.HasValue,
                    x => x.comment.CamNangId == input.Id);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                .OrderByDescending(x => x.comment.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
            );

            var result = items.Select(x => new CamNangCommentInListDto
            {
                Id = x.comment.Id,
                CamNangId = x.comment.CamNangId,
                CamNangTen = x.camNang.Ten,
                TenNguoiDung = x.comment.TenNguoiDung,
                Email = x.comment.Email,
                NoiDung = x.comment.NoiDung,
                TrangThai = x.comment.TrangThai,
                ParentId = x.comment.ParentId,
                CreationTime = x.comment.CreationTime
            }).ToList();

            return new PagedResultDto<CamNangCommentInListDto>(
                totalCount,
                result
            );
        }
        // ================= CREATE COMMENT =================

        [Authorize]
        public override async Task<CamNangCommentDto> CreateAsync(CreateUpdateCamNangCommentDto input)
        {
            if (string.IsNullOrWhiteSpace(input.NoiDung))
                throw new UserFriendlyException("Nội dung comment không được để trống");

            string tenNguoiDung;
            string email;

            // Nếu admin gửi seeding thì dùng input
            if (!string.IsNullOrWhiteSpace(input.TenNguoiDung))
            {
                tenNguoiDung = input.TenNguoiDung;
                email = input.Email;
            }
            else
            {
                // User comment bình thường
                if (!CurrentUser.IsAuthenticated)
                    throw new UserFriendlyException("Bạn cần đăng nhập để bình luận");

                tenNguoiDung = CurrentUser.UserName;
                email = CurrentUser.Email;
            }

            var entity = new CamNangComment
            {
                CamNangId = input.CamNangId,
                TenNguoiDung = tenNguoiDung,
                Email = email,
                NoiDung = input.NoiDung,
                ParentId = input.ParentId,
                TrangThai = true,

                CreationTime = DateTime.Now.AddMinutes(-Random.Shared.Next(5, 3000))
            };

            await Repository.InsertAsync(entity, autoSave: true);

            return ObjectMapper.Map<CamNangComment, CamNangCommentDto>(entity);
        }

        // ================= GET COMMENT BY CAMNANG =================

        [AllowAnonymous]
        public async Task<List<CamNangCommentDto>> GetListByCamNangAsync(Guid camNangId)
        {
            var comments = await Repository.GetListAsync(
                x => x.CamNangId == camNangId && x.TrangThai
            );

            // Map toàn bộ sang DTO một lần
            var allDtos = ObjectMapper.Map<List<CamNangComment>, List<CamNangCommentDto>>(comments);

            // Build cây: gán Replies cho từng node
            // Dùng Dictionary để O(1) lookup
            var lookup = allDtos.ToDictionary(x => x.Id);

            foreach (var dto in allDtos)
            {
                // Reset để tránh trùng nếu gọi nhiều lần
                dto.Replies = new List<CamNangCommentDto>();
            }

            foreach (var dto in allDtos)
            {
                if (dto.ParentId.HasValue && lookup.ContainsKey(dto.ParentId.Value))
                {
                    // Gán vào đúng cha của nó (bất kể cấp nào)
                    lookup[dto.ParentId.Value].Replies.Add(dto);
                }
            }

            // Sắp xếp replies theo thời gian tăng dần
            foreach (var dto in allDtos)
            {
                dto.Replies = dto.Replies.OrderBy(x => x.CreationTime).ToList();
            }

            // Chỉ trả về root comments (không có ParentId)
            var roots = allDtos
                .Where(x => x.ParentId == null)
                .OrderByDescending(x => x.CreationTime)
                .ToList();

            return roots;
        }


        // ================= DELETE MULTIPLE =================

        [Authorize(VietlifeStorePermissions.CamNang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
