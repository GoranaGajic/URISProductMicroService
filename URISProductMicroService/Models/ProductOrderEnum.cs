using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace URISProductMicroService.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductOrderEnum
    {
        Id,
        Code,
        Name,
        Price,
        ParentProductId,
        ProductCategoryId
    }
}