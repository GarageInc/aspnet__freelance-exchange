
namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Request
    {
        [Key]
        [Display(Name = "ID")]
        public virtual int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        public virtual string Name { get; set; }

        [Display(Name = "Описание/Пожелания")]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        public virtual string Description { get; set; }
        
        [Display(Name = "Файл")]
        public virtual Document Document { get; set; }

        [Display(Name = "ID файла")]
        public virtual int? DocumentId { get; set; }

        [Display(Name = "Автор")]
        public virtual ApplicationUser Author { get; set; }

        [Display(Name = "ID Автора")]
        public virtual string AuthorId { get; set; }

        [Display(Name = "Решает")]
        public virtual ApplicationUser Executor { get; set; }

        [Display(Name = "ID исполнителя задачи")]
        public virtual string ExecutorId { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Обязательно для заполнения!")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Срок?")]
        public virtual DateTime Deadline { get; set; }

        // Статус задачи
        [Display(Name = "Статус")]
        public int Status { get; set; }

        // Приоритет задачи
        [Display(Name = "Приоритет")]
        public int Priority { get; set; }

        // Внешний ключ Категория
        [Display(Name = "ID Типа")]
        public int? CategoryId { get; set; }

        [Display(Name = "Тип")]
        public Category Category { get; set; }
        
        // Внешний ключ Категория
        [Display(Name = "ID предмета")]
        public int? SubjectId { get; set; }

        [Display(Name = "Предмет")]
        public Subject Subject { get; set; }
        
        // Ссылка на жизненный цикл заявки - Навигационное свойство
        [Display(Name = "Обработка")]
        public Lifecycle Lifecycle { get; set; }

        [Display(Name = "ID Обработки")]
        public virtual int? LifecycleId { get; set; }

        [Display(Name ="Цена")]
        [Range(typeof(decimal), "5,0", "100000,6", ErrorMessage = "Наименьшая цена - 5 рублей, в качестве разделителя дробной и целой части используется запятая")]
        public virtual decimal Price { get; set; }

        [Display(Name="Оплачено?")]
        public virtual bool IsPaid { get; set; }

        [Display(Name="Захотевшие решать")]
        public virtual ICollection<ApplicationUser> Solvers { get; set; }
        
        [Display(Name = "Допуск(проверено ли администрацией)")]
        public virtual bool Checked { get; set; }
        
        [Display(Name = "Можно скачивать решение?")]
        public bool CanDownload { get; set; }

        [Display(Name = "Комментарии")]
        public virtual ICollection<Comment> Comments { get; set; }

        [Display(Name = "Решение нужно онлайн")]
        public virtual  bool IsOnline { get; set; }
    }

    // Перечисление для статуса заявки
    public enum RequestStatus
    {
        Open = 1,
        Distributed = 2,
        Proccesing = 3,
        Closed = 4
    }

    // Перечисление для приоритета заявки
    public enum RequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}