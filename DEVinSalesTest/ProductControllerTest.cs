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
    public class ProductControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextTestes;
        public ProductControllerTest()
        {
            _contextTestes = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("ProductControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var contextoUsado = new SqlContext(_contextTestes);
            contextoUsado.Database.EnsureDeleted();
            contextoUsado.Database.EnsureCreated();
        }

        [Test]
        public async Task DeveRetornarProdutoInformadoPorNome()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var nomeProduto = await contextoUsado.Product.FirstAsync(product => product.Name.Contains("Java"));
            var controller = new ProductController(contextoUsado);
            var resultado = await controller.GetProduct(name: "Java", price_min: 1.00M, price_max: 500.00M);
            var resultadoEsperado = (resultado.Result as ObjectResult);
            NUnit.Framework.Assert.That(resultadoEsperado.Value.ToString(), Is.EqualTo("Curso de Java"));
        }

        [Test]
        public async Task DeveDeletarProdutoInformadoPorId()
        {
            var contextoUsado = new SqlContext(_contextTestes);
            var quantidadeProdutos = contextoUsado.Product.Count();
            var controller = new ProductController(contextoUsado);
            var resultado = await controller.DeleteProduct(product_id: 3);
            NUnit.Framework.Assert.That(contextoUsado.CityPrice.Count(), Is.EqualTo(quantidadeProdutos + 2));
        }

    }
}