using System;

namespace NeuroSoft.Models
{
    public class LoginResponse
    {
        public string refresh { get; set; }
        public string access { get; set; }
        public UserData user { get; set; }
    }

    public class UserData
    {
        public int id { get; set; }
        public string username { get; set; }
        public string nombre_completo { get; set; }
        public string email { get; set; }
        public string rol { get; set; }
    }
}