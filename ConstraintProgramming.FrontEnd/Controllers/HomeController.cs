using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConstraintProgramming.Entities;
using ConstraintProgramming.Entities.Solver;

namespace ConstraintProgramming.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetProducts(UserFilter filter) {
            ProductRecommender rec = new ProductRecommender(filter);
            return Json(rec.GetProducts());
        }

    }
}