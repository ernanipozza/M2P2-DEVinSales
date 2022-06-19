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
    public class StateControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextTestes;
        public StateControllerTest()
        {
            _contextTestes = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("StateControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var contextoUsado = new SqlContext(_contextTestes);
            contextoUsado.Database.EnsureDeleted();
            contextoUsado.Database.EnsureCreated();
        }


        [Test]
        public async Task DeveRetornarEstadoInformadoPorNome()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var state = await contextoUsado.State.FirstAsync(state => state.Name.Contains("Alagoas"));
            var controller = new StateController(contextoUsado);
            var resultado = await controller.GetState(name: "Alagoas");
            var resultadoEsperado = (resultado.Result as ObjectResult);
            var dado = resultadoEsperado.Value as List<State>;
            NUnit.Framework.Assert.That(dado[0].Name.Contains("Alagoas"));
        }

        [Test]
        public async Task DeveRetornarEstadoInformadoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new StateController(contextoUsado);
            var resultado = await controller.GetStateId(state_id: 27);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(!resultadoEsperado.Value.ToString().Contains("Alagoas"));
        }

        [Test]
        public async Task DeveDeletarEstadoInformadoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadeEstados = contextoUsado.State.Count();
            var controller = new StateController(contextoUsado);
            var resultado = await controller.DeleteState(id: 27);
            NUnit.Framework.Assert.That(contextoUsado.State.Count(), Is.EqualTo(quantidadeEstados - 1));
        }



    }
}