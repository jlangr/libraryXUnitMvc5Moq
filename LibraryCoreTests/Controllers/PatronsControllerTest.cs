using System.Collections.Generic;
using System.Linq;
using LibraryCore.Controllers;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;
using LibraryCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Controllers
{
    public class PatronsControllerTest
    {
        private PatronsController controller;
        private InMemoryRepository<Patron> patronRepo;
        private InMemoryRepository<Holding> holdingRepo;

        public PatronsControllerTest()
        {
            patronRepo = new InMemoryRepository<Patron>();
            holdingRepo = new InMemoryRepository<Holding>();
            controller = new PatronsController(patronRepo, holdingRepo);
        }

        public class DetailsTest: PatronsControllerTest
        {
            [Fact]
            public void ReturnsNotFoundWhenNoPatronAdded()
            {
                var view = controller.Details(0);

                Assert.IsType<NotFoundResult>(view);
            }

                // TODO
            [Fact]
            public void ReturnsBadRequestErrorWhenIdNull()
            {
                // var view = controller.Details(null);

//                Assert.Equal(400, (view as HttpStatusCodeResult).StatusCode);
            }

            [Fact]
            public void ReturnsViewOnPatronWhenFound()
            {
                var id = patronRepo.Create(new Patron() { Name = "Jeff" }); 

                var view = controller.Details(id);

                var viewPatron = (view as ViewResult)?.Model as Patron;
                Assert.Equal("Jeff", viewPatron.Name);
            }
        }

        public class HoldingsTest: PatronsControllerTest
        {
            IRepository<Branch> branchRepo = new InMemoryRepository<Branch>();
            CheckOutController checkoutController;
            int patronId;
            int branchId;

            public HoldingsTest()
            {
                CreateCheckoutController();
                CreatePatron();
                CreateBranch();
            }
            
            private void CreateCheckoutController()
            {
                checkoutController = new CheckOutController(branchRepo, holdingRepo, patronRepo);
            }

            private void CreatePatron()
            {
                patronId = patronRepo.Create(new Patron());
            }

            private void CreateBranch()
            {
                branchId = branchRepo.Create(new Branch());
            }

            [Fact]
            public void ReturnsEmptyWhenPatronHasNotCheckedOutBooks()
            {
                var view = (controller.Holdings(patronId) as ViewResult)?.Model as IEnumerable<Holding>;

                Assert.True(!view?.Any());
            }

            [Fact]
            public void ReturnsListWithCheckedOutHolding()
            {
                int holdingId1 = CreateCheckedOutHolding(patronId, checkoutController, 1);
                int holdingId2 = CreateCheckedOutHolding(patronId, checkoutController, 2);

                var view = (controller.Holdings(patronId) as ViewResult)?.Model as IEnumerable<Holding>;

                Assert.Equal(new List<int> { holdingId1, holdingId2 }, view.Select(h => h.Id));
            }

            private int CreateCheckedOutHolding(int id, CheckOutController controller, int copyNumber)
            {
                var holdingId = holdingRepo.Create(new Holding { Classification = "X", CopyNumber = copyNumber, BranchId = branchId });
                var checkOutViewModel = new CheckOutViewModel { Barcode = $"X:{copyNumber}", PatronId = id };
                controller.Index(checkOutViewModel);
                return holdingId;
            }
        }
        
        public class Index: PatronsControllerTest
        {
            [Fact]
            public void RetrievesViewOnAllPatrons()
            {
                patronRepo.Create(new Patron { Name = "Alpha" }); 
                patronRepo.Create(new Patron { Name = "Beta" }); 

                var view = controller.Index();

                var patrons = (view as ViewResult)?.Model as IEnumerable<Patron>;
                Assert.Equal(new string[] { "Alpha", "Beta" }, patrons?.Select(p => p.Name));
            }
        }

        public class Create: PatronsControllerTest
        {
            [Fact]
            public void CreatesPatronWhenModelStateValid()
            {
                var patron = new Patron { Name = "Venkat" };

                controller.Create(patron);

                var retrieved = patronRepo.GetAll().First();
                Assert.Equal("Venkat", retrieved.Name);
            }

            [Fact]
            public void RedirectsToIndexWhenModelValid()
            {
                var result = controller.Create(new Patron()) as RedirectToRouteResult;

                Assert.Equal("Index", result?.RouteValues["action"]);
            }

            [Fact]
            public void AddsNoPatronWhenModelStateInvalid()
            {
                controller.ModelState.AddModelError("", "");

                controller.Create(new Patron());

                Assert.False(patronRepo.GetAll().Any());
            }
        }
    }
}
