﻿using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace EntityFramework.Exceptions.Tests
{
    public class SqlServerTests : IClassFixture<SqlServerDemoContextFixture>, IDisposable
    {
        private readonly DemoContextFixture fixture;

        public SqlServerTests(SqlServerDemoContextFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void UniqueColumnViolationThrowsUniqueConstraintException()
        {
            fixture.Context.Products.Add(new Product { Name = "GD" });
            fixture.Context.Products.Add(new Product { Name = "GD" });

            Assert.Throws<UniqueConstraintException>(() => fixture.Context.SaveChanges());
        }

        [Fact]
        public void RequiredColumnViolationThrowsCannotInsertNullException()
        {
            fixture.Context.Products.Add(new Product());

            Assert.Throws<CannotInsertNullException>(() => fixture.Context.SaveChanges());
        }

        [Fact]
        public void MaxLengthViolationThrowsMaxLengthExceededException()
        {
            fixture.Context.Products.Add(new Product { Name = new string('G', 20) });

            Assert.Throws<MaxLengthExceededException>(() => fixture.Context.SaveChanges());
        }

        [Fact]
        public void NumericOverflowViolationThrowsNumericOverflowException()
        {
            var product = new Product { Name = "GD" };
            fixture.Context.Products.Add(product);
            fixture.Context.ProductSales.Add(new ProductSale { Price = 3141.59265m, Product = product});

            Assert.Throws<NumericOverflowException>(() => fixture.Context.SaveChanges());
        }

        [Fact]
        public void ReferenceViolationThrowsReferenceConstraintException()
        {
            fixture.Context.ProductSales.Add(new ProductSale { Price = 3.14m});

            Assert.Throws<ReferenceConstraintException>(() => fixture.Context.SaveChanges());
        }

        public void Dispose()
        {
            foreach (var entityEntry in fixture.Context.ChangeTracker.Entries())
            {
                entityEntry.State = EntityState.Detached;
            }
        }
    }

    public class SqlServerDemoContextFixture : DemoContextFixture
    {
        protected override DbContextOptionsBuilder<DemoContext> BuildOptions(DbContextOptionsBuilder<DemoContext> builder, IConfigurationRoot configuration)
        {
            return builder.UseSqlServer(configuration.GetConnectionString("SqlServer")).UseExceptionProcessor();
        }
    }
}