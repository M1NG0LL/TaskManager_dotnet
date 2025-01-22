using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskManagerAPI.Token;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;


namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Context _context;
        private readonly TokenHelper _tokenHelper;

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return HashPassword(inputPassword) == hashedPassword;
        }

        private Guid GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId);
        }

        public AccountController(Context context, TokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }

        /* 
            POST FUNC
            api/Account/create
        */
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            if (await _context.Accounts.AnyAsync(a => a.Email == account.Email)) 
                return BadRequest("Email is already in use.");

            account.Password = HashPassword(account.Password);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully." });
        }

        /* 
            POST FUNC
            api/Account/login
        */
        [HttpPost("login")]
        public async Task<IActionResult> LoginAccount ([FromBody] LoginModel login)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Name == login.Name);

            if (account == null || !VerifyPassword(login.Password, account.Password))
                return Unauthorized("Account isn't found.");

            var token = _tokenHelper.GenerateToken(account.Uuid);
            return Ok(new { token });
        }

        /* 
            GET FUNC
            api/Account
        */
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccount()
        {
            var userId = GetUserIdFromToken();
            var account = await _context.Accounts.FindAsync(userId);

            if (account == null)
                return NotFound("Account not found.");

            return Ok(new
            {
                account.Name,
                account.Email
            });
        }

        /* 
            PUT FUNC
            api/Account
        */
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateAccount([FromBody] Account updatedAccount)
        {
            var userId = GetUserIdFromToken();
            var account = await _context.Accounts.FindAsync(userId);

            if (account == null)
                return NotFound("Account not found.");
            
            account.Name = updatedAccount.Name;
            account.Email = updatedAccount.Email;

            if (!string.IsNullOrWhiteSpace(updatedAccount.Password))
                account.Password = HashPassword(updatedAccount.Password);

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account updated successfully." });
        }

        /* 
            DELETE FUNC
            api/Account
        */
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserIdFromToken();
            var account = await _context.Accounts.FindAsync(userId);

            if (account == null)
                return NotFound("Account not found.");
            
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account deleted successfully." });
        }
    }
}