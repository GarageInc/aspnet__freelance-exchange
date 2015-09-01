
namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PropsCategory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Название категории")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи(50 символов)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Дополнительная информация")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи(200 символов)")]
        public string Info { get; set; }
    }
}