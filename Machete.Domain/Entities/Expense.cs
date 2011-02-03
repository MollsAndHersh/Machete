﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Machete.Domain
{
    public class Expense
    {
       
        public int ExpenseId { get; set; }       
        public string  Transaction { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}