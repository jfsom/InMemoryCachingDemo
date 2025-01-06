using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace InMemoryCachingDemo.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        public string Name { get; set; }
        public List<State> States { get; set; }
    }
}