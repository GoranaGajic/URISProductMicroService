using System.Collections.Generic;
using System.Web.Http;
using URISProductMicroService.DataAccess;
using URISProductMicroService.Models;
using URISUtil.DataAccess;

namespace URISProductMicroService.Controllers
{
    public class ProductController : ApiController
    {
        /// <summary>
        /// Get all products based on filters
        /// </summary>
        /// <param name="parentId">Id of parent product</param>
        /// <param name="productCategoryId">Id of product category</param>
        /// <param name="filter">String filter</param>
        /// <param name="active">Indicates if the product is active or not</param>
        /// <param name="order">Ordering</param>
        /// <param name="orderDirection">Order direction</param>
        /// <returns>List of products</returns>
        [Route("api/Product"), HttpGet]
        public List<Product> GetProducts([FromUri]int? parentId = null, [FromUri]int? productCategoryId = null, [FromUri]string filter = null, [FromUri]ActiveStatusEnum active = ActiveStatusEnum.Active)
        {
            return ProductDB.GetProducts(parentId, productCategoryId, filter, active);
        }

        /// <summary>
        /// Get single product based on id
        /// </summary>
        /// <param name="id">Product id</param>
        /// <returns>Single product</returns>
        [Route("api/Product/{id}"), HttpGet]
        public Product GetProduct([FromUri]int id)
        {
            return ProductDB.GetProduct(id);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="product">Product as json</param>
        /// <returns>Created product</returns>
        [Route("api/Product"), HttpPost]
        public Product CreateProduct([FromBody]Product product)
        {
            return ProductDB.CreateProduct(product);
        }

        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="product">Product as json</param>
        /// <returns>Updated product</returns>
        [Route("api/Product"), HttpPut]
        public Product UpdateProduct([FromBody]Product product)
        {
            return ProductDB.UpdateProduct(product);
        }

        /// <summary>
        /// Deleta a product
        /// </summary>
        /// <param name="id"></param>
        [Route("api/Product/{id}"), HttpDelete]
        public void DeleteProduct([FromUri]int id)
        {
            ProductDB.DeleteProduct(id);
        }
    }
}