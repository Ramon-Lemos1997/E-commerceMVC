﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public AdminController()
        {
            
        }

        //----------------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult Index() => View();
        
    }
}
