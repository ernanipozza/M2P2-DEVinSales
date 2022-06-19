using DevInSales.Context;
using DevInSales.Controllers;
using DevInSales.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DEVinSalesTest
{
    [TestClass]
    public class AddressControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextTestes;
        public AddressControllerTest()
        {
            _contextTestes = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("AddressControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var contextoUsado = new SqlContext(_contextTestes);
            contextoUsado.Database.EnsureDeleted();
            contextoUsado.Database.EnsureCreated();
        }


        [Test]
        public async Task DeveRetornarTodosEnderecos()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadeEnderecos = contextoUsado.Address.Count();
            var controller = new AddressController(contextoUsado);
            var resultado = await controller.GetAddress();
            var resultadoEsperado = (resultado.Result as ObjectResult);
            var dados = resultadoEsperado.Value as List<AddressDTO>;
            NUnit.Framework.Assert.That(dados.Count, Is.EqualTo(quantidadeEnderecos));
        }

        [Test]
        public async Task DeveRetornarEnderecoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new AddressController(contextoUsado);
            var resultado = await controller.GetAddress(id: 2);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Value.ToString().Contains("Frente"));
        }

        [Test]
        public async Task DeveDeletarEnderecoInformadoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadeEnderecos = contextoUsado.Address.Count();
            var controller = new AddressController(contextoUsado);
            var resultado = await controller.Delete(id: 1);
            NUnit.Framework.Assert.That(contextoUsado.Address.Count(), Is.EqualTo(quantidadeEnderecos - 1));
        }
    }
}