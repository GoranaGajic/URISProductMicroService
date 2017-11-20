using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace URISProductMicroService.Models
{
    public class ProductCategory
    {
        /// <summary>
        /// Product category Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Product category name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Product category description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID of parent category
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Indicator if the category is active
        /// </summary>
        [Required]
        public bool Active { get; set; }
    }
}