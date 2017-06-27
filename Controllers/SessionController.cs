using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AdminAPI.Models;
using System.Linq;

namespace AdminAPI.Controllers
{
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly UserContext _context;
        private string generateSessionToken(){
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            string[] result = new string[16];
            Random random = new Random();
            for(int i = 0; i < result.Length; i++){
                result[i] = input[random.Next(0, input.Length)].ToString();
            }
            return string.Join("", result);
        }
        public SessionController(){

        }

        
    }
}