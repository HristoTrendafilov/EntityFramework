﻿using PetStore.Common;
using PetStore.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PetStore.ServiceModels.Products.InputModels
{
    public class AddProductInputServiceModel
    {
        [Required]
        [MinLength(GlobalConstants.ProductNameMinLength)]
        [MaxLength(GlobalConstants.ProductNameMaxLength)]
        public string Name { get; set; }

        public int ProductType { get; set; }

        [Range(GlobalConstants.SellableMinPrice, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
