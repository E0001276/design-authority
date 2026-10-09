using DesignAuthority.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace DesignAuthority.Infrastructure.Configuration;

public sealed class ConfiguredUserContext : IConfiguredUserContext
{
    public ConfiguredUserContext(IConfiguration configuration)
    {
        UserId = configuration.GetValue<int?>("ConfiguredUser:Id")
            ?? throw new InvalidOperationException("ConfiguredUser:Id must be configured.");
        if (UserId <= 0) throw new InvalidOperationException("ConfiguredUser:Id must be positive.");
    }

    public int UserId { get; }
}
