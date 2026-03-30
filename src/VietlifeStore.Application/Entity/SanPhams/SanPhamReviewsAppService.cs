using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VietlifeStore.Entity.SanPhams;
using VietlifeStore.Entity.SanPhamsList.SanPhamReviews;
using VietlifeStore.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace VietlifeStore.Entity.SanPhamsList.SanPhamReviews
{
    public class SanPhamReviewsAppService :
        CrudAppService<
            SanPhamReview,
            SanPhamReviewDto,
            Guid,
            PagedResultRequestDto,
            CreateUpdateSanPhamReviewDto,
            CreateUpdateSanPhamReviewDto>,
        ISanPhamReviewsAppService
    {
        private readonly IRepository<SanPham, Guid> _sanPhamRepository;
        public SanPhamReviewsAppService(
            IRepository<SanPhamReview, Guid> repository, IRepository<SanPham, Guid> sanPhamRepository)
            : base(repository)
        {
            GetPolicyName = VietlifeStorePermissions.SanPham.View;
            GetListPolicyName = VietlifeStorePermissions.SanPham.View;
            CreatePolicyName = VietlifeStorePermissions.SanPham.Create;
            UpdatePolicyName = VietlifeStorePermissions.SanPham.Update;
            DeletePolicyName = VietlifeStorePermissions.SanPham.Delete;
            _sanPhamRepository = sanPhamRepository;
        }

        // ================= GET ALL =================
        [AllowAnonymous]
        public async Task<List<SanPhamReviewInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync(x => x.TrangThai);

            return ObjectMapper.Map<List<SanPhamReview>, List<SanPhamReviewInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        public async Task<PagedResultDto<SanPhamReviewInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var reviewQuery = await Repository.GetQueryableAsync();

            reviewQuery = reviewQuery
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.TenNguoiDung.Contains(input.Keyword) ||
                         x.NoiDung.Contains(input.Keyword))
                .WhereIf(input.Id.HasValue,
                    x => x.SanPhamId == input.Id);

            var totalCount = await AsyncExecuter.CountAsync(reviewQuery);

            var sanPhamQuery = await _sanPhamRepository.GetQueryableAsync();

            var query =
                from review in reviewQuery
                join sanPham in sanPhamQuery
                on review.SanPhamId equals sanPham.Id
                orderby review.CreationTime descending
                select new
                {
                    review,
                    TenSanPham = sanPham.Ten
                };

            var items = await AsyncExecuter.ToListAsync(
                query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
            );

            var result = items.Select(x => new SanPhamReviewInListDto
            {
                Id = x.review.Id,
                SanPhamId = x.review.SanPhamId,
                TenSanPham = x.TenSanPham,
                TenNguoiDung = x.review.TenNguoiDung,
                Email = x.review.Email,
                SoSao = x.review.SoSao,
                NoiDung = x.review.NoiDung,
                TrangThai = x.review.TrangThai,
                CreationTime = x.review.CreationTime
            }).ToList();

            return new PagedResultDto<SanPhamReviewInListDto>(totalCount, result);
        }

        // ================= GET REVIEW BY PRODUCT =================
        [AllowAnonymous]
        public async Task<List<SanPhamReviewInListDto>> GetBySanPhamAsync(Guid sanPhamId)
        {
            var list = await Repository.GetListAsync(
                x => x.SanPhamId == sanPhamId && x.TrangThai
            );

            return ObjectMapper.Map<List<SanPhamReview>, List<SanPhamReviewInListDto>>(list);
        }

        // ================= CREATE REVIEW =================
        [Authorize]
        public override async Task<SanPhamReviewDto> CreateAsync(CreateUpdateSanPhamReviewDto input)
        {
            if (string.IsNullOrWhiteSpace(input.NoiDung) || input.NoiDung.Trim().Length < 10)
                throw new UserFriendlyException("Nội dung đánh giá phải từ 10 ký tự trở lên");

            if (input.SoSao < 1 || input.SoSao > 5)
                throw new UserFriendlyException("Số sao phải từ 1 đến 5");

            bool isAdminSeeding = !string.IsNullOrWhiteSpace(input.TenNguoiDung);

            Guid userId;
            string tenNguoiDung;
            string email;

            if (isAdminSeeding)
            {
                // Admin seeding → dùng dữ liệu input
                userId = Guid.Empty;
                tenNguoiDung = input.TenNguoiDung;
                email = input.Email;
            }
            else
            {
                // User thật
                if (!CurrentUser.IsAuthenticated)
                    throw new UserFriendlyException("Bạn cần đăng nhập để đánh giá sản phẩm");

                userId = CurrentUser.Id.Value;
                tenNguoiDung = CurrentUser.Name ?? "Khách hàng";
                email = CurrentUser.Email;

                // check đã review chưa
                var existingReview = await Repository.FirstOrDefaultAsync(x =>
                    x.SanPhamId == input.SanPhamId &&
                    x.UserId == userId &&
                    x.TrangThai == true
                );

                if (existingReview != null)
                    throw new UserFriendlyException("Bạn đã đánh giá sản phẩm này rồi! Chỉ được đánh giá 1 lần.");
            }

            var entity = new SanPhamReview
            {
                SanPhamId = input.SanPhamId,
                SoSao = input.SoSao,
                NoiDung = input.NoiDung.Trim(),

                UserId = userId,
                TenNguoiDung = tenNguoiDung,
                Email = email,

                TrangThai = true,
                CreationTime = DateTime.Now.AddMinutes(-Random.Shared.Next(10, 5000))
            };

            await Repository.InsertAsync(entity, autoSave: true);

            return ObjectMapper.Map<SanPhamReview, SanPhamReviewDto>(entity);
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.SanPham.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}