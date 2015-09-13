
namespace WebApplication.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    // Модель жизенного цикла задачи-заявки
    public class Lifecycle : BaseModel.BaseModel
    {
        // Дата открытия
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name="Открыта")]
        public DateTime Opened { get; set; }

        // Дата распределения
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Распределена")]
        public DateTime? Distributed { get; set; }

        // Дата обработки
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "В процессе решения")]
        public DateTime? Proccesing { get; set; }

        // Дата проверки
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Проверена")]
        public DateTime? Checking { get; set; }

        // Дата закрытия
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Закрыта")]
        public DateTime? Closed { get; set; }
    }
}