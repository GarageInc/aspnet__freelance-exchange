
namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Requirement : BaseModel.BaseModel
    {

        [Display(Name = "Название")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи(50 символов)")]
        public virtual string Name { get; set; }

        [Display(Name = "Описание/Пожелания")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи(200 символов)")]
        public virtual string Description { get; set; }

        [Display(Name = "Файл")]
        public virtual Document Document { get; set; }

        [Display(Name = "ID файла")]
        public virtual int? DocumentId { get; set; }

        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }
        
        // Статус
        [Display(Name = "Статус")]
        public int Status { get; set; }

        [Display(Name = "Заблокировано?")]
        public virtual bool Blocked{ get; set; }

        [Display(Name = "Причина блокировки")]
        public virtual string BlockedReason { get; set; }
        
        [Display(Name = "Выводимый баланс:")]
        [Range(typeof(decimal), "5,0", "100000,6", ErrorMessage = "Наименьшая цена - 5 рублей, в качестве разделителя дробной и целой части используется запятая")]
        public virtual decimal Price { get; set; }
    }
}