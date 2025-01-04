using AnnounceMS.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnnounceMS.MVC.Controllers
{
    public class ExpiredAnnouncementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AnnounceController> _logger;
        private const string API_BASE_URL = "api/Announcement/";

        public ExpiredAnnouncementController(IHttpClientFactory httpClientFactory, ILogger<AnnounceController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AnnounceApi");
                var response = await client.GetAsync($"{API_BASE_URL}expired_announcement");

                if (response.IsSuccessStatusCode)
                {
                    var announcements = await response.Content.ReadFromJsonAsync<List<AnnouncementDTO>>();
                    return View("ExpiredIndex", announcements);
                }

                TempData["ErrorMessage"] = "Veriler getirilirken bir hata oluştu.";
                return View("ExpiredIndex", new List<AnnouncementDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching announcements");
                TempData["ErrorMessage"] = "Bir hata oluştu! Lütfen daha sonra tekrar deneyiniz.";
                return View("ExpiredIndex", new List<AnnouncementDTO>());
            }
        }
    }
}
