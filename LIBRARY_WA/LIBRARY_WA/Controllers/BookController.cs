﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LIBRARY_WA.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace LIBRARY_WA.Data
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BookController(LibraryContext context)
        {
            _context = context;
        }

        // get data to combobox
        [HttpGet]
        public List<String> GetAuthor()
        {
            return _context.Book.Where(a=>a.is_available==true).Select(a => a.author_fullname).Distinct().ToList();
        }

        [HttpGet]
        public List<String> GetBookType()
        {
            return _context.Book.Where(a => a.is_available == true).Select(a => a.type).Distinct().ToList();
        }

        [HttpGet]
        public List<String> GetLanguage()
        {
            return _context.Book.Where(a => a.is_available == true).Select(a => a.language).Distinct().ToList();
        }

        [HttpGet("{isbn}")]
        public IEnumerable<Book> IfISBNExists([FromRoute] String isbn)
        {
            return _context.Book.Where(a => (a.isbn == isbn)); //&& (a.is_available == true)
        }



        //BOOK function

        [HttpPost, Authorize(Roles = "l")]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Book.Where(a => a.isbn == book.isbn).Count() > 0)
            {
                return BadRequest(new {alert= "Książka o danym ISBN już istnieje w bazie danych." });
            }
            
            _context.Book.Add(book);
            await _context.SaveChangesAsync();
            Volume volume = new Volume();
            volume.is_free = true;
            volume.book_id = book.book_id;
            _context.Volume.Add(volume);
            _context.SaveChanges();

            return CreatedAtAction("AddBook", new { id = book.book_id }, book);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = await _context.Book.FindAsync(id);

            if (book == null)
            {
                return NotFound(new { alert = "Książka o danym id nie istnieje!" });
            }

            return Ok(book);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVolumeByBookId([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var volume = _context.Volume.Where(a=>a.book_id==id);

            if (volume == null)
            {
                return NotFound();
            }

            return Ok(volume);
        }


        [HttpPost]
        public async Task<IActionResult> SearchBook([FromBody] String[] search)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            String[] name = { "book_id", "ISBN", "title", "author_fullname", "year", "language", "type" };
            String sql = "Select * from Book where is_available=true ";
            for (int i = 0; i < search.Length; i++)
            {
                if (search[i] != "%")
                {
                    if (name[i] == "title")
                    {
                        String[] words = search[i].ToLower().Split(" ");
                        for (int j = 0; j < words.Length; j++)
                        {
                            sql += " and title like('%" + words[j] + "%') ";
                        }
                    }
                    else
                    {
                        sql += "and " + name[i] + "='" + search[i] + "'";
                    }
                }

            }
            
            var book = _context.Book.FromSql(sql);

            return Ok(book);
        }


        [HttpDelete("{id}"), Authorize(Roles = "l")]
        public async Task<IActionResult> RemoveBook([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { alert = "Nie znaleziono książki o danym id." });
            }

            if (_context.Rent.Where(a => a.book_id == id).Count() > 0)
            {
                return NotFound(new { alert = "Dana książka jest wypożyczona. Nie można jej usunąć" });
            }


            _context.Reservation.FromSql("DELETE from Reservation where book_id='" + id + "'");

            foreach (Volume volume in _context.Volume.Where(a => a.book_id == id))
            {
                _context.Volume.Remove(volume);
            }
            book.is_available = false;
            //usuń wszystkie rezerwacje

            await _context.SaveChangesAsync();
            return Ok(book);
        }
        
        //Volume function
        [HttpPost, Authorize(Roles = "l")]//, ]
        public async Task<IActionResult> AddVolume([FromBody] int id)
        {
            Volume volume = new Volume();
            volume.is_free = true;
            volume.book_id = id;
            _context.Volume.Add(volume);
            await _context.SaveChangesAsync();
            return CreatedAtAction("AddVolume", new { id = volume.volume_id }, volume);
            // return Ok(2);
        }

        [HttpGet]
        public async Task<IActionResult> GetVolume()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var volume = _context.Volume;

            return Ok(volume);
        }

        //-----------------------


        [HttpDelete("{id}"), Authorize(Roles = "l")]//
        public async Task<IActionResult> RemoveVolume([FromRoute] int id)
        {
            //jeśli ma wypożyczone książki to komunikat, że nie można usunąć użytkownika bo ma nie wszystkie książki oddane, 
            //a jesli usunięty to zmienia isValid na false
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (_context.Volume.Where(a => a.volume_id == id).Count() == 0)
            {
                return NotFound(new { alert = "Egzemplarz o danym id nie istnieje!" });
            }

            var volume = _context.Volume.Where(a => a.volume_id == id).FirstOrDefault();

            if (_context.Rent.Where(a => a.volume_id == id).Count() > 0)
            {
                return NotFound(new { alert = "Dany egzemplarz jest wypożyczony. Nie można go usunąć!" });
            }

            if (_context.Reservation.Where(a => a.volume_id == id && a.is_active==true).Count() > 0)
            {
                Reservation reservation = _context.Reservation.Where(a => a.volume_id == id && a.is_active == true).FirstOrDefault();

                foreach (Reservation reserv in _context.Reservation.Where(a => a.book_id == volume.book_id && a.is_active == false)) {
                    reserv.queue = reserv.queue + 1;
                }
                if(_context.Volume.Where(a => a.book_id == volume.book_id).Count() > 1)
                {
                    var n = _context.Volume.Where(a => a.book_id == volume.book_id && a.volume_id!=id).FirstOrDefault();
                    Reservation r = new Reservation(reservation.user_id, reservation.title, reservation.isbn, reservation.book_id, n.volume_id, reservation.start_date, reservation.expire_date, 1, false);
                    r.reservation_id = reservation.reservation_id;
                    _context.Reservation.Remove(reservation);
                    _context.SaveChanges();
                    _context.Reservation.Add(r);
                }
                else
                {
                    _context.Reservation.Remove(reservation);
                }
               
                _context.SaveChanges();
            }
            
            _context.Volume.Remove(volume);
            await _context.SaveChangesAsync();

            return Ok(volume);
        }


        [HttpPut, Authorize(Roles = "l,r")]
        public async Task<IActionResult> ReserveBook([FromBody] int[] data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
           
            if (_context.Book.Where(a => a.book_id == data[0]).Count() == 0)
            {
                return NotFound(new { alert = "Nie znaleziono książki o podanym id" });
            }
          
            if (_context.User.Where(a => a.user_id == (data[1])).Count() == 0)
            {
                return NotFound(new { alert = "Nie znaleziono użytkownika o podanym id" });
            }

            if (_context.Reservation.Where(a => a.book_id == data[0] && a.user_id==data[1]).Count()> 0)
            {
                return BadRequest(new { alert = "Użytkownik ma już zarezerwowaną tę książkę!" });
            }

            if (_context.Volume.Where(a => a.book_id == data[0]).Count() == 0)
            {
                return NotFound(new { alert = "Książka nie ma żadnych egzemplarzy!" });
            }
          
            Book book = _context.Book.Where(a => a.book_id == (data[0])).FirstOrDefault();
           
            int volume_id = _context.Volume.Where(a => a.book_id == data[0]).FirstOrDefault().volume_id;
          
            DateTime start_date = DateTime.Now;
            DateTime expire_date;
            int queue;
            Boolean is_active = true;
          
            
            if (_context.Volume.Where(a => a.book_id == data[0] && a.is_free == true).Count() == 0)
            {
                if (_context.Reservation.Where(a => a.book_id == data[0]).OrderByDescending(a => a.queue).FirstOrDefault() == null)
                    queue = 1;
                else
                 queue = _context.Reservation.Where(a => a.book_id == data[0]).OrderByDescending(a => a.queue).FirstOrDefault().queue + 1;
                is_active = false;
                expire_date = DateTime.Now;
            }
            else
            {
                volume_id = _context.Volume.Where(a => a.book_id == Convert.ToInt32(data[0]) && a.is_free == true).FirstOrDefault().volume_id;
                expire_date = DateTime.Now.AddDays(14);
                _context.Volume.Where(a => a.volume_id == volume_id).FirstOrDefault().is_free = false;
                queue = 0;
              
            }
          
            Reservation reservation = new Reservation(data[1], book.title, book.isbn, book.book_id, volume_id, start_date, expire_date, queue, is_active);
            _context.Reservation.Add(reservation);
            await _context.SaveChangesAsync();
           
            return Ok(reservation);
        }



        [HttpPut, Authorize(Roles = "l")] //, 
        public async Task<IActionResult> RentBook([FromBody] int[] reservation_id)
        {
           
          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            if (_context.Reservation.Where(a => a.reservation_id == reservation_id[0]).Count() == 0)
            {
                return BadRequest(new { alert="Nie ma takiej rezerwacji" });
            }

            Reservation reservation = _context.Reservation.Where(a => a.reservation_id == reservation_id[0]).FirstOrDefault();
            if (_context.Volume.Where(a => a.is_free == true).Count() == 0)
            {
                return BadRequest(new { alert = "Nie ma takiego egzemplarza" });
            }
            int volume_id = _context.Volume.Where(a => a.is_free == true).FirstOrDefault().volume_id;
            //zmień is free na false
            _context.Volume.Where(a => a.is_free == true).FirstOrDefault().is_free = false;
            Rent rent = new Rent(reservation.user_id, reservation.book_id, reservation.title, reservation.isbn, volume_id, DateTime.Now, DateTime.Now.AddMonths(1));
            await _context.Rent.AddAsync(rent);
            _context.Reservation.Remove(reservation);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Authorize(Roles = "l")] //, , 
        public async Task<IActionResult> ReturnBook([FromBody] int[] rent_id)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Rent.Where(a => a.rent_id == rent_id[0]).Count() == 0)
            {
                return BadRequest(new { alert = "Nie ma takiego wypożyczenia" });
            }

            Rent rent = _context.Rent.Where(a => a.rent_id == rent_id[0]).FirstOrDefault();
            Volume volume = _context.Volume.Where(a => a.volume_id == rent.volume_id).FirstOrDefault();
            Reservation[] reservations = _context.Reservation.Where(a => a.book_id == rent.book_id).ToArray();
            Renth renth = new Renth(rent.user_id, rent.title, rent.isbn, rent.book_id, rent.volume_id, rent.start_date, DateTime.Now);
            //wstaw do historii rezerwacji
            await _context.Renth.AddAsync(renth);
            await _context.SaveChangesAsync();
            
            foreach (Reservation reservation in reservations)
            {
                reservation.queue = reservation.queue - 1;
                if (reservation.queue == 0)
                {
                    reservation.is_active = true;
                    reservation.expire_date = DateTime.Now.AddDays(8);
                    reservation.volume_id = volume.volume_id;
                }
            }

            //usuń z rent
            _context.Rent.Remove(rent);
            _context.SaveChanges();
            return Ok(new { message = "Książka została poprawnie zwrócona" });
        }

        [HttpGet("{user_id}"), Authorize(Roles = "l,r")]
        public async Task<IActionResult> GetSuggestion([FromRoute] int user_id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.User.Where(a => a.user_id == user_id).Count() == 0)
            {
                return BadRequest(new { alert = "Nie ma takiego użytkownika" });
            }
            var sql = "CALL Get_suggestion(" + user_id + ")";
            _context.Database.ExecuteSqlCommand(sql);
            var suggestion = _context.Suggestion;
            return Ok(suggestion);
        }


        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.book_id == id);
        }

        [HttpPut, Authorize(Roles = "l")]
        public async Task<IActionResult> UpdateBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Book.Where(a => a.book_id == book.book_id).Count() == 0)
            {
                return NotFound("Nie znaleziono książki!");
            }
         
            _context.Entry(book).State = EntityState.Modified;
            _context.Book.Update(book);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            }
            return Ok();
        }
        
        [HttpPut]
        public async Task<IActionResult> EditBook( [FromBody] Book book)
        {
           if (!ModelState.IsValid)
           {
               return BadRequest(ModelState);
           }

           _context.Entry(book).State = EntityState.Modified;

           try
           {
               await _context.SaveChangesAsync();
           }
           catch (DbUpdateConcurrencyException)
           {
               if (_context.Book.Where(a=>a.book_id==book.book_id).Count()==0)
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
        }
    }



