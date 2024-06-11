using DiyetisyenDanisanWebApp.Models;
using DiyetisyenDanisanWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiyetisyenDanisanWebApp.Controllers
{
    public class DiyetisyenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int kullaniciId;

        public DiyetisyenController(IHttpContextAccessor httpContextAccessor)
        {
            _context = new ApplicationDbContext();
            kullaniciId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        [Authorize(Roles = "Diyetisyen")]
        public IActionResult Index()
        {
            var danismanlarim = _context.Kullanicilar.Where(x => x.DiyetisyenKodu == kullaniciId && x.Role == Role.Danisman).ToList();
            var diyetisyen = _context.Kullanicilar.First(m => m.Id == kullaniciId && m.Role == Role.Diyetisyen);
            var mesajlar = _context.Mesajlar.Where(m => m.GonderenId == kullaniciId || m.AliciId == kullaniciId).ToList();

            var mesajView = new List<MesajView>();

            mesajlar.ForEach(mesaj =>
                    mesajView.Add(new()
                    {
                        GonderenId = mesaj.GonderenId,
                        GonderenAdi = mesaj.GonderenId == kullaniciId ? "Ben" : danismanlarim.First(m => m.Id == mesaj.GonderenId).AdSoyad,
                        AliciId = mesaj.AliciId,
                        AliciAdi = mesaj.AliciId == kullaniciId ? "Ben" : danismanlarim.First(m => m.Id == mesaj.AliciId).AdSoyad,
                        Mesaj = mesaj.Icerik,
                        Okundu = mesaj.Okundu
                    })
            );

            ViewBag.mesajlar = mesajView;

            return View(danismanlarim);
        }

        public IActionResult Giris()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult Giris(LoginForm loginForm)
        {
            if (ModelState.IsValid)
            {
                var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Mail == loginForm.Mail && x.Parola == loginForm.Parola && x.Role == Role.Diyetisyen);
                if (kullanici != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,kullanici.AdSoyad),
                        new Claim(ClaimTypes.NameIdentifier,kullanici.Id.ToString()),
                        new Claim(ClaimTypes.Role,"Diyetisyen")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return Redirect("/Diyetisyen");
                }
            }

            TempData["error"] = $"Kullanıcı adı veya parola yanlış.";
            return View();
        }

        public IActionResult Kayit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Kayit(Kullanici diyetisyen)
        {
            if (ModelState.IsValid)
            {
                var mailCheck = _context.Kullanicilar.Any(d => d.Role == Role.Diyetisyen && d.Mail == diyetisyen.Mail);

                if (mailCheck)
                {
                    TempData["error"] = "Bu mail adresi ile daha önce üye olunmuş.";
                    return View(diyetisyen);
                }

                _context.Kullanicilar.Add(diyetisyen);
                await _context.SaveChangesAsync();
                return Redirect("/Diyetisyen/Giris");
            }
            else
            {
                return View(diyetisyen);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DiyetPlani(DiyetPlaniModel diyatPlani)
        {
            var danisman = _context.Kullanicilar.First(x => x.Id == diyatPlani.DanismanId && x.Role == Role.Danisman);
            danisman.DiyetProgrami = diyatPlani.DiyetPlani;
            await _context.SaveChangesAsync();
            return Redirect("/Diyetisyen");
        }

        [HttpPost]
        public async Task<IActionResult> MesajGonder(MesajGonderModel mesajModel)
        {
            var kayit = new Mesaj
            {
                AliciId = mesajModel.AliciId,
                GonderenId = kullaniciId,
                Icerik = mesajModel.Mesaj
            };

            _context.Mesajlar.Add(kayit);

            await _context.SaveChangesAsync();
            return Redirect("/Diyetisyen");
        }

        [HttpPost]
        public IActionResult MesajOkundu(int gonderenId, int aliciId)
        {
            var danismanId = kullaniciId == gonderenId ? aliciId : gonderenId;
            var mesajlar = _context.Mesajlar
                .Where(m => m.GonderenId == danismanId && m.AliciId == kullaniciId)
                .ToList();

            foreach (var mesaj in mesajlar)
                mesaj.Okundu = true;

            _context.SaveChanges();

            return Ok();
        }

    }
}
