using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceMS.Entities.Entities
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }                     // Benzersiz Kimlik
        public string Title { get; set; }              // Anons Başlığı
        public string Content { get; set; }            // Anons İçeriği
        public DateTime StartDate { get; set; }        // Anons Başlangıç Tarihi
        public DateTime EndDate { get; set; }          // Anons Bitiş Tarihi
        public bool IsActive { get; set; }             // Anonsun Aktiflik Durumu
    }
}
