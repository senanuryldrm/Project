namespace DiyetisyenDanisanWebApp.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Mail { get; set; }
        public string Telefon { get; set; }
        public string Parola { get; set; }
        public Role Role { get; set; }
        public string? DiyetProgrami { get; set; }
        public int? DiyetisyenKodu { get; set; }
    }
}
