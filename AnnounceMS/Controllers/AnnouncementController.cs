using AnnounceMS.Application.DTOs;
using AnnounceMS.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnnounceMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Gecerli bir id girisi saglayin");
                var announcement = await _announcementService.GetByIdAsync(id);
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("expired_announcement")]
        public async Task<ActionResult<IEnumerable<AnnouncementDTO>>> GetExpiredAnnouncement()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var announcements = await _announcementService.GetExpiredAnnouncement();
                    return Ok(announcements);
                }
                else
                {
                    return BadRequest("Model is not valid");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<AnnouncementDTO>>> GetUpcoming()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var announcements = await _announcementService.GetAllUpcomingAsync();
                    return Ok(announcements);
                }
                else {
                    return BadRequest("Model is not valid");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create_announce")]
        public async Task<ActionResult<AnnouncementDTO>> Create(AnnouncementDTO createDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var created = await _announcementService.CreateAsync(createDto);
                    return Ok(created);
                }
                else return BadRequest("Model not valid");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update_annonunce")]
        public async Task<ActionResult<AnnouncementDTO>> Update(AnnouncementDTO createDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var created = await _announcementService.UpdateAsync(createDto);
                    return Ok(created);
                }
                else return BadRequest("Model not valid");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnnouncementDTO>> Delete(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Gecerli bir id girisi saglayin");

                bool isDeleted = await _announcementService.DeleteAsync(id);
                if (isDeleted)return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
