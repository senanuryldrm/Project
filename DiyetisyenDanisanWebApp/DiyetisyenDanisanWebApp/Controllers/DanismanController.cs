using DiyetisyenDanisanWebApp.Models;
using DiyetisyenDanisanWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class DanismanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly int kullaniciId;

    public DanismanController(IHttpContextAccessor httpContextAccessor)
    {
        _context = new ApplicationDbContext();
        kullaniciId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Authorize(Roles = "Danisman")]
    public IActionResult Index()
    {
        var danisman = _context.Kullanicilar.First(m => m.Id == kullaniciId && m.Role == Role.Danisman);
        var diyetisyen = _context.Kullanicilar.First(m => m.Id == danisman.DiyetisyenKodu && m.Role == Role.Diyetisyen);
        var mesajlar = _context.Mesajlar.Where(m => m.GonderenId == kullaniciId || m.AliciId == kullaniciId).ToList();

        var mesajView = new List<MesajView>();

        mesajlar.ForEach(mesaj =>
                mesajView.Add(new()
                {
                    GonderenId = mesaj.GonderenId,
                    GonderenAdi = mesaj.GonderenId == kullaniciId ? "Ben" : diyetisyen.AdSoyad,
                    AliciId = mesaj.AliciId,
                    AliciAdi = mesaj.AliciId == kullaniciId ? "Ben" : diyetisyen.AdSoyad,
                    Mesaj = mesaj.Icerik,
                    Okundu = mesaj.Okundu
                })
        );

        ViewBag.mesajlar = mesajView;

        return View(danisman);
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
            var kullanici = _context.Kullanicilar.FirstOrDefault(x => x.Mail == loginForm.Mail && x.Parola == loginForm.Parola && x.Role == Role.Danisman);
            if (kullanici != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,kullanici.AdSoyad),
                    new Claim(ClaimTypes.NameIdentifier,kullanici.Id.ToString()),
                    new Claim(ClaimTypes.Role,"Danisman")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Redirect("/Danisman");
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
    public async Task<IActionResult> Kayit(Kullanici danisman)
    {
        if (ModelState.IsValid && danisman.Role == Role.Danisman)
        {
            var diyetisyen = _context.Kullanicilar
                .Where(d => d.Role == Role.Diyetisyen)
                .OrderBy(d => _context.Kullanicilar.Count(x => x.DiyetisyenKodu == d.Id))
                .FirstOrDefault();

            danisman.DiyetisyenKodu = diyetisyen.Id;

            var mailCheck = _context.Kullanicilar.Any(d => d.Role == Role.Danisman && d.Mail == danisman.Mail);

            if (mailCheck)
            {
                TempData["error"] = "Bu mail adresi ile daha önce üye olunmuş.";
                return View(danisman);
            }

            if (diyetisyen == null)
            {
                TempData["error"] = "Sistemde kayıtlı diyetisyen bulunmamaktadır.";
                return View(danisman);
            }

            _context.Kullanicilar.Add(danisman);
            await _context.SaveChangesAsync();
            return Redirect("/Danisman/Giris");
        }
        return View(danisman);
    }

    [HttpPost]
    public async Task<IActionResult> MesajGonder(MesajGonderModel mesajModel)
    {
        var kayit = new Mesaj
        {
            GonderenId = kullaniciId,
            AliciId = mesajModel.AliciId,
            Icerik = mesajModel.Mesaj
        };

        _context.Mesajlar.Add(kayit);

        await _context.SaveChangesAsync();
        return Redirect("/Danisman");
    }

    [HttpPost]
    public IActionResult MesajOkundu(int gonderenId, int aliciId)
    {
        var mesajlar = _context.Mesajlar
            .Where(m => m.GonderenId == gonderenId && m.AliciId == aliciId)
            .ToList();

        foreach (var mesaj in mesajlar)
            mesaj.Okundu = true;

        _context.SaveChanges();

        return Ok();
    }
}
