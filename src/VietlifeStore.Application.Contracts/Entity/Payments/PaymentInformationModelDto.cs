using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace VietlifeStore.Entity.Payments
{
    public class PaymentInformationModelDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Amount { get; set; }
        public string TransactionId { get; set; }
        public string PaymentCode { get; set; }
        public string PaymentInfor { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
