using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using ReservationSystem.Middleware;
using ReservationSystem.Services;
using ReservationSystem.Services.ReservationSystem.Services;

namespace ReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {

        private readonly Services.ReservationSystem.Services.IItemService _service;
        private readonly IUserAuthenticationService _authenticationService;

        public ItemsController(IItemService service, IUserAuthenticationService authenticationService)
        {

            _service = service;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()          
        {

            return Ok(await _service.GetItemsAsync());
        }

        [HttpGet("user/{username}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems(String username)        
        {

            return Ok(await _service.GetItemsAsync(username));
        }


        // GET: api/Items
        /// <summary>
        /// Get all items from database
        /// </summary>
        /// <returns>list of items</returns>
        [HttpGet("{query}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> QueryItems(String query)        
        {
            return Ok(await _service.QueryItemsAsync(query));
        }

        // GET: api/Items/5
        /// <summary>
        /// Get a single item
        /// </summary>
        /// <param name="id">Id of item</param>
        /// <returns>Data for single item</returns>
        /// <response code = "200">Return the item</response>
        /// <response code = "404">Item not found from database</response>
        /// 
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ItemDTO>> GetItem(long id)      
        {
            var item = await _service.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutItem(long id, ItemDTO item) 
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            //Tarkista, onko oikeus muokata

            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);
            if (!isAllowed)
            {
                return Unauthorized();
            }

            ItemDTO updatedItem = await _service.UpdateItemAsync(item);

            if (updatedItem == null)
            {
                return NotFound();
            }
            return NoContent();

        }

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ItemDTO>> PostItem(ItemDTO item)  

        {


            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);
            if (!isAllowed)
            {
                return Unauthorized();
            }

            ItemDTO newItem = await _service.CreateItemAsync(item);
            if (newItem == null)
            {
                return Problem();
            }

            return CreatedAtAction("GetItem", new { id = newItem.Id }, newItem);
        }
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
            return NotFound();
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(long id) 
        {
            //Tarkista, onko oikeus muokata
            ItemDTO item = new ItemDTO();
            item.Id = id;
            bool isAllowed = await _authenticationService.IsAllowed(this.User.FindFirst(ClaimTypes.Name).Value, item);
            if (!isAllowed)
            {
                return Unauthorized();
            }

            if (await _service.DeleteItemAsync(id))
            {
                return Ok();
            }
            return NotFound();

        }




    }
}