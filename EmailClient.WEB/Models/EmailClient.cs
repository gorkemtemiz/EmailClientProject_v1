using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmailClients.WEB.Models
{
    public class EmailClient
    {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Surname { get; set; }
      public string Nickname { get; set; }
      public int Age { get; set; }
      public string Email { get; set; }
     
    }
}