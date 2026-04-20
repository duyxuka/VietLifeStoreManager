using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.Entity.Payments
{
    public class PaymentIPN
    {
        public string RspCode { get; set; }
        public string Message { get; set; }

        // Thêm các property này
        public string OrderId { get; set; }
        public long Amount { get; set; }
        public string VnpResponseCode { get; set; }
        public string VnpTransactionStatus { get; set; }

        public void Set(string code, string message)
        {
            RspCode = code;
            Message = message;
        }
    }
}
