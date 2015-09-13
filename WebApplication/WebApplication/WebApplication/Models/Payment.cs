
namespace WebApplication.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Оплата задач
    /// </summary>
    public class Payment : BaseModel.BaseModel
    {
        [Display(Name = "ID решения")]
        public virtual int? RequestSolutionId { get; set; }

        [Display(Name = "Решение")]
        [ForeignKey("RequestSolutionId")]
        public virtual RequestSolution RequestSolution { get; set; }

        [Display(Name = "ID задачи")]
        public virtual int? RequestId { get; set; }

        [Display(Name = "Задача")]
        [ForeignKey("RequestId")]
        public virtual Request Request { get; set; }
        
        [Display(Name ="Описание/Комментарий")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        public virtual string Description { get; set; }

        [Display(Name = "Файл")]
        public virtual Document Document { get; set; }

        [Display(Name = "ID файла")]
        public virtual int? DocumentId { get; set; }
        
        [Display(Name = "Проверено")]
        public virtual bool Checked { get; set; }

        [Display(Name = "Закрыто / Открыто")]
        public virtual bool Closed { get; set; }

        [Display(Name = "Пополнение баланса")]
        public virtual bool AddingFunds { get; set; }

        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }

        [Display(Name = "Внесено:")]
        [Range(typeof(decimal), "5,0", "100000,6", ErrorMessage = "Наименьший ввод - 5 рублей, в качестве разделителя дробной и целой части используется запятая")]
        public virtual decimal Price { get; set; }
    }
}