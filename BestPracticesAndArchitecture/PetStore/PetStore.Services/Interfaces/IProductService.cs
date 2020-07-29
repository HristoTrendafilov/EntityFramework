using PetStore.ServiceModels.Products.InputModels;
using PetStore.ServiceModels.Products.OutputModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetStore.Services.Interfaces
{
    public interface IProductService
    {
        ICollection<ListAllProductsByProductTypeServiceModel> ListAllByProductType(string type);

        void AddProduct(AddProductInputServiceModel model);

        ICollection<ListAllProductsServiceModel> GetAll();

        bool RemoveById(string id);

        bool RemoveByName(string name);

        ICollection<ListAllProductsByNameServiceModel> SearchByName(string name, bool caseSensitive);

        void EditProduct(string id, EditProductInputServiceModel model);
    }
}
