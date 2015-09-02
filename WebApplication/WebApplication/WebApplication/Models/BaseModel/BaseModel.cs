using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models.BaseModel
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата добавления")]
        public virtual DateTime CreateDateTime { get; set; }
        
        [Display(Name = "Удален ли объект")]
        public virtual bool IsDeleted { get; set; }
    }
}