
namespace WebApplication.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RequestSolution: BaseModel.BaseModel
    {
        [Display(Name = "Название")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи(50 символов)")]
        public virtual string Name { get; set; }

        [Display(Name = "ID заявки")]
        public virtual int? ReqId { get; set; }

        [Display(Name = "Заявка")]
        [ForeignKey("ReqId")]
        public virtual Request Req { get; set; }

        [Display(Name = "Файл")]
        public virtual Document Document { get; set; }

        [Display(Name = "ID файла")]
        public virtual int? DocumentId { get; set; }

        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }
        
        [Display(Name = "Комментарии")]
        public virtual string Comment { get; set; }
    }
}