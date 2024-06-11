namespace DiyetisyenDanisanWebApp.Models
{
    public class MesajView
    {
        public int GonderenId { get; set; }
        public string GonderenAdi { get; set; }
        public int AliciId { get; set; }
        public string AliciAdi { get; set; }
        public string Mesaj { get; set; }
        public bool Okundu { get; set; }
    }
}
