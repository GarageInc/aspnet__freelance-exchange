

namespace WebApplication.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ErrorMessage : BaseModel.BaseModel
    {
        [Display(Name = "Сообщение")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        public virtual string Text { get; set; }

        [Display(Name = "Файлы")]
        public virtual Document Document { get; set; }

        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }

        // Статус задачи
        [Display(Name = "Статус")]
        public int ErrorStatus { get; set; }
        
        [Display(Name = "Для администрации")]
        public virtual bool ForAdministration { get; set; }

        [Display(Name = "Мыло")]
        [EmailAddress]
        public virtual string Email { get; set; }
    }

    // Перечисление для статуса заявки
    public enum ErrorMessageStatus
    {
        Open = 0,
        Closed = 1
    }
}