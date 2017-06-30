using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AdminAPI.Models;
using System.Linq;
using System.Threading;

namespace AdminAPI.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly UserContext _context;
        private string generateSessionToken(){
            string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string[] result = new string[128];
            Random random = new Random();
            for(int i = 0; i < result.Length; i++){
                result[i] = input[random.Next(0, input.Length)].ToString();
            }
            return string.Join("", result);
        }

        public UserController(UserContext context)
        {
            _context = context;
        }
        [HttpGet("show", Name="Show")]
        public ActionResult Show(){
            if(Request.Cookies["admin-api-session-token"] == null){
                return RedirectToAction("Login", "User");
            }
            var users = _context.Users.Where(u => u.SessionToken == Request.Cookies["admin-api-session-token"]);

            if(users.Count() > 0){
                ViewBag.Username = users.First().Username;
                return View();
            }else{
                return RedirectToAction("Login", "User");
            }
        }

        [HttpGet("new", Name="New")]
        public IActionResult New(){
            string pastSessionToken = Request.Cookies["admin-api-session-token"];
            if(pastSessionToken != null){
                var existingUser = _context.Users.Where(u => u.SessionToken == pastSessionToken);
                if(existingUser.Count() > 0){
                    return RedirectToAction("Show", "User");
                }
            }
            return View();
        }
        [HttpPost("create", Name="Create")]
        public ActionResult Create(){
            string userName = Request.Form["username"];
            string passwordSalt = BCrypt.GenerateSalt();
            string passwordDigest = BCrypt.HashPassword(Request.Form["password"], passwordSalt);
            string createdAt = DateTime.Now.ToString();
            string sessionToken = generateSessionToken();
            _context.Users.Add(new User { 
                Username = userName,
                PasswordDigest = passwordDigest,
                PasswordSalt = passwordSalt,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                SessionToken = sessionToken,
                Activities = "",
            });
            _context.SaveChanges();
            Globals.pushStore.addSubscription(userName);
            
            Response.Cookies.Append("admin-api-session-token", sessionToken);
            return RedirectToAction("Show", "User");
        }

        [HttpGet("login", Name="Login")]
        public IActionResult Login(){
            string pastSessionToken = Request.Cookies["admin-api-session-token"];
            if(pastSessionToken != null){
                var existingUser = _context.Users.Where(u => u.SessionToken == pastSessionToken);
                if(existingUser.Count() > 0){
                    return RedirectToAction("Show", "User");
                }
            }
            return View();
        }

        [HttpPost("create_session", Name="CreateSession")]
        public IActionResult CreateSession(){
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            var user = _context.Users.Where(u => u.Username == username);
            if(user.Count() < 1){
                return RedirectToAction("New", "User");
            }
            string passwordDigest = user.First().PasswordDigest;

            if(BCrypt.CheckPassword(password, passwordDigest)){
                string sessionToken = generateSessionToken();
                user.First().SessionToken = sessionToken;
                Response.Cookies.Delete("admin-api-session-token");
                Response.Cookies.Append("admin-api-session-token", sessionToken);
                _context.SaveChanges();

                return RedirectToAction("Show", "User");
            }else{
                return RedirectToAction("Login", "User");
            }
        }

        [HttpPost("delete_session", Name="DeleteSession")]
        public ActionResult DeleteSession(){
            var sessionToken = generateSessionToken();
            var user = _context.Users.Where(u => u.SessionToken == Request.Cookies["admin-api-session-token"]);
            user.First().SessionToken = sessionToken;
            _context.SaveChanges();
            Response.Cookies.Delete("admin-api-session-token");

            return RedirectToAction("Login", "User");
        }
        [HttpGet("fetch/{id}", Name="Fetch")]
        public List<string> Fetch(string id){            
            while(!Globals.pushStore.shouldUpdate(id)){
                Thread.Sleep(500);
            }
            
            return Globals.pushStore.newInfo(id);
        }
        [HttpPost("post/id={userId}&content={content}", Name="Post")]
        public void Post(string userId, string content){
            var user = _context.Users.Where(u => u.SessionToken == Request.Cookies["admin-api-session-token"]);
            user.First().Activities += (content + ";");
            _context.SaveChanges();
            Globals.pushStore.addInfo(userId, content);
        }
        [HttpDelete("delete/id={userId}", Name="Delete")]
        public void Delete(string userId){
            Globals.pushStore.removeSubscription(userId);
        }
        [HttpGet("user_status/id={userId}", Name="UserStatus")]
        public List<string> UserStatus(string userId){
            return Globals.pushStore.userStatus(userId);
        }
        [HttpPost("silence/id={userId}&targetId={targetUserId}", Name="Silence")]
        public void Silence(string userId, string targetUserId){
            Globals.pushStore.silence(userId, targetUserId);
        }
        [HttpPost("unsilence/id={userId}&targetId={targetUserId}", Name="Unsilence")]
        public void Unsilence(string userId, string targetUserId){
            Globals.pushStore.unsilence(userId, targetUserId);
        }
    }
}