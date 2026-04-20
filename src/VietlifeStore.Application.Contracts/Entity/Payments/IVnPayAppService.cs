using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.Entity.DonHangs;

namespace VietlifeStore.Entity.Payments
{
    public interface IVnPayAppService
    {
        string CreatePaymentUrl(DonHang model);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
        Task<PaymentIPN> ResponsepayAsync(IQueryCollection collections);
    }
}
