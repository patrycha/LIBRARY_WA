﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LIBRARY_WA.Models
{
    public class Book_DTO
    {
        public Book_DTO(int book_id, string title, string isbn, string author_fullname, string year, string language, string type, string description, bool is_available)
        {
            this.book_id = book_id;
            this.title = title;
            this.isbn = isbn;
            this.author_fullname = author_fullname;
            this.year = year;
            this.language = language;
            this.type = type;
            this.description = description;
            this.is_available = is_available;
        }

        public int book_id { get; set; }

        [MaxLength(50, ErrorMessage = "Za długi tytuł")]
        public string title { get; set; }

        [RegularExpression(@"\d{13}", ErrorMessage = "Niepoprawny format numer ISBN")]
        [StringLength(13)]
        public string isbn { get; set; }

        [MaxLength(100, ErrorMessage = "Za długie nazwisko")]
        public string author_fullname { get; set; }

        [RegularExpression(@"\d{4}", ErrorMessage = "Niepoprawny format roku wydania książki")]
        public String year { get; set; }

        [MaxLength(20, ErrorMessage = "Za długi język")]
        public string language { get; set; }

        [MaxLength(30, ErrorMessage = "Za długi typ")]
        public string type { get; set; }

        [MaxLength(300)]
        [System.ComponentModel.DefaultValue("Brak opisu")]
        public string description { get; set; }


        //czy istnieje w bibliotece
        [System.ComponentModel.DefaultValue(true)]
        public Boolean is_available { get; set; }
    }
}
