using System;
using LibraryCore.Controllers;
using LibraryCore.Extensions.SystemWebMvcController;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;
using LibraryCore.Util;
using LibraryCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Controllers
{
    public class CheckInControllerTest
    {
        private InMemoryRepository<Branch> branchRepo;
        private CheckInController controller;
        private InMemoryRepository<Holding> holdingRepo;
        private InMemoryRepository<Patron> patronRepo;
        private int someValidBranchId;
        private int someValidPatronId;

        public CheckInControllerTest()
        {
            holdingRepo = new InMemoryRepository<Holding>();

            branchRepo = new InMemoryRepository<Branch>();
            someValidBranchId = branchRepo.Create(new Branch() {Name = "b"});

            patronRepo = new InMemoryRepository<Patron>();

            controller = new CheckInController(branchRepo, holdingRepo, patronRepo);
        }

        public class CheckInGeneratesErrorTest : CheckInControllerTest
        {
            [Fact]
            public void WhenHoldingWithBarcodeDoesNotExist()
            {
                controller.Index(new CheckInViewModel
                    {Barcode = "NONEXISTENT:42", BranchId = someValidBranchId}); // as ViewResult;

                Assert.Equal("Invalid holding barcode.", controller.SoleErrorMessage(CheckInController.ModelKey));
            }

            [Fact]
            public void WhenHoldingBarcodeIsInvalid()
            {
                var result =
                    controller.Index(new CheckInViewModel
                        {Barcode = "BADFORMAT", BranchId = someValidBranchId}); //as ViewResult;

                Assert.Equal("Invalid holding barcode format.",
                    controller.SoleErrorMessage(CheckInController.ModelKey));
            }

            [Fact]
            public void WhenHoldingAlreadyCheckedIn()
            {
                holdingRepo.Create(new Holding {Classification = "X", CopyNumber = 1, BranchId = 1});

                var result = controller.Index(new CheckInViewModel {Barcode = "X:1", BranchId = someValidBranchId});
                // as ViewResult;

                Assert.Equal("Holding is already checked in.", controller.SoleErrorMessage(CheckInController.ModelKey));
            }
        }

        public class WhenCheckInSucceedsTest : CheckInControllerTest
        {
            private Holding aCheckedOutHolding;
            private DateTime now;
            
            private readonly ITestOutputHelper testOutputHelper;
            public WhenCheckInSucceedsTest(ITestOutputHelper testOutputHelper)
            {
                this.testOutputHelper = testOutputHelper;
                CreateCheckedOutHolding();
                CreateValidPatron();
                FixTimeService();
            }

            private void CreateCheckedOutHolding()
            {
                var aHoldingId = holdingRepo.Create(
                    new Holding
                    {
                        Classification = "ABC", CopyNumber = 1, BranchId = Branch.CheckedOutId,
                        HeldByPatronId = someValidPatronId
                    });
                aCheckedOutHolding = holdingRepo.GetByID(aHoldingId);
            }

            private void CreateValidPatron()
            {
                var someValidPatron = new Patron {Name = "X"};
                someValidPatronId = patronRepo.Create(someValidPatron);
            }

            private void FixTimeService()
            {
                now = DateTime.Now;
                TimeService.NextTime = now;
            }

            RedirectToRouteResult CheckInHolding()
            {
                return controller.Index(new CheckInViewModel
                        {Barcode = aCheckedOutHolding.Barcode, BranchId = someValidBranchId})
                    as RedirectToRouteResult;
            }

            [Fact]
            public void ThenHoldingBranchIsUpdated()
            {
                CheckInHolding();

                Assert.Equal(someValidBranchId, holdingRepo.GetByID(aCheckedOutHolding.Id).BranchId);
            }

            [Fact]
            public void ThenHoldingIsNotCheckedOut()
            {
                CheckInHolding();

                Assert.False(holdingRepo.GetByID(aCheckedOutHolding.Id).IsCheckedOut);
            }

            [Fact]
            public void ThenHoldingPatronIsCleared()
            {
                CheckInHolding();

                Assert.Equal(Holding.NoPatron, holdingRepo.GetByID(aCheckedOutHolding.Id).HeldByPatronId);
            }

            [Fact]
            public void ThenHoldingLastDateCheckedInIsUpdated()
            {
                CheckInHolding();

                Assert.Equal(now, holdingRepo.GetByID(aCheckedOutHolding.Id).LastCheckedIn);
            }

            [Fact]
            public void ThenRedirectsToIndex()
            {
                testOutputHelper.WriteLine($"hey ho");
                var redirectToRouteResult = CheckInHolding();
                testOutputHelper.WriteLine($"result: {redirectToRouteResult}");
                var routeValue = redirectToRouteResult.RouteValues["action"];
                testOutputHelper.WriteLine($"route value {routeValue}");
                Assert.Equal("Index", routeValue);
            }
        }

        // TODO exercise: fines on late checkin
    }
}