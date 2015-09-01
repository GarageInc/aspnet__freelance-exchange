using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models.BaseModel
{
    public class BaseModel
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Название категории")]
        [MaxLength(100, ErrorMessage = "Превышена максимальная длина записи(100 символов)")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата добавления")]
        public virtual DateTime CreateDateTime { get; set; }


    }
}