using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using URISProductMicroService.DataAccess;
using URISProductMicroService.Models;
using URISUtil.DataAccess;

namespace URISProductMicroService.Controllers
{
    public class ProductCategoryController : ApiController
    {
        /// <summary>
        /// Get all product categories based on filters
        /// </summary>
        /// <param name="parentId">Id of parent category</param>
        /// <param name="active">Indicates if the product category is active or not</param>
        /// <returns>List of product categories</returns>
        [Route("api/ProductCategory"), HttpGet]
        public IEnumerable<ProductCategory> GetProductCategories([FromUri]int? parentId = null, [FromUri]ActiveStatusEnum active = ActiveStatusEnum.Active)
        {
            return ProductCategoryDB.GetProductCategories(parentId, active);
        }

        /// <summary>
        /// Gets single product category based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Single product category</returns>
        [Route("api/ProductCategory/{id}"), HttpGet]
        public ProductCategory GetProductCategory([FromUri]int id)
        {
            return ProductCategoryDB.GetProductCategory(id);
        }

        /// <summary>
        /// Create a new product category
        /// </summary>
        /// <param name="productCategory">Product category as json</param>
        /// <returns>Created product category</returns>
        [Route("api/ProductCategory"), HttpPost]
        public ProductCategory CreateProductCategory([FromBody]ProductCategory productCategory)
        {
            return ProductCategoryDB.CreateProductCategory(productCategory);
        }

        /// <summary>
        /// Update a product category
        /// </summary>
        /// <param name="productCategory">Product category as json</param>
        /// <returns>Updated product category</returns>
        [Route("api/ProductCategory"), HttpPut]
        public ProductCategory UpdateProductCategory([FromBody]ProductCategory productCategory)
        {
            return ProductCategoryDB.UpdateProductCategory(productCategory);
        }

        /// <summary>
        /// Delete a product category
        /// </summary>
        /// <param name="id">Id of product category to be deleted</param>
        [Route("api/ProductCategory/{id}"), HttpDelete]
        public void DeleteProductCategory([FromUri]int id)
        {
            ProductCategoryDB.DeleteProductCategory(id);
        }
    }
}