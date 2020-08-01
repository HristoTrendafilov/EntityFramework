 using AutoMapper;
using AutoMapper.QueryableExtensions;
using PetStore.Data;
using PetStore.Models;
using PetStore.Models.Enumerations;
using PetStore.ServiceModels.Products.InputModels;
using PetStore.ServiceModels.Products.OutputModels;
using PetStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PetStore.Services
{
    public class ProductService : IProductService
    {
        private readonly PetStoreDbContext dbContext;
        private readonly IMapper mapper;

        public ProductService(PetStoreDbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
        }

        public ProductDetailsServiceModel GetById(string id)
        {
            var product = this.dbContext.Products
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
            {
                throw new ArgumentException("Product not found!");
            }

            var serviceModel = this.mapper.Map<ProductDetailsServiceModel>(product);

            return serviceModel;
        }

        public void AddProduct(AddProductInputServiceModel model)
        {
            try
            {
                var product = this.mapper.Map<Product>(model);

                this.dbContext.Products.Add(product);
                this.dbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid product type!");
            }
        }

        public void EditProduct(string id, EditProductInputServiceModel model)
        {
            try
            {
                var product = this.mapper.Map<Product>(model);

                var productToUpdate = this.dbContext.Products
                    .Find(id);

                if(productToUpdate == null)
                {
                    throw new ArgumentException("Product not found!");
                }

                productToUpdate.Name = product.Name;
                productToUpdate.ProductType = product.ProductType;
                productToUpdate.Price = product.Price;

                this.dbContext.SaveChanges();
            }
            catch(ArgumentException ae)
            {
                throw ae;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Invalid product type");
            }
        }

        public ICollection<ListAllProductsServiceModel> GetAll()
        {
            var products = this.dbContext.Products
                .ProjectTo<ListAllProductsServiceModel>(this.mapper.ConfigurationProvider)
                .ToList();

            return products;
        }

        public ICollection<ListAllProductsByProductTypeServiceModel> ListAllByProductType(string type)
        {
            ProductType productType;

            bool hasParsed = Enum.TryParse<ProductType>(type, true, out productType);

            if (!hasParsed)
            {
                throw new ArgumentException("Invalid product type provided!");
            }

            var productsServiceModels = this.dbContext.Products
                .Where(x => x.ProductType == productType)
                .ProjectTo<ListAllProductsByProductTypeServiceModel>(this.mapper.ConfigurationProvider)
                .ToList();

            return productsServiceModels;
        }

        public bool RemoveById(string id)
        {
            var productToRemove = this.dbContext.Products
                .Find(id);

            if (productToRemove == null)
            {
                throw new ArgumentException("Product with given id does not exist!");
            }

            this.dbContext.Products.Remove(productToRemove);
            int rowsAffected = this.dbContext.SaveChanges();

            bool wasDeleted = rowsAffected == 1;

            return wasDeleted;
        }

        public bool RemoveByName(string name)
        {
            var productToRemove = this.dbContext.Products
                .FirstOrDefault(x => x.Name == name);

            if(productToRemove == null)
            {
                throw new ArgumentException("Product with given name does not exist!");
            }

            this.dbContext.Products.Remove(productToRemove);
            var rowsAffected = this.dbContext.SaveChanges();

            bool removed = rowsAffected == 1;

            return removed;
        }

        public ICollection<ListAllProductsByNameServiceModel> SearchByName(string name, bool caseSensitive)
        {
            ICollection<ListAllProductsByNameServiceModel> products;

            if (caseSensitive)
            {
                products = this.dbContext.Products
                    .Where(x => x.Name.Contains(name))
                    .ProjectTo<ListAllProductsByNameServiceModel>(this.mapper.ConfigurationProvider)
                    .ToList();
            }

            products = this.dbContext.Products
                    .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                    .ProjectTo<ListAllProductsByNameServiceModel>(this.mapper.ConfigurationProvider)
                    .ToList();

            return products;
        }
    }
}
