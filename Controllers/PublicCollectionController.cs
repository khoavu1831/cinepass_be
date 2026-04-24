using CinePass_be.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinePass_be.Controllers
{
    [Route("api/public/collections")]
    [ApiController]
    public class PublicCollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public PublicCollectionController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicCollectionsAsync()
        {
            var result = await _collectionService.GetAllAsync();
            return Ok(result);
        }
    }
}
