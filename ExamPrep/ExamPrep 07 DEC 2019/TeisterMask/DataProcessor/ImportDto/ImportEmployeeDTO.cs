using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class ImportEmployeeDTO
    {
        [Required]
        [MaxLength(40)]
        [RegularExpression("^[a-zA-Z0-9]+$")]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [RegularExpression("^(\\d{3})\\-(\\d{3})\\-(\\d{4})$")]
        [Required]
        public string Phone { get; set; }

        public int[] Tasks { get; set; }
    }
}
