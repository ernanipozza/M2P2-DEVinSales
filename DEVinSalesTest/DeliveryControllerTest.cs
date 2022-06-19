using DevInSales.Context;
using DevInSales.Controllers;
using DevInSales.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEVinSalesTest
{
    [TestClass]
    public class DeliveryControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextTestes;
        public DeliveryControllerTest()
        {
            _contextTestes = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("DeliveryControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var contextoUsado = new SqlContext(_contextTestes);
            contextoUsado.Database.EnsureDeleted();
            contextoUsado.Database.EnsureCreated();
        }


        [Test]
        public async Task DeveRetornarDadosDaEntrega()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var controller = new DeliveryController(contextoUsado);
            var resultado = await controller.GetDelivery(address_id: 1, order_id: 1);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Equals, Is.EqualTo(10));
        }

    }
}