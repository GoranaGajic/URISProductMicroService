using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace URISProductMicroService.Models
{
    public class Product
    {
        /// <summary>
        /// Produtct Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Product price
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// Default quantity of a product
        /// </summary>
        public decimal? DefaultQuantity { get; set; }

        /// <summary>
        /// Minimal quantity of a product that can be ordered
        /// </summary>
        public decimal? MinQuantity { get; set; }

        /// <summary>
        /// Maximum quantity of a product that can be ordered
        /// </summary>
        public decimal? MaxQuantity { get; set; }

        /// <summary>
        /// ID of parent product or service
        /// </summary>
        public int? ParentProductId { get; set; }

        /// <summary>
        /// ID of product category
        /// </summary>
        [Required]
        public int ProductCategoryId { get; set; }

        /// <summary>
        /// Indicator if product is active
        /// </summary>
        [Required]
        public bool Active { get; set; }
    }
}