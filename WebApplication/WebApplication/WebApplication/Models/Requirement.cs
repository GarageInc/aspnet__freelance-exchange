using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;
    using System.ComponentModel.DataAnnotations;

    public class Requirement : BaseModel.BaseModel
    {
        [Display(Name = "Описание/Пожелания")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи(200 символов)")]
        public virtual string Description { get; set; }
        
        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }
        
        [Display(Name = "Проверено")]
        public virtual bool Checked { get; set; }
        
        [Display(Name = "Подтверждение администрации")]
        public virtual ICollection<RequirementConfirmation> RequirementConfirmations { get; set; }
        
        [Display(Name = "Подтверждение?")]
        public virtual bool CanDownload { get; set; }

        [Display(Name = "Закрыто?")]
        public virtual bool Closed { get; set; }

        [Display(Name = "Заблокировано?")]
        public virtual bool IsBlocked { get; set; }

        [Display(Name = "Дата разблокировки")]
        public virtual DateTime BlockForDate { get; set; }

        [Display(Name = "Причина блокировки")]
        public virtual string BlockReason { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата блокировки")]
        public virtual DateTime DateOfBlocking { get; set; }

        [Display(Name = "Выводимый баланс:")]
        [Range(typeof(decimal), "5,0", "100000,6", ErrorMessage = "Наименьшая цена - 5 рублей, в качестве разделителя дробной и целой части используется запятая")]
        [Remote("CheckPrice", "Requirement", "Данный пользователь уже зарегистрирован в сети")]
        public virtual decimal Price { get; set; }
    }
}