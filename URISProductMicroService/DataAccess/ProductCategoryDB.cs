using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using URISProductMicroService.Models;
using URISUtil.DataAccess;
using URISUtil.Logging;
using URISUtil.Response;

namespace URISProductMicroService.DataAccess
{
    public static class ProductCategoryDB
    {
        private static ProductCategory ReadRow(SqlDataReader reader, string table = "ProductCategory")
        {
            ProductCategory retVal = new ProductCategory();

            retVal.Id = (int)reader[$"{ table }_Id"];
            retVal.Name = reader[$"{ table }_Name"] as string;
            retVal.Description = reader[$"{ table }_Description"] as string;
            retVal.ParentCategoryId = reader[$"{ table }_ParentCategoryId"] as int?;
            retVal.Active = (bool)reader[$"{ table }_Active"];

            return retVal;
        }

        private static int ReadId(SqlDataReader reader)
        {
            return (int)reader["Id"];
        }

        private static void FillData(SqlCommand command, ProductCategory productCategory)
        {
            command.AddParameter("@Id", SqlDbType.Int, productCategory.Id);
            command.AddParameter("@Name", SqlDbType.NVarChar, productCategory.Name);
            command.AddParameter("@Description", SqlDbType.NVarChar, productCategory.Description);           
            command.AddParameter("@ParentCategoryId", SqlDbType.Int, productCategory.ParentCategoryId);
            command.AddParameter("@Active", SqlDbType.Bit, productCategory.Active);
        }

        private static string AllColumnSelect(string table = "ProductCategory")
        {
            return $@"
                [{ table }].[Id] AS [{ table }_Id],
                [{ table }].[Name] AS [{ table }_Name],
                [{ table }].[Description] AS [{ table }_Description],
                [{ table }].[ParentCategoryId] AS [{ table }_ParentCategoryId],
                [{ table }].[Active] AS [{ table }_Active]
            ";
        }

        private static object CreateLikeQueryString(string str)
        {
            return str == null ? (object)DBNull.Value : "%" + str + "%";
        }

        public static List<ProductCategory> GetProductCategories(int? parentId, ActiveStatusEnum active)
        {
            try
            {
                List<ProductCategory> retVal = new List<ProductCategory>();

                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();                   

                    command.CommandText = $@"
                        SELECT
                            { AllColumnSelect() }
                        FROM
                            [product].[ProductCategory]
                        WHERE
                            (@Active IS NULL OR [product].[ProductCategory].Active = @Active) AND
                            (@ParentCategoryId IS NULL OR [product].[ProductCategory].ParentCategoryId = @ParentCategoryId)
                    ";
                    command.Parameters.Add("@ParentCategoryId", SqlDbType.Int);
                    if (parentId != null)
                    {                        
                        command.Parameters["@ParentCategoryId"].Value = parentId;
                    }
                    else
                    {
                        command.Parameters["@ParentCategoryId"].Value = DBNull.Value;
                    }
                    command.Parameters.Add("@Active", SqlDbType.Bit);
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

        public static ProductCategory GetProductCategory(int id)
        {
            try
            {
                ProductCategory result = null;

                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();

                    command.CommandText = $@"
                        SELECT
                            { AllColumnSelect() }
                        FROM
                            [product].[ProductCategory]
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

        public static ProductCategory CreateProductCategory(ProductCategory productCategory)
        {
            int id = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"

                        INSERT INTO [product].[ProductCategory]
                        (
	                        [Name],
	                        [Description],
	                        [ParentCategoryId],
	                        [Active]
                        )
                        VALUES
                        (
	                        @Name,
	                        @Description,
	                        @ParentCategoryId,
	                        @Active
                        )
                        SET @Id = SCOPE_IDENTITY();
						SELECT @Id as Id
                    ");

                    FillData(command, productCategory);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = ReadId(reader);
                        }
                    }
                    return GetProductCategory(id);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static ProductCategory UpdateProductCategory(ProductCategory productCategory)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"

                        UPDATE
                            [product].[ProductCategory]
                        SET
	                        [Name] = @Name,
	                        [Description] = @Description,
	                        [ParentCategoryId] = @ParentCategoryId,
	                        [Active] = @Active
                        WHERE
                            [Id] = @Id
                    ");

                    FillData(command, productCategory);
                    connection.Open();
                    command.ExecuteNonQuery();

                    return GetProductCategory(productCategory.Id.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static void DeleteProductCategory(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"
                        UPDATE
                            product.ProductCategory
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
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}