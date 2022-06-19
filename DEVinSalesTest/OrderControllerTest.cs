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
    public class OrderControllerTest
    {
        private readonly DbContextOptions<SqlContext> _contextOptions;
        public OrderControllerTest()
        {
            _contextOptions = new DbContextOptionsBuilder<SqlContext>()
                .UseInMemoryDatabase("StateControllerTest")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            using var context = new SqlContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}