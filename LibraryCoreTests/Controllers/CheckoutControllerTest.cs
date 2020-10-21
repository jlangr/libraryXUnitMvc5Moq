using System;
using Library.Controllers;
using Library.Extensions.SystemWebMvcController;
using Library.Models;
using Library.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Controllers
{
    public class CheckOutControllerTest
    {
        CheckOutController controller;
        IRepository<Holding> holdingRepo;
        IRepository<Patron> patronRepo;
        CheckOutViewModel checkout;
        int someValidBranchId;
        private int someValidPatronId;
        private Holding aCheckedInHolding;

        public CheckOutControllerTest()
        {
            holdingRepo = new InMemoryRepository<Holding>();

            var branchRepo = new InMemoryRepository<Branch>();
            someValidBranchId = branchRepo.Create(new Branch() { Name = "b" });

            patronRepo = new InMemoryRepository<Patron>();
            someValidPatronId = patronRepo.Create(new Patron { Name = "x" });

            controller = new CheckOutController(branchRepo, holdingRepo, patronRepo);
            checkout = new CheckOutViewModel();
            
            CreateCheckedInHolding();
        }

        public void CreateCheckedInHolding()
        {
            aCheckedInHolding = new Holding { Classification = "ABC", CopyNumber = 1 };
            aCheckedInHolding.CheckIn(DateTime.Now, someValidBranchId);
        }

        public class CheckOutGeneratesError : CheckOutControllerTest
        {
            [Fact]
            public void WhenPatronIdInvalid()
            {
                controller.Index(new CheckOutViewModel { PatronId = 0, Barcode = aCheckedInHolding.Barcode }); // as ViewResult;

                Assert.Equal("Invalid patron ID.", controller.SoleErrorMessage(CheckOutController.ModelKey));
            }

            [Fact]
            public void WhenNoHoldingFoundForBarcode()
            {
                controller.Index(new CheckOutViewModel { Barcode = "NONEXISTENT:1", PatronId = someValidPatronId }); // as ViewResult;

                Assert.Equal("Invalid holding barcode.", controller.SoleErrorMessage(CheckOutController.ModelKey));
            }

            [Fact]
            public void WhenHoldingAlreadyCheckedOut()
            {
                holdingRepo.Create(aCheckedInHolding);
                checkout = new CheckOutViewModel { Barcode = aCheckedInHolding.Barcode, PatronId = someValidPatronId };
                controller.Index(checkout);

                controller.Index(checkout); // as ViewResult;

                Assert.Equal("Holding is already checked out.", controller.SoleErrorMessage(CheckOutController.ModelKey));
            }

            [Fact]
            public void WhenBarcodeHasInvalidFormat()
            {
                controller.Index(new CheckOutViewModel { Barcode = "HasNoColon", PatronId = someValidPatronId }); // as ViewResult;

                Assert.Equal("Invalid holding barcode format.", controller.SoleErrorMessage(CheckOutController.ModelKey));
            }
        }

        public class WhenCheckoutSucceedsTest: CheckOutControllerTest
        {
            private int holdingId;

            public WhenCheckoutSucceedsTest()
            {
                holdingId = holdingRepo.Create(aCheckedInHolding);
                checkout = new CheckOutViewModel { Barcode = aCheckedInHolding.Barcode, PatronId = someValidPatronId };
            }

            [Fact]
            public void ThenMarksHoldingAsCheckedOut()
            {
                controller.Index(checkout);

                var retrievedHolding = holdingRepo.GetByID(holdingId);
                Assert.True(retrievedHolding.IsCheckedOut);
            }

            [Fact]
            public void ThenRedirectsToIndex()
            {
                var result = controller.Index(checkout) as RedirectToRouteResult;

                Assert.Equal("Index", result?.RouteValues["action"]);
            }
        }
    }
}
