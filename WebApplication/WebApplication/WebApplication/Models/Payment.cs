
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
        public virtual int? ReqSolutionId { get; set; }

        [Display(Name = "Решение")]
        [ForeignKey("ReqSolutionId")]
        public virtual RequestSolution ReqSolution { get; set; }

        [Display(Name = "ID задачи")]
        public virtual int? ReqId { get; set; }

        [Display(Name = "Задача, за которую оплачивается")]
        [ForeignKey("ReqId")]
        public virtual Request Req { get; set; }
        
        [Display(Name ="Описание/Комментарий")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        public virtual string Description { get; set; }

        [Display(Name = "Файл")]
        public virtual Document Document { get; set; }

        [Display(Name = "ID файла")]
        public virtual int? DocumentId { get; set; }
        
        [Display(Name = "Проверено")]
        public virtual bool Checked { get; set; }
        
        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }

        [Display(Name = "Внесено:")]
        [Range(typeof(decimal), "5,0", "100000,6", ErrorMessage = "Наименьший ввод - 5 рублей, в качестве разделителя дробной и целой части используется запятая")]
        public virtual decimal Price { get; set; }
    }
}