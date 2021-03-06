﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Store.DAL.EF;
using Store.DAL.Repos.Base;
using Store.DAL.Repos.Interfaces;
using Store.Models.Entities;
using Store.Models.ViewModels.Base;

namespace Store.DAL.Repos
{
    public class ProductRepo : RepoBase<Product>, IProductRepo
    {
        public ProductRepo(DbContextOptions<StoreContext> options) : base(options)
        {
        }
        public ProductRepo() : base()
        {
        }
        public override IEnumerable<Product> GetAll()
            => Table.OrderBy(x => x.ModelName);
        public override IEnumerable<Product> GetRange(int skip, int take)
            => GetRange(Table.OrderBy(x => x.ModelName), skip, take);

        internal ProductAndCategoryBase GetRecord(Product p, Category c)
            => new ProductAndCategoryBase()
            {
                CategoryName = c.CategoryName,
                CategoryId = p.CategoryId,
                CurrentPrice = p.CurrentPrice,
                Description = p.Description,
                IsFeatured = p.IsFeatured,
                Id = p.Id,
                ModelName = p.ModelName,
                ModelNumber = p.ModelNumber,
                ProductImage = p.ProductImage,
                ProductImageLarge = p.ProductImageLarge,
                ProductImageThumb = p.ProductImageThumb,
                TimeStamp = p.TimeStamp,
                UnitCost = p.UnitCost,
                UnitsInStock = p.UnitsInStock
            };

        public IEnumerable<ProductAndCategoryBase> GetProductsForCategory(int id)
            => Table
                .OrderBy(x => x.ModelName)
                .Where(p => p.CategoryId == id)
                .Select(item => GetRecord(item, item.Category))
                ;


        public IEnumerable<ProductAndCategoryBase> GetAllWithCategoryName()
            => Table
                .OrderBy(x => x.ModelName)
                .Include(p => p.Category)
                .Select(item => GetRecord(item, item.Category));


        public IEnumerable<ProductAndCategoryBase> GetFeaturedWithCategoryName()
            => Table
                .OrderBy(x => x.ModelName)
                .Include(p => p.Category)               
                .Where(p => p.IsFeatured)
                .Select(item => GetRecord(item, item.Category));
                

        public ProductAndCategoryBase GetOneWithCategoryName(int id)
            => Table
             	.Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(item => GetRecord(item, item.Category))
                .SingleOrDefault();

        public IEnumerable<ProductAndCategoryBase> Search(string searchString)
            => Table
                .OrderBy(x => x.ModelName)
                 .Include(p => p.Category)
                .Where(p =>
                    p.Description.ToLower().Contains(searchString.ToLower())
                    || p.ModelName.ToLower().Contains(searchString.ToLower()))
                .Select(item => GetRecord(item, item.Category));
               
    }
}
