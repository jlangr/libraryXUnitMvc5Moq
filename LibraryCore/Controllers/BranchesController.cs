using Library.Models;
using Library.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace Library.Controllers
{
    public class BranchesController : Controller
    {
        IRepository<Branch> repository;

        public BranchesController()
        {
            repository = new EntityRepository<Branch>(db => db.Branches);
        }

        public BranchesController(IRepository<Branch> repository)
        {
            this.repository = repository;
        }

        // GET: Branches
        public ActionResult Index()
        {
            return View(repository.GetAll());
        }

        // GET: Branches/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Branches/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,Name")] Branch branch)
        {
            if (ModelState.IsValid)
            {
                repository.Create(branch);
                return RedirectToAction("Index");
            }

            return View(branch);
        }

        // GET: Branches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var branch = repository.GetByID(id.Value);
            if (branch == null)
                return NotFound();
            return View(branch);
        }

        // POST: Branches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,Name")] Branch branch)
        {
            if (ModelState.IsValid)
            {
                repository.Save(branch);
                return RedirectToAction("Index");
            }
            return View(branch);
        }

        // GET: Branches/Delete/5
        public ActionResult Delete(int? id)
        {
            return Edit(id);
        }

        // POST: Branches/Delete/5
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.ActionName("Delete")]
        [Microsoft.AspNetCore.Mvc.ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            repository.Delete(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
