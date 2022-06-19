using DevInSales.Context;
using DevInSales.DTOs;
using DevInSales.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevInSales.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    //[Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly SqlContext _context;
        public AuthenticationController(SqlContext context)
        {
            _context = context;
        }
        [HttpPost("/autenticar")]
        public async Task<ActionResult<dynamic>> AuthenticateAsync([FromBody] UserLoginDto dto)
        {
            var user = await _context.User.Include(x => x.Profile).FirstOrDefaultAsync(x => x.Email == dto.Email && x.Password == dto.Password);
            if (user == null)
            {
                return BadRequest(new { Message = "Usuário e/ou senha inválidos."});
            }
            var token = TokenService.GenerateToken(user);
            var result = new
            {
                token,
                User = new
                {
                    user.Id,
                    user.Name,
                    user.Email
                }
            };
            user.Password = "";
            return Ok(new {result});
        }

        [HttpGet("endpointAberto")]
        public ActionResult EndpointAberto()
        {
            return Ok(new { Message = "Endpoint aberto, só chegar!" });
        }

        [HttpGet("endpointUsuario")]
        [Authorize(Roles = "Usuário, Gerente, Administrador")]
        public ActionResult EndpointUsuario()
        {
            return Ok(new { Message = "Seja bem vindo ao endpoint do usuário" });
        }

        [HttpGet("endpointGerente")]
        [Authorize(Roles = "Gerente, Administrador")]
        public ActionResult EndpointGerente()
        {
            return Ok(new { Message = "Seja bem vindo ao endpoint do gerente" });
        }

        [HttpGet("endpointAdmin")]
        [Authorize(Roles = "Administrador")]
        public ActionResult EndpointAdmin()
        {
            return Ok(new { Message = "Seja bem vindo ao endpoint do administrador" });
        }
    }
}
