﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LIBRARY_WA.Models;
using Microsoft.AspNetCore.Cors;

namespace LIBRARY_WA.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly LibraryContext context;

        public UserController(UserContext context)
        {
            this._context = context;//.UserContext;
         //   this.context = context;
        }



        // GET: api/User
        [HttpPost]
        public String IsLogged([FromBody] User userData)
        {
            //  _context.User.Add(null, "admin', 'admin', '', '', '', '', '1989-12-09', '', '');

            if (_context.User.Where(u => u.login=="admin").First()!=null)// u => u.login==userData.login && u.password== userData.password).FirstOrDefault() != null)
            {
                return "istnieje";//_context.User.Where(u => user.login == u.login && user.password == u.password).FirstOrDefault();
            }
            else
            {
                return "nie istnieje";//new Models.User();
            }
          //  return "done";
        }

        //[HttpGet]
        //public IEnumerable<User> GetUser([FromBody] User user)
        //{
        //  //rom p in context.Professors
        //   // select p.Name).ToList()

        //    return _context.User;
        //}

        // GET: api/User/5
        [HttpGet("{id}")]
        public  IEnumerable<User> GetUser([FromRoute] string id)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //var user = await _context.User.FindAsync(id);

            //if (user == null)
            //{
            //    return NotFound();
            //}
            return _context.User;
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] Int32 id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id.ToString() == user.user_Id.ToString())
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> addUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.user_Id }, user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(Int32 id)
        {
            return _context.User.Any(e => e.user_Id.ToString() == id.ToString());
        }
    }
}