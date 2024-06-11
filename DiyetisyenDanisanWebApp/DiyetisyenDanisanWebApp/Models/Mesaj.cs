namespace DiyetisyenDanisanWebApp.Models
{
    public class Mesaj
    {
        public int Id { get; set; }
        public int GonderenId { get; set; }
        public int AliciId { get; set; }
        public string Icerik { get; set; }
        public bool Okundu { get; set; }
    }

}
