namespace WebApplication.Models.BaseModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

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

        [Display(Name = "Дата удаления")]
        public virtual DateTime? DateOfDeleting { get; set; }

        // Добавить элемент "Архив" - когда и какие изменения были внесены в сущность и, главное, КЕМ. + функция восстановить и т.п.
    }
}