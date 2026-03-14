using AutoMapper.Internal.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.Payments;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using VietlifeStore.Permissions;
using VietlifeStore.Entity.SanPhamsList.DanhMucSanPhams;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using VietlifeStore.Entity.SanPhamsList.QuaTangs;
using Volo.Abp.ObjectMapping;

namespace VietlifeStore.Payments
{
    public class PaymentInformationModelsAppService :
        CrudAppService<
            PaymentInformationModel,                 // Entity
            PaymentInformationModelDto,              // DTO
            Guid,                                    // Primary key
            PagedResultRequestDto,                   // Paging
            CreateUpdatePaymentInformationModelDto,  // Create DTO
            CreateUpdatePaymentInformationModelDto>, // Update DTO
        IPaymentInformationModelsAppService
    {
        public PaymentInformationModelsAppService(
            IRepository<PaymentInformationModel, Guid> repository)
            : base(repository)
        {
            // Permission
            GetPolicyName = VietlifeStorePermissions.PaymentInformationModel.View;
            GetListPolicyName = VietlifeStorePermissions.PaymentInformationModel.View;
            CreatePolicyName = VietlifeStorePermissions.PaymentInformationModel.Create;
            UpdatePolicyName = VietlifeStorePermissions.PaymentInformationModel.Update;
            DeletePolicyName = VietlifeStorePermissions.PaymentInformationModel.Delete;
        }

        // ================= GET ALL =================
        [Authorize(VietlifeStorePermissions.QuaTang.View)]
        public async Task<List<PaymentInformationModelInListDto>> GetListAllAsync()
        {
            var list = await Repository.GetListAsync();
            return ObjectMapper.Map<List<PaymentInformationModel>, List<PaymentInformationModelInListDto>>(list);
        }

        // ================= FILTER + PAGING =================
        [Authorize(VietlifeStorePermissions.QuaTang.View)]
        public async Task<PagedResultDto<PaymentInformationModelInListDto>> GetListFilterAsync(BaseListFilterDto input)
        {
            var query = await Repository.GetQueryableAsync();

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.PaymentCode.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<PaymentInformationModelInListDto>(
                totalCount,
                ObjectMapper.Map<List<PaymentInformationModel>, List<PaymentInformationModelInListDto>>(items)
            );
        }

        // ================= DELETE MULTIPLE =================
        [Authorize(VietlifeStorePermissions.QuaTang.Delete)]
        public async Task DeleteMultipleAsync(IEnumerable<Guid> ids)
        {
            await Repository.DeleteManyAsync(ids);
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
