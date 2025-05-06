using System.Collections.Generic;

namespace Accessory.Builder.WebApi.Common;

public class WebApiOptions
{
    public ICollection<string>? CorsAllowedOrigins { get; set; }
}