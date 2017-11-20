using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using URISProductMicroService.Models;
using URISUtil.DataAccess;
using URISUtil.Logging;
using URISUtil.Response;

namespace URISProductMicroService.DataAccess
{
    public static class ProductDB
    {

        public static Product ReadRow(SqlDataReader reader, string table = "Product")
        {
            Product retVal = new Product();

            retVal.Id = (int)reader[$"{ table }_Id"];
            retVal.Name = reader[$"{ table }_Name"] as string;
            retVal.Description = reader[$"{ table }_Description"] as string;
            retVal.Price = (decimal)reader[$"{ table }_Price"];
            retVal.DefaultQuantity = reader[$"{ table }_DefaultQuantity"] as decimal?;
            retVal.MinQuantity = reader[$"{ table }_MinQuantity"] as decimal?;
            retVal.MaxQuantity = reader[$"{ table }_MaxQuantity"] as decimal?;
            retVal.ParentProductId = reader[$"{ table }_ParentProductId"] as int?;
            retVal.ProductCategoryId = (int)reader[$"{ table }_ProductCategoryId"];
            retVal.Active = (bool)reader[$"{ table }_Active"];

            return retVal;
        }

        private static int ReadId(SqlDataReader reader)
        {
            return (int)reader["Id"];
        }

        private static string AllColumnSelect(string table = "Product")
        {
            return $@"
                [{ table }].[Id] AS [{ table }_Id],
                [{ table }].[Name] AS [{ table }_Name],
                [{ table }].[Description] AS [{ table }_Description],
                [{ table }].[Price] AS [{ table }_Price],
                [{ table }].[DefaultQuantity] AS [{ table }_DefaultQuantity],
                [{ table }].[MinQuantity] AS [{ table }_MinQuantity],
                [{ table }].[MaxQuantity] AS [{ table }_MaxQuantity],
                [{ table }].[ParentProductId] AS [{ table }_ParentProductId],
                [{ table }].[ProductCategoryId] AS [{ table }_ProductCategoryId],
                [{ table }].[Active] AS [{ table }_Active]
            ";
        }

        private static void FillData(SqlCommand command, Product product)
        {
            command.AddParameter("@Id", SqlDbType.Int, product.Id);
            command.AddParameter("@Name", SqlDbType.NVarChar, product.Name);
            command.AddParameter("@Description", SqlDbType.NVarChar, product.Description);
            command.AddParameter("@Price", SqlDbType.Decimal, product.Price);
            command.AddParameter("@DefaultQuantity", SqlDbType.Decimal, product.DefaultQuantity);
            command.AddParameter("@MinQuantity", SqlDbType.Decimal, product.MinQuantity);
            command.AddParameter("@MaxQuantity", SqlDbType.Decimal, product.MaxQuantity);
            command.AddParameter("@ParentProductId", SqlDbType.Int, product.ParentProductId);
            command.AddParameter("@ProductCategoryId", SqlDbType.Int, product.ProductCategoryId);
            command.AddParameter("@Active", SqlDbType.Bit, product.Active);
        }

        private static object CreateLikeQueryString(string str)
        {
            return str == null ? (object)DBNull.Value : "%" + str + "%";
        }

        public static List<Product> GetProducts(int? parentId, int? productCategoryId, string filter, ActiveStatusEnum active)
        {
            try
            {
                List<Product> retVal = new List<Product>();

                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    string addFilter = "";
                    SqlCommand command = connection.CreateCommand();

                    command.Parameters.Add("@Filter", SqlDbType.NVarChar);
                    if (!String.IsNullOrEmpty(filter))
                    {
                        addFilter = @" AND ([Product].[Name] LIKE '%' + @Filter + '%' OR
                                            [Product].[Description] LIKE '%' + @Filter + '%')";
                        command.Parameters["@Filter"].Value = filter;
                    }
                    else
                    {
                        command.Parameters["@Filter"].Value = DBNull.Value;
                    }

                    command.CommandText = $@"
                        SELECT
                            { AllColumnSelect() }
                        FROM
                            [product].[Product]
                        WHERE
                            (@ParentProductId IS NULL OR [product].[Product].ParentProductId = @ParentProductId) AND
                            (@ProductCategoryId IS NULL OR [product].[Product].ProductCategoryId = @ProductCategoryId) AND
                            (@Active IS NULL OR [product].[Product].Active = @Active)
                            { addFilter }
                    ";

                    command.Parameters.Add("@ParentProductId", SqlDbType.Int);
                    command.Parameters.Add("@ProductCategoryId", SqlDbType.Int);
                    command.Parameters.Add("@Active", SqlDbType.Bit);                    

                    if (parentId != null)
                    {
                        command.Parameters["@ParentProductId"].Value = parentId;
                    }
                    else
                    {
                        command.Parameters["@ParentProductId"].Value = DBNull.Value;
                    }

                    if (productCategoryId != null)
                    {
                        command.Parameters["@ProductCategoryId"].Value = productCategoryId;
                    }
                    else
                    {
                        command.Parameters["@ProductCategoryId"].Value = DBNull.Value;
                    }
                    
                    switch (active)
                    {
                        case ActiveStatusEnum.Active:
                            command.Parameters["@Active"].Value = true;
                            break;
                        case ActiveStatusEnum.Inactive:
                            command.Parameters["@Active"].Value = false;
                            break;
                        case ActiveStatusEnum.All:
                            command.Parameters["@Active"].Value = DBNull.Value;
                            break;
                    }                    

                    
                    

                    System.Diagnostics.Debug.WriteLine(command.CommandText);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retVal.Add(ReadRow(reader));
                        }
                    }
                }

                return retVal;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static Product GetProduct(int id)
        {
            try
            {
                Product result = null;

                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();

                    command.CommandText = $@"
                        SELECT
                            { AllColumnSelect() }
                        FROM
                            [product].[Product]
                        WHERE
                            [Id] = @Id
                    ";
                    command.Parameters.Add("@Id", SqlDbType.Int);
                    command.Parameters["@Id"].Value = id;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = ReadRow(reader);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static Product CreateProduct(Product product)
        {
            int id = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"

                        INSERT INTO product.Product
                        (
                            [Name],
                            [Description],
                            [Price],
                            [DefaultQuantity],
                            [MinQuantity],
                            [MaxQuantity],
                            [ParentProductId],
                            [ProductCategoryId],
                            [Active]               
                        )
                        VALUES
                        (
                            @Name,
                            @Description,
                            @Price,
                            @DefaultQuantity,
                            @MinQuantity,
                            @MaxQuantity,
                            @ParentProductId,
                            @ProductCategoryId,
                            @Active   
                        )
                        SET @Id = SCOPE_IDENTITY();
						SELECT @Id as [Id]  
                    ");
                    FillData(command, product);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = ReadId(reader);
                        }
                    }
                    return GetProduct(id);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static Product UpdateProduct(Product product)
        {
            if (product.Id == product.ParentProductId)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Product cannot be parent product to itself.") };
                throw new HttpResponseException(resp);
            };

            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"      

                        UPDATE product.Product
                        SET [Name] = @Name,
                            [Description] = @Description,
                            [Price] = @Price,
                            [DefaultQuantity] = @DefaultQuantity,
                            [MinQuantity] = @MinQuantity,
                            [MaxQuantity] = @MaxQuantity,
                            [ParentProductId] = @ParentProductId,
                            [ProductCategoryId] = @ProductCategoryId,
                            [Active] = @Active 
                        WHERE Id = @Id
                                                 
                    ");
                    FillData(command, product);
                    connection.Open();
                    command.ExecuteNonQuery();

                    return GetProduct(product.Id.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static void DeleteProduct(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"

                        UPDATE
                            product.Product
                        SET
                            Active = 'False'
                        WHERE
                            Id = @Id
                    ");

                    command.AddParameter("@Id", SqlDbType.Int, id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, string.Format("{0} = {1}", ex.Message, id));
            }
        }
    }
}