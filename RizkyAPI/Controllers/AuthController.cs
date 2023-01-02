using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RizkyAPI.Models;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace RizkyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const int iMaxMinutesToken = 15;
        private readonly ILogger<ItemController> _logger;
        private readonly IConfiguration _conf;
        private readonly TokenValidationParameters _tokenValidParam;

        public AuthController(ILogger<ItemController> logger, IConfiguration conf, TokenValidationParameters tokenValidParam)
        {
            _logger = logger;
            _conf = conf;
            _tokenValidParam = tokenValidParam;
        }

        [AllowAnonymous]
        [Route("GetToken")]
        [HttpPost]
        public async Task<IActionResult> GetToken(UserLogin userDat)
        {
            var userToken = new TokenEntity();
            var dat = await GetUser(userDat);
            if (!string.IsNullOrEmpty(dat.Role))
            {
                userToken = await SetToken(dat);
                return Ok(userToken);
            }
            else
            {
                var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                msg.Content = new StringContent(JsonConvert.SerializeObject(new { Message = "user or password invalid" }));
                //StatusCode(Convert.ToInt32(msg.StatusCode), msg.Content);
                return NotFound(msg);
            }
        }

        [AllowAnonymous]
        [Route("RefreshToken")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken(string sRefreshToken)
        {
            //Cek ke database ada atau tidak
            var userToken = new TokenEntity();
            var lid = await GetIDRefreshTokenFound(sRefreshToken);
            if (lid != 0)
            {
                var dat = await GetUser(lid);
                if (dat != null)
                {
                    userToken = await SetToken(dat);
                    return Ok(userToken);
                }
                else
                {
                    var msg = new HttpResponseMessage(HttpStatusCode.NotFound);
                    msg.Content = new StringContent(JsonConvert.SerializeObject(new { Message = "Refresh Token Not Valid" }));
                    return NotFound(msg);
                }
            }            
            else
            {
                var msg = new HttpResponseMessage(HttpStatusCode.NoContent);
                msg.Content = new StringContent(JsonConvert.SerializeObject(new { Message = "Refresh Token Not Valid" }));
                return NotFound(msg);
            }
        }

        private async Task<TokenEntity> SetToken(UserEntity dat)
        {
            var userToken = new TokenEntity();
            //StatusCode(200, "Get Token Succed");
            var tokenDat = string.Empty;
            var refDat = new RefreshTokenEntity();
            async Task DoAllGenerate()
            {
                tokenDat = await GenerateToken(dat);
                refDat = await GetRefreshToken();
            }
            await Task.WhenAll(DoAllGenerate());
            //return Ok(tokenDat);
            userToken.UserID = dat.Id;
            userToken.UserName = dat.UserName;
            userToken.MainToken = tokenDat;
            userToken.RefreshToken = refDat.RefreshToken;
            userToken.FetchDate = DateTime.Now;
            userToken.ExpiredDate = userToken.FetchDate.AddMinutes(iMaxMinutesToken);
            await CreateRefreshToken(userToken, refDat);

            return userToken;
        }

        private async Task<long> GetIDRefreshTokenFound(string sRefreshToken)
        {
            long returnDat = 0;
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"SELECT TOP 1 UserID FROM dbo.tblRefreshToken WHERE RefreshToken = @RefreshToken AND ExpiredDate >= GETDATE()";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            command.Parameters.AddWithValue("@RefreshToken", sRefreshToken);
            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        returnDat = reader.IsDBNull("UserID") ? 0 : reader.GetInt64(reader.GetOrdinal("UserID"));
                    }
                }
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
            return returnDat;
        }

        private async Task<RefreshTokenEntity> GetRefreshToken()
        {
            var refDat = new RefreshTokenEntity
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedDate = DateTime.Now,
                ExpiredDate = DateTime.Now.AddDays(2)
            };

            return refDat;
        }
        
        private async Task<IActionResult> CreateRefreshToken(TokenEntity tokenDat, RefreshTokenEntity refDat)
        {
            var iCode = 0;
            double dId = 0;
            _logger.LogInformation("Start Set Refresh Token Process");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"spSetRefreshToken";
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
                if (tokenDat.UserID == null)
                {
                    iCode = 404;
                    throw new Exception("UserID cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@UserID", tokenDat.UserID);
                if (string.IsNullOrWhiteSpace(refDat.RefreshToken))
                {
                    iCode = 404;
                    throw new Exception("RefreshToken cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@RefreshToken", refDat.RefreshToken);
                if (refDat.CreatedDate < new DateTime(1999, 1, 1))
                {
                    iCode = 404;
                    throw new Exception("CreatedDate cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@CreatedDate", refDat.CreatedDate);
                if (refDat.ExpiredDate < new DateTime(1999, 1, 1))
                {
                    iCode = 404;
                    throw new Exception("ExpiredDate cant be Empty");
                }
                else
                    command.Parameters.AddWithValue("@ExpiredDate", refDat.ExpiredDate);
                await command.ExecuteScalarAsync();
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
                return StatusCode(iCode, "Set Refresh Token Process Failed : " + ex.Message);
            }
            return StatusCode(iCode, "Set Refresh Token Succeed, ID : " + dId.ToString());
        }


        private async Task<bool> IsTokenValid(string sToken)
        {
            var returnDat = false;

            return returnDat;
        }

        private ClaimsPrincipal GetPrincipal(string sToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principalDat = tokenHandler.ValidateToken(sToken, _tokenValidParam, out var validatedToken);
                if (
                    !(validatedToken is JwtSecurityToken jst && jst.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    )
                {
                    return null;
                }
                return principalDat;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Get Principal Error : " + ex.Message);
                return null;
            }
        }


        [AllowAnonymous]
        [Route("ReadToken")]
        [HttpPost]
        public async Task<IActionResult> ReadUserToken(string sToken)
        {
            var iIdentity = GetPrincipal(sToken);
            var userDat = new UserEntity();
            if (iIdentity.Claims.Count() <= 0)
            {
                return NotFound(userDat);
            }
            else
            {
                userDat = await GetUserEntity(iIdentity);
                return Ok(userDat);
            }
        }

        [AllowAnonymous]
        [Route("ReadCurrentUser")]
        [HttpPost]
        public async Task<IActionResult> ReadCurrentUser()
        {
            //string emailMember = User.FindFirst(ClaimTypes.Email)?.Value;
            var iIdentity = User;
            var userDat = new UserEntity();
            if (iIdentity.Claims.Count() <= 0)
            {
                return NotFound(userDat);
            }
            else
            {
                userDat = await GetUserEntity(iIdentity);
                return Ok(userDat);
            }
        }

        private async Task<UserEntity> GetUserEntity(ClaimsPrincipal iIdentity)
        {
            var userDat = new UserEntity();
            var userClaims = iIdentity.Claims;
            userDat.Id = Convert.ToInt64(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Sid)?.Value);
            userDat.UserName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
            userDat.SureName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value;
            userDat.Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
            userDat.Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;
            return userDat;
        }

        private async Task<string> GenerateToken(UserEntity userDat)
        {
            var returnDat = string.Empty;
            _logger.LogInformation("Start Generate Token");
            var secureityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["JWTConfig:Key"]));
            var credentialSign = new SigningCredentials(secureityKey, SecurityAlgorithms.HmacSha256);
            var expDat = DateTime.Now.AddMinutes(iMaxMinutesToken);
            var claims = new[]
            {
                new Claim(ClaimTypes.Sid, userDat.Id.ToString()),
                new Claim(ClaimTypes.Role, userDat.Role),
                new Claim(ClaimTypes.Surname, userDat.SureName),
                new Claim(ClaimTypes.NameIdentifier, userDat.UserName),
                new Claim(ClaimTypes.Email, userDat.Email),
                new Claim(ClaimTypes.Expired, expDat.ToString())
            };
            //var token = new JwtSecurityToken(_conf["JWTConfig:Issuer"], _conf["JWTConfig:Audience"],
            //    claims: claims, expires:DateTime.Now.AddMinutes(15), signingCredentials: credentialSign
            //    );
            var token = new JwtSecurityToken(claims: claims, expires: expDat, signingCredentials: credentialSign
                );

            returnDat = new JwtSecurityTokenHandler().WriteToken(token);

            return returnDat;
        }
        private async Task<UserEntity> GetUser(UserLogin userDat)
        {
            _logger.LogInformation("Start Check User");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Select * from tblUser where UserName=@UserName AND Password=@Password";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            var returnDat = new UserEntity();
            try
            {
                await connection.OpenAsync();
                command.Parameters.AddWithValue("@UserName", userDat.UserName);
                command.Parameters.AddWithValue("@Password", userDat.Password);
                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        returnDat.Id = reader.IsDBNull("Id") ? 0 : reader.GetInt64(reader.GetOrdinal("Id"));
                        returnDat.SureName = reader.IsDBNull("SureName") ? string.Empty : reader.GetString(reader.GetOrdinal("SureName"));
                        returnDat.UserName = reader.IsDBNull("UserName") ? string.Empty : reader.GetString(reader.GetOrdinal("UserName"));
                        returnDat.Password = reader.IsDBNull("Password") ? string.Empty : reader.GetString(reader.GetOrdinal("Password"));
                        returnDat.Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString(reader.GetOrdinal("Email"));
                        returnDat.Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString(reader.GetOrdinal("Phone"));
                        returnDat.Role = reader.IsDBNull("Role") ? string.Empty : reader.GetString(reader.GetOrdinal("Role"));
                        returnDat.Note = reader.IsDBNull("Note") ? string.Empty : reader.GetString(reader.GetOrdinal("Note"));
                        returnDat.Status = reader.IsDBNull("Status") ? string.Empty : reader.GetString(reader.GetOrdinal("Status"));
                    }
                }
                await reader.CloseAsync();
                await connection.CloseAsync();
            }
            catch
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
            return returnDat;
        }

        private async Task<UserEntity> GetUser(long lId)
        {
            _logger.LogInformation("Start Check User");
            var sConn = _conf.GetConnectionString("DataConn");
            var sQuery = @"Select * from tblUser where Id=@Id";
            using var connection = new SqlConnection(sConn);
            using var command = new SqlCommand(sQuery, connection);
            var returnDat = new UserEntity();
            try
            {
                await connection.OpenAsync();
                command.Parameters.AddWithValue("@Id", lId);
                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        returnDat.Id = reader.IsDBNull("Id") ? 0 : reader.GetInt64(reader.GetOrdinal("Id"));
                        returnDat.SureName = reader.IsDBNull("SureName") ? string.Empty : reader.GetString(reader.GetOrdinal("SureName"));
                        returnDat.UserName = reader.IsDBNull("UserName") ? string.Empty : reader.GetString(reader.GetOrdinal("UserName"));
                        returnDat.Password = reader.IsDBNull("Password") ? string.Empty : reader.GetString(reader.GetOrdinal("Password"));
                        returnDat.Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString(reader.GetOrdinal("Email"));
                        returnDat.Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString(reader.GetOrdinal("Phone"));
                        returnDat.Role = reader.IsDBNull("Role") ? string.Empty : reader.GetString(reader.GetOrdinal("Role"));
                        returnDat.Note = reader.IsDBNull("Note") ? string.Empty : reader.GetString(reader.GetOrdinal("Note"));
                        returnDat.Status = reader.IsDBNull("Status") ? string.Empty : reader.GetString(reader.GetOrdinal("Status"));
                    }
                }
                await reader.CloseAsync();
                await connection.CloseAsync();
            }
            catch
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
            return returnDat;
        }

        //[AllowAnonymous]
        //[Route("api/gettoken")]
        //[HttpPost]
        //public async Task<string> GetToken(SCH_UsersAPI userDat)
        //{
        //    var dat = await CheckUser(userDat.username, userDat.password);
        //    if (!string.IsNullOrEmpty(dat.role))
        //    {
        //        return await JWTProcess.GenerateToken(dat);
        //    }
        //    else
        //    {
        //        var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //        msg.Content = new StringContent(JsonConvert.SerializeObject(new { Message = "user or password invalid" }));
        //        throw new HttpResponseException(msg);
        //    }
        //}

        //public async Task<SCH_UsersAPI> CheckUser(string username, string password)
        //{
        //    var returnData = new SCH_UsersAPI();
        //    try
        //    {
        //        using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MasterConnectionString"].ConnectionString))
        //        {
        //            await connection.OpenAsync();

        //            //using (var command = connection.CreateCommand())
        //            //{
        //            //    command.CommandText = @"select count(*) from SCH_UsersAPI where username=@username and password=@password";
        //            //    command.Parameters.AddWithValue("@username", username);
        //            //    command.Parameters.AddWithValue("@password", password);
        //            //    returnData = (int)command.ExecuteScalar() > 0 ? true : false;
        //            //}

        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = @"select username, role from SCH_UsersAPI where username=@username and password=@password;";
        //                command.Parameters.AddWithValue("@username", username);
        //                command.Parameters.AddWithValue("@password", password);
        //                using (var reader = await command.ExecuteReaderAsync())
        //                {
        //                    while (await reader.ReadAsync())
        //                    {
        //                        if (!(reader["username"] is DBNull))
        //                        {
        //                            returnData.username = reader["username"].ToString();
        //                        }
        //                        if (!(reader["role"] is DBNull))
        //                        {
        //                            returnData.role = reader["role"].ToString();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //        message.Content = new StringContent(ex.ToString());
        //        //throw new HttpResponseException(message);
        //        throw new HttpResponseException(HttpStatusCode.InternalServerError);
        //    }
        //    return returnData;
        //}
    }
}
