namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Props: BaseModel.BaseModel
    {
        // Внешний ключ Категория
        [Display(Name = "ID")]
        public int? PropsCategoryId { get; set; }

        [Display(Name = "Категория")]
        public PropsCategory PropsCategory { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [Display(Name = "Номер кошелька/счета")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи(200 символов)")]
        public string Number { get; set; }
        
        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }
    }
}