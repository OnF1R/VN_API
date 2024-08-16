using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelListController : ControllerBase
    {
        private readonly INovelService _novelService;
        private readonly IMemoryCache _cache;

        public VisualNovelListController(INovelService novelService, IMemoryCache cache)
        {
            _novelService = novelService ?? throw new ArgumentNullException(nameof(novelService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        //[HttpPost("AddListType")]
        //public async Task<IActionResult> AddListType(string name)
        //{
        //    var listType = await _novelService.AddListType(name);

        //    if (listType == null)
        //    {
        //        return StatusCode(StatusCodes.Status204NoContent, "Error while add listType");
        //    }

        //    return StatusCode(StatusCodes.Status200OK, listType);
        //}

        [HttpGet("VisualNovelList")]
        public async Task<IActionResult> GetVisualNovelList(int listId)
        {
            var lists = await _novelService.GetVisualNovelList(listId);

            if (lists == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, lists);
        }

        [HttpGet("VisualNovelLists")]
        public async Task<IActionResult> GetVisualNovelLists(string userId, bool showPrivate)
        {
            var lists = await _novelService.GetVisualNovelLists(userId, showPrivate);

            if (lists == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, lists);
        }

        [HttpGet("VisualNovelsInList")]
        public async Task<IActionResult> GetVisualNovelsInList(string userId, int listId)
        {
            var vnInList = await _novelService.GetVisualNovelsInList(userId, listId);

            if (vnInList == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vnInList);
        }

        [HttpGet("UserVisualNovelsInLists")]
        public async Task<IActionResult> GetUserVisualNovelsInLists(string userId, bool showPrivate)
        {
            var vnInList = await _novelService.GetUserVisualNovelsInLists(userId, showPrivate);

            if (vnInList == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vnInList);
        }

        [HttpGet("VisualNovelInAnyUserList")]
        public async Task<IActionResult> GetVisualNovelInAnyUserList(string userId, int visualNovelId)
        {
            var entry = await _novelService.GetVisualNovelInAnyUserList(userId, visualNovelId);

            if (entry == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }  

            return StatusCode(StatusCodes.Status200OK, entry);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVisualNovelList(string userId, int listId, VisualNovelList visualNovelList)
        {
            var dbList = await _novelService.UpdateVisualNovelList(userId, listId, visualNovelList);

            if (dbList == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            if (dbList.Id != listId)
            {
                return BadRequest();
            }

            return StatusCode(StatusCodes.Status200OK, dbList);
        }

        [HttpPost("CreateBaseLists")]
        public async Task<IActionResult> CreateBaseLists(string userId)
        {
            (bool status, string message) = await _novelService.CreateBaseLists(userId);

            if (!status)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, message);
        }

        [HttpPost("CreateCustomList")]
        public async Task<IActionResult> CreateCustomList(string userId, VisualNovelList visualNovelList)
        {
            var list = await _novelService.CreateCustomList(userId, visualNovelList);

            if (list == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK, list);
        }

        [HttpPost("AddToList")]
        public async Task<IActionResult> AddToList(string userId, int listId, int visualNovelId)
        {
            (bool status, string message) = await _novelService.AddToList(userId, listId, visualNovelId);

            if (!status)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, message);
        }

        [HttpDelete("RemoveFromList")]
        public async Task<IActionResult> RemoveFromList(string userId, int listId, int visualNovelId)
        {
            (bool status, string message) = await _novelService.RemoveFromList(userId, listId, visualNovelId);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, message);
        }

        [HttpDelete("DeleteList")]
        public async Task<IActionResult> DeleteList(string userId, int listId)
        {
            (bool status, string message) = await _novelService.DeleteList(userId, listId);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, message);
        }
    }
}