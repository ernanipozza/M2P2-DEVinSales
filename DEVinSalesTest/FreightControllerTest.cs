using DevInSales.Context;
using DevInSales.Controllers;
using DevInSales.DTOs;
using DevInSales.Models;
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
    public class FreightControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextTestes;
        public FreightControllerTest()
        {
            _contextTestes = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("FreightControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var contextoUsado = new SqlContext(_contextTestes);
            contextoUsado.Database.EnsureDeleted();
            contextoUsado.Database.EnsureCreated();
        }

        [Test]
        public async Task DeveRetornarFreteMaisBaratoParaCidadeInformadaPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new FreightController(contextoUsado);
            var resultado = controller.GetFreight(cityId: 1);
            var resultadoEsperado = (resultado.Result as ObjectResult).Value;
            NUnit.Framework.Assert.That(resultadoEsperado.Equals, Is.EqualTo(10));
        }

        [Test]
        public async Task DeveRetornarDadosDaCompanhiaInformadaPorNome() 
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var companyName = await contextoUsado.ShippingCompany.FirstAsync(shippingCompany => shippingCompany.Name.Contains("Rapidex"));
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.GetCompanyByName(name: "Rapidex");
            var resultadoEsperado = (resultado.Result as ObjectResult);
            var dado = resultadoEsperado.Value as List<ShippingCompany>;
            NUnit.Framework.Assert.That(dado[0].Name.Contains("Rapidex"));
        }

        [Test]
        public async Task DeveRetornarDadosDaCompanhiaInformadaPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.GetCompanyById(id: 1);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Value.ToString().Contains("Rapidex"));
        }

        [Test]
        public async Task DeveRetornarPrecoPorEstadoEmpresaInformadosPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.GetStateCompanyById(stateId: 11, companyId: 1);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Value, Is.EqualTo(17));
        }

        [Test]
        public async Task DeveRetornarPrecoPorCidadeEmpresaInformadosPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.GetCityCompanyById(cityId: 1, companyId: 1);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Value, Is.EqualTo(10));
        }

        [Test]
        public async Task DeveDeletarPrecoPorCidadeInformadaPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadePrecosPorCidadeInformada = contextoUsado.CityPrice.Count();
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.DeleteCityPrice(cityPriceId: 3);
            NUnit.Framework.Assert.That(contextoUsado.CityPrice.Count(), Is.EqualTo(quantidadePrecosPorCidadeInformada - 1));
        }

        [Test]
        public async Task DeveDeletarPrecoPorEstadoInformadoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadePrecosPorEstadoInformado = contextoUsado.StatePrice.Count();
            var controller = new FreightController(contextoUsado);
            var resultado = await controller.DeleteStatePrice(statePriceId: 11);
            NUnit.Framework.Assert.That(contextoUsado.StatePrice.Count(), Is.EqualTo(quantidadePrecosPorEstadoInformado - 1));
        }

    }
}