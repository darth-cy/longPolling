using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AdminAPI.Models;
using System.Linq;

namespace AdminAPI.Controllers
{
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly PostContext _context;

        public PostsController(PostContext context)
        {
            _context = context;

            if (_context.Posts.Count() == 0)
            {
                string timeStamp = DateTime.Now.ToString();
                _context.Posts.Add(new Post { Content = string.Format("在 " + timeStamp + ": 创建新初始文本。") });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<Post> GetAll()
        {
            return _context.Posts.ToList();
        }

        [HttpGet("new", Name = "CreatePost")]
        public IActionResult CreatePost(){
            return View();
        }

        [HttpPost("new", Name = "AddPost")]
        public IEnumerable<Post> AddPost(){
            string content = Request.Form["content"];
            string timeStamp = DateTime.Now.ToString();
            _context.Posts.Add(new Post { Content = string.Format("在 " + timeStamp + ": " + content + "。") });
            _context.SaveChanges();
            return _context.Posts.ToList();
        }

        [HttpGet("{id}", Name = "GetPost")]
        public IActionResult GetById(long id)
        {
            var item = _context.Posts.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }
    }
}