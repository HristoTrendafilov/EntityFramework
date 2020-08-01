using PetStore.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PetStore.ServiceModels.Products.InputModels
{
    public class EditProductInputServiceModel
    {
        [Required]
        [MinLength(GlobalConstants.ProductNameMinLength)]
        public string Name { get; set; }

        public string ProductType { get; set; }

        [Range(GlobalConstants.SellableMinPrice, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
