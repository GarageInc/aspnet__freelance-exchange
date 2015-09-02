
namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PropsCategory : BaseModel.BaseModel
    {
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Название категории")]
        public virtual string Name { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Дополнительная информация")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи(200 символов)")]
        public virtual string Info { get; set; }
    }
}