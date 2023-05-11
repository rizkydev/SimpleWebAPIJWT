using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RizkyAPI.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RizkyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "administrator,rpx")]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;
        private readonly IConfiguration _conf;
        private readonly IWebHostEnvironment _env;

        public ItemController(ILogger<ItemController> logger, IConfiguration conf, IWebHostEnvironment env)
        {
            _logger = logger;
            _conf = conf;
            _env = env;
        }

        //[Route("GetAllItems")] //Untuk ganti nama API
        [HttpGet]
        //public async Task<IActionResult> GetAllItems()
        //public async Task<JsonResult> GetAllItems()
        [Authorize(Roles = "admin,user")]
        public async Task<List<ItemEntity>> GetAllItems()
        {
            _logger.LogInformation("Start Get All Items Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Select Id, ItemCode, ItemName, ItemDesc From tblItem Order By Id";
            //var sQuery = @"Select * from vGetListItems"; // Get from Views

            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);

            var listDat = new List<ItemEntity>();

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new ItemEntity(); 
                        item.Id = reader.IsDBNull("Id") ? 0 : reader.GetInt64(reader.GetOrdinal("Id"));
                        item.ItemName = reader.IsDBNull("ItemName") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemName"));
                        item.ItemCode = reader.IsDBNull("ItemCode") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemCode"));
                        item.ItemDesc = reader.IsDBNull("ItemDesc") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemDesc"));
                        listDat.Add(item);
                    }
                }
                //await Task.WhenAll((IEnumerable<Task>)listDat);
                await reader.CloseAsync();
                await connection.CloseAsync();
            }
            catch
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                //return UnprocessableEntity(listDat); //Untuk IActionResult
            }
            //return Ok(listDat); //Untuk IActionResult
            //return new JsonResult(listDat); //Untuk JsonResult
            return listDat; //Untuk List<ItemEntity>
        }

        [Route("GetByID")] //Untuk ganti nama API
        [HttpGet]
        //public async Task<IActionResult> GetAllItems()
        //public async Task<JsonResult> GetAllItems()
        //[Authorize(Roles = "admin,user")]
        public async Task<ItemEntity> GetItemByID(long Id)
        {
            _logger.LogInformation("Start Get Items By ID Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Select Top 1 Id, ItemCode, ItemName, ItemDesc From tblItem Where @Id = Id";
            //var sQuery = @"Select * from vGetListItems"; // Get from Views

            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);

            var ItemDat = new ItemEntity();

            try
            {
                await connection.OpenAsync();
                command.Parameters.AddWithValue("@Id", Id);
                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        ItemDat.Id = reader.IsDBNull("Id") ? 0 : reader.GetInt64(reader.GetOrdinal("Id"));
                        ItemDat.ItemName = reader.IsDBNull("ItemName") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemName"));
                        ItemDat.ItemCode = reader.IsDBNull("ItemCode") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemCode"));
                        ItemDat.ItemDesc = reader.IsDBNull("ItemDesc") ? string.Empty : reader.GetString(reader.GetOrdinal("ItemDesc"));
                    }
                }
                //await Task.WhenAll((IEnumerable<Task>)listDat);
                await reader.CloseAsync();
                await connection.CloseAsync();
            }
            catch
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                ItemDat = new ItemEntity();
            }
            return ItemDat; 
        }


        [HttpPost]
        //[Authorize(Roles = "admin,user")]
        public async Task<IActionResult> CreateItem(ItemEntity dat)
        {
            var iCode = 0;
            _logger.LogInformation("Start Create Items Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Insert Into tblItem (ItemName, ItemCode, ItemDesc) values (@ItemName, @ItemCode, @ItemDesc); SELECT SCOPE_IDENTITY();";
            double dId = 0;
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            try
            {
                await connection.OpenAsync();
                if (string.IsNullOrWhiteSpace(dat.ItemCode))
                {
                    iCode = 404;
                    throw new Exception("ItemCode cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemCode", dat.ItemCode);
                if (string.IsNullOrWhiteSpace(dat.ItemName))
                {
                    iCode = 404;
                    throw new Exception("ItemName cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemName", dat.ItemName);
                //command.Parameters.AddWithValue("@ItemName", string.IsNullOrWhiteSpace(dat.ItemName) ? string.Empty : dat.ItemName);
                //command.Parameters.AddWithValue("@ItemCode", string.IsNullOrWhiteSpace(dat.ItemCode) ? string.Empty : dat.ItemCode);
                command.Parameters.AddWithValue("@ItemDesc", string.IsNullOrWhiteSpace(dat.ItemDesc) ? string.Empty : dat.ItemDesc);
                dId =  Convert.ToDouble(await command.ExecuteScalarAsync());
                await connection.CloseAsync();
                iCode = 200;
            }
            catch(Exception ex)
            {
                iCode = iCode == 0 ? 505 : iCode;
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                return StatusCode(iCode, "Insert Process Failed" + ex.Message);
            }
            return StatusCode(iCode, "Process Succeed, ID : " + dId.ToString());
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateItem(ItemEntity dat)
        {
            var iCode = 0;
            _logger.LogInformation("Start Update Items Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Update tblItem Set ItemName=@ItemName, ItemCode=@ItemCode, ItemDesc=@ItemDesc where Id=@Id";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            try
            {
                await connection.OpenAsync();
                if (dat.Id == null || dat.Id <= 0)
                {
                    iCode = 404;
                    throw new Exception("Id cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@Id", dat.Id);
                if (string.IsNullOrWhiteSpace(dat.ItemCode))
                {
                    iCode = 404;
                    throw new Exception("ItemCode cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemCode", dat.ItemCode);
                if (string.IsNullOrWhiteSpace(dat.ItemName))
                {
                    iCode = 404;
                    throw new Exception("ItemName cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemName", dat.ItemName);
                command.Parameters.AddWithValue("@ItemDesc", string.IsNullOrWhiteSpace(dat.ItemDesc) ? string.Empty : dat.ItemDesc);
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
                iCode = 200;
            }
            catch (Exception ex)
            {
                iCode = iCode == 0 ? 505 : iCode;
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                return StatusCode(iCode, "Update Process Failed : " + ex.Message);
            }
            return StatusCode(iCode, "Process Succeed ");
        }

        [Route("SetItem")]
        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SetItem(ItemEntity dat)
        {
            var iCode = 0;
            double dId = 0;
            _logger.LogInformation("Start Set Items Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"spSetItem";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            var transaction = connection.BeginTransaction();
            command.Transaction = transaction;
            try
            {
                if (dat.Id == null || dat.Id <= 0)
                {
                    command.Parameters.AddWithValue("@Id", 0);
                }
                else
                    command.Parameters.AddWithValue("@Id", dat.Id);
                if (string.IsNullOrWhiteSpace(dat.ItemCode))
                {
                    iCode = 404;
                    throw new Exception("ItemCode cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemCode", dat.ItemCode);
                if (string.IsNullOrWhiteSpace(dat.ItemName))
                {
                    iCode = 404;
                    throw new Exception("ItemName cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ItemName", dat.ItemName);
                command.Parameters.AddWithValue("@ItemDesc", string.IsNullOrWhiteSpace(dat.ItemDesc) ? string.Empty : dat.ItemDesc);
                dId = Convert.ToDouble(await command.ExecuteScalarAsync());
                transaction.Commit();
                await connection.CloseAsync();
                iCode = 200;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                iCode = iCode == 0 ? 505 : iCode;
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                return StatusCode(iCode, "Update Process Failed : " + ex.Message);
            }
            return StatusCode(iCode, "Process Succeed, ID : " + dId.ToString());
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMahasiswa(Int64 Id)
        {
            var iCode = 0;
            _logger.LogInformation("Start Set Items Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Delete tblItem WHERE Id = @Id";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            var transaction = connection.BeginTransaction();
            command.Transaction = transaction;
            try
            {
                if (Id == null || Id <= 0)
                {
                    iCode = 404;
                    throw new Exception("Id cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@Id", Id);
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                await connection.CloseAsync();
                iCode = 200;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                iCode = iCode == 0 ? 505 : iCode;
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                return StatusCode(iCode, "Delete Process Failed : " + ex.Message);
            }
            return StatusCode(iCode, "Process Succeed, Delete ID : " + Id.ToString());
        }

        //[Route("SaveFile")]
        //[HttpPost]
        //public JsonResult SavePhoto()
        //{
        //    try
        //    {
        //        var httpReq = Request.Form;
        //        var postedFile = httpReq.Files[0];
        //        var fileName = postedFile.FileName;
        //        var phisicPath = _env.ContentRootPath + "/Photos/" + fileName;
        //        using (var streamDat = new FileStream(phisicPath, FileMode.Create))
        //        {
        //            postedFile.CopyTo(streamDat);
        //        }
        //        return new JsonResult(fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult(ex.Message);
        //    }
        //}

    }
}
