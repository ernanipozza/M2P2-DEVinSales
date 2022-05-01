﻿using DevInSales.DTOs;
using DevInSales.Models;
using DevInSales.Context;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DevInSales.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SqlContext _context;

        public UserController(SqlContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get(
            [FromQuery] string? name, [FromQuery] string? birth_date_min, [FromQuery] string? birth_date_max)
        {
            var consulta = _context.User as IQueryable<User>;

            if (!string.IsNullOrWhiteSpace(name))
            {
                consulta = consulta.Where(u => u.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(birth_date_min))
            {
                var dataNascimentoMinimia = DateTime.ParseExact(birth_date_min, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                consulta = consulta.Where(u => u.BirthDate >= dataNascimentoMinimia);
            }

            if (!string.IsNullOrWhiteSpace(birth_date_max))
            {
                var dataNascimentoMaxima = DateTime.ParseExact(birth_date_max, "dd/MM/yyyy", new CultureInfo("pt-BR"));
                consulta = consulta.Where(u => u.BirthDate <= dataNascimentoMaxima);
            }

            var usuarios = await consulta.OrderBy(c => c.Name).ToListAsync();

            return usuarios;
        }

        /// <summary>
        /// Cadastra um novo User.
        /// </summary>
        /// <param name="requisicao">Representa as informações do novo usuário.</param>
        /// <returns>Retorna o resultado do User cadastrado.</returns>
        /// <response code="200">Retorno do User cadastrado.</response>
        /// <response code="400">Usuário menor de idade ou email já cadastrado.</response>
        /// <response code="404">Perfil não encontrado.</response>
        /// <response code="500">Ocorreu uma exceção durante o cadastro.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] UserPostDTO requisicao)
        {
            if (!isDataNascimentoValida(requisicao.BirthDate))
            {
                return BadRequest("O usuário deve ser maior de 18 anos.");
            }

            bool isEmailExistente = _context.User.Any(user => user.Email == requisicao.Email);
            if (isEmailExistente)
            {
                return BadRequest("O email informado já existe.");
            }

            var perfil = await _context.Profile.FindAsync(requisicao.ProfileId);
            if (perfil == null)
            {
                return NotFound("O perfil informado não foi encontrado.");
            }

            var novoUsuario = UserPostDTO.ConverterParaEntidade(requisicao, perfil);
            _context.User.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Create", new { id = novoUsuario.Id });
        }

        private bool isDataNascimentoValida(string data)
        {
            DateTime dataNascimento = DateTime.ParseExact(data, "dd/MM/yyyy", new CultureInfo("pt-BR"));
            DateTime diaAtual = DateTime.Today;
            int idade = diaAtual.Year - dataNascimento.Year;
            if (idade >= 18)
            {
                return true;
            }
            return false;
        }
    }
}
