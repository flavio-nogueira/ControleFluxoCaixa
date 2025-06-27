using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleFluxoCaixa.Application.DTOs
{
    public class RateLimitingSettings
    {
        public int PermitLimit { get; set; }
        public int WindowInMinutes { get; set; }
        public int QueueLimit { get; set; }
    }

}
