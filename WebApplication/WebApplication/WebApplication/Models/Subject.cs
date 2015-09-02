
namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Subject: BaseModel.BaseModel
    {
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Название категории")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи(50 символов)")]
        public string Name { get; set; }
    }
}