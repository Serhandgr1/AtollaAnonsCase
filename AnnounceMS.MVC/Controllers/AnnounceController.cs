using AnnounceMS.MVC.Models;
using Microsoft.AspNetCore.Mvc;

public class AnnounceController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AnnounceController> _logger;
    private const string API_BASE_URL = "api/Announcement/";

    public AnnounceController(IHttpClientFactory httpClientFactory, ILogger<AnnounceController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AnnounceApi");
            var response = await client.GetAsync($"{API_BASE_URL}upcoming");

            if (response.IsSuccessStatusCode)
            {
                var announcements = await response.Content.ReadFromJsonAsync<List<AnnouncementDTO>>();
                return View("AnnounceIndex", announcements);
            }

            TempData["ErrorMessage"] = "Veriler getirilirken bir hata oluştu.";
            return View("AnnounceIndex", new List<AnnouncementDTO>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching announcements");
            TempData["ErrorMessage"] = "Bir hata oluştu! Lütfen daha sonra tekrar deneyiniz.";
            return View("AnnounceIndex", new List<AnnouncementDTO>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AnnounceApi");
            var response = await client.GetAsync($"{API_BASE_URL}{id}");

            if (response.IsSuccessStatusCode)
            {
                var announcement = await response.Content.ReadFromJsonAsync<AnnouncementDTO>();
                return Json(announcement);
            }

            return NotFound(new { message = "Duyuru bulunamadı." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcement with ID: {Id}", id);
            return StatusCode(500, new { message = "Duyuru getirilirken bir hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AnnouncementDTO announcement)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = _httpClientFactory.CreateClient("AnnounceApi");
            var response = await client.PostAsJsonAsync($"{API_BASE_URL}create_announce", announcement);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"API returned non-success: {errorMessage}");
            return Json(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] AnnouncementDTO announcement)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = _httpClientFactory.CreateClient("AnnounceApi");
            var response = await client.PutAsJsonAsync($"{API_BASE_URL}update_annonunce", announcement);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"API returned non-success: {errorMessage}");
            return Json(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement");
            return Json(new { success = false, message = "Güncelleme sırasında bir hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (id <= 0) return Json(new { success = false, message = "Silinmek istenen id hatali gonderildi" });
            var client = _httpClientFactory.CreateClient("AnnounceApi");
            var response = await client.DeleteAsync($"{API_BASE_URL}{id}");

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"API returned non-success: {errorMessage}");
            return Json(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting announcement: {Id}", id);
            return Json(new { success = false, message = "Silme işlemi sırasında bir hata oluştu." });
        }
    }
}