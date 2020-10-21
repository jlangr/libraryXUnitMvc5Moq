using LibraryCore.ControllerHelpers;
using LibraryCore.Controllers;
using LibraryCore.Extensions.SystemWebMvcController;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Controllers
{
    public class HoldingsControllerTest
    {
        HoldingsController controller;
        IRepository<Holding> holdingRepo;
        IRepository<Branch> branchRepo;

        public HoldingsControllerTest()
        {
            holdingRepo = new InMemoryRepository<Holding>();
            branchRepo = new InMemoryRepository<Branch>();
            controller = new HoldingsController(holdingRepo, branchRepo);
        }

        [Fact]
        public void CreatePersistsHolding()
        {
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1 });

            var holding = HoldingsControllerUtil.FindByBarcode(holdingRepo, "AB123:1");
            Assert.Equal("AB123:1", holding.Barcode);
        }

        [Fact]
        public void CreateReturnsGeneratedId()
        {
            var result = controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1 }) as RedirectToRouteResult;

            var id = (int)result.RouteValues["ID"];
            Assert.Equal("AB123:1", holdingRepo.GetByID(id).Barcode);
        }

        [Fact]
        public void CreateAssignsCopyNumberWhenNotProvided()
        {
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 0 });

            var holding = HoldingsControllerUtil.FindByBarcode(holdingRepo, "AB123:1");
            Assert.Equal("AB123:1", holding.Barcode);
        }

        [Fact]
        public void CreateUsesHighwaterCopyNumberWhenAssigning()
        {
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1, HeldByPatronId = 1});

            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 0, HeldByPatronId = 2 });

            var holding = HoldingsControllerUtil.FindByBarcode(holdingRepo, "AB123:2");
            Assert.Equal(2, holding.HeldByPatronId);
        }

        [Fact]
        public void CreateUsesHighwaterOnlyForBooksWithSameClassification()
        {
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1, HeldByPatronId = 1});

            controller.Create(new Holding() { Classification = "XX999", CopyNumber = 0, HeldByPatronId = 2 });

            var holding = HoldingsControllerUtil.FindByBarcode(holdingRepo, "XX999:1");
            Assert.Equal(2, holding.HeldByPatronId);
        }

        [Fact]
        public void CreateErrorsWhenAddingDuplicateBarcode()
        {
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1 });

            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1 }); // as ViewResult;

            Assert.Equal("Duplicate classification / copy number combination.", controller.SoleErrorMessage(HoldingsController.ModelKey));
        }

        // TODO ?? 
        [Fact]
        public void PopulatesViewModelWithBranchName()
        {
            var branchId = branchRepo.Create(new Branch { Name = "branch123" });
            controller.Create(new Holding() { Classification = "AB123", CopyNumber = 1, BranchId = branchId });

//            var model = (controller.Index() /* as ViewResult */).Model as IEnumerable<HoldingViewModel>;

        }
    }
}
