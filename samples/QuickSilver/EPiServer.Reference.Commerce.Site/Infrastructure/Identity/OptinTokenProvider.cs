using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Identity
{
    public class OptinTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public OptinTokenProvider(IDataProtectionProvider dataProtectionProvider, 
            IOptions<OptinTokenProviderOptions> options,
            ILogger<OptinTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class OptinTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public const string TokenProviderName = "OptinTokenProvider";

        public OptinTokenProviderOptions()
        {
            // update the defaults
            Name = TokenProviderName;
            TokenLifespan = TimeSpan.FromMinutes(15);
        }
    }
}
