using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using MySql.Data;
using MySql.Data.MySqlClient;

using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private MySqlConnection _connection;
        
        private string _connectionStrings;
        
        public IConfiguration Configuration { get; }

        public CategoriesController(IConfiguration configuration)
        {
            _connectionStrings = configuration["ConnectionStrings:Default"];
            _connection = new MySqlConnection(_connectionStrings);
        }

        [HttpGet("CategoryList")]
        public async Task<IActionResult> GetCategoryList(int pageNo, int pageRow)
        {
            List<CategoryListResponse> categoryList = new List<CategoryListResponse>();
            MySqlCommand cmd = new MySqlCommand("GetCategoryList", _connection);
            cmd.CommandType = CommandType.StoredProcedure;
            _connection.Open();

            cmd.Parameters.Add(new MySqlParameter("pageNo", pageNo == 0 ? 1 : pageNo));
            cmd.Parameters["pageNo"].Direction = ParameterDirection.InputOutput;
            cmd.Parameters.Add(new MySqlParameter("pageRow", pageRow == 0 ? 10 : pageRow));
            cmd.Parameters["pageRow"].Direction = ParameterDirection.InputOutput;
            cmd.Parameters.Add(new MySqlParameter("totalRecord", System.Data.SqlDbType.Int));
            cmd.Parameters["totalRecord"].Direction = ParameterDirection.Output;

            using var reader = await cmd.ExecuteReaderAsync();
        
            while (await reader.ReadAsync())
            {
                CategoryListResponse categoryResponse = new CategoryListResponse();
                categoryResponse.categoryID = Convert.ToInt32(reader["categoryID"]);
                categoryResponse.categoryNameTH = reader["categoryNameTH"].ToString();
                categoryResponse.categoryNameEN = reader["categoryNameEN"].ToString();
                categoryList.Add(categoryResponse);
            }
                
            int count = categoryList.Count();
            Console.WriteLine(count);

            reader.Close();

            return Ok(
                new {
                    pageNo = pageNo == 0 ? 1 : pageNo,
                    pageRow = pageRow == 0 ? 10 : pageRow,
                    totalRecord = count,
                    data = categoryList
                }
            );
        }
    }
}
