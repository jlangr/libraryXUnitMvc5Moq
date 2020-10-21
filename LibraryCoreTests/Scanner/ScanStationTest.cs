using System;
using System.Collections.Generic;
using System.Linq;
using LibraryCore.ControllerHelpers;
using LibraryCore.Models;
using LibraryCore.Models.Repositories;
using LibraryCore.Scanner;
using LibraryCore.Util;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.Scanner
{
    public class ScanStationTest
    {
        const string SomeBarcode = "QA123:1";

        readonly DateTime now = DateTime.Now;

        ScanStation scanner;
        IRepository<Holding> holdingRepo;
        IRepository<Patron> patronRepo;
        Mock<IClassificationService> classificationService;
        int somePatronId;

        public ScanStationTest()
        {
            holdingRepo = new InMemoryRepository<Holding>();
            patronRepo = new InMemoryRepository<Patron>();
            classificationService = new Mock<IClassificationService>();
            AlwaysReturnBookMaterial(classificationService);
            somePatronId = patronRepo.Create(new Patron { Name = "x" });

            // calling .Object on a mock for the first time is an expensive operation (~200+ ms)
            scanner = new ScanStation(1, classificationService.Object, holdingRepo, patronRepo);
        }

        void AlwaysReturnBookMaterial(Mock<IClassificationService> serviceMock)
        {
            serviceMock.Setup(service => service.Retrieve(It.IsAny<string>()))
                .Returns(new Material() { CheckoutPolicy = new BookCheckoutPolicy() });
        }

        void ScanNewMaterial(string barcode)
        {
            var classification = Holding.ClassificationFromBarcode(barcode);
            var isbn = "x";
            classificationService.Setup(service => service.Classification(isbn)).Returns(classification);
            scanner.AddNewMaterial(isbn);
        }

        Holding GetByBarcode(string barcode)
        {
            return HoldingsControllerUtil.FindByBarcode(holdingRepo, barcode);
        }

        void CheckOut(string barcode)
        {
            CheckOut(barcode, somePatronId);
        }

        void CheckOut(string barcode, int patronId)
        {
            CheckOut(barcode, patronId, now);
        }

        void CheckOut(string barcode, int patronId, DateTime dateTime)
        {
            TimeService.NextTime = dateTime;
            scanner.AcceptLibraryCard(patronId);
            TimeService.NextTime = dateTime;
            scanner.AcceptBarcode(barcode);
        }

        void CheckIn(string barcode)
        {
            CheckIn(barcode, now);
        }

        void CheckIn(string barcode, DateTime dateTime)
        {
            TimeService.NextTime = dateTime;
            scanner.AcceptBarcode(barcode);
        }

        public class NotRequiringCheckoutTest : ScanStationTest
        {
            [Fact]
            public void StoresHoldingAtBranchWhenNewMaterialAdded()
            {
                classificationService.Setup(service => service.Classification("anIsbn")).Returns("AB123");

                scanner.AddNewMaterial("anIsbn");

                Assert.Equal(scanner.BranchId, GetByBarcode("AB123:1").BranchId);
            }

            [Fact]
            public void CopyNumberIncrementedWhenNewMaterialWithSameIsbnAdded()
            {
                classificationService.Setup(service => service.Classification("anIsbn")).Returns("AB123");
                scanner.AddNewMaterial("anIsbn");

                scanner.AddNewMaterial("anIsbn");

                var holdingBarcodes = holdingRepo.GetAll().Select(h => h.Barcode);
                // Assert.That(holdingBarcodes, Is.EquivalentTo(new List<string> { "AB123:1", "AB123:2" }));
                Assert.Equal(new List<string> { "AB123:1", "AB123:2" }, holdingBarcodes);
            }

            [Fact]
            public void ThrowsWhenCheckingInCheckedOutBookWithoutPatronScan()
            {
                ScanNewMaterial(SomeBarcode);

                Assert.Throws<CheckoutException>(() => scanner.AcceptBarcode(SomeBarcode));
            }

            [Fact]
            public void PatronIdUpdatedWhenLibraryCardAccepted()
            {
                scanner.AcceptLibraryCard(somePatronId);

                Assert.Equal(somePatronId, scanner.CurrentPatronId);
            }

            [Fact]
            public void PatronIdClearedWhenCheckoutCompleted()
            {
                scanner.AcceptLibraryCard(somePatronId);

                scanner.CompleteCheckout();

                Assert.Equal(ScanStation.NoPatron, scanner.CurrentPatronId);
            }
        }

        public class WhenNewMaterialCheckdOutTest : ScanStationTest
        {
            public WhenNewMaterialCheckdOutTest()
            {
                ScanNewMaterial(SomeBarcode);
                CheckOut(SomeBarcode);
            }

            [Fact]
            public void HeldByPatronIdUpdated()
            {
                Assert.Equal(somePatronId, GetByBarcode(SomeBarcode).HeldByPatronId);
            }

            [Fact]
            public void CheckOutTimestampUpdated()
            {
                Assert.Equal(now, GetByBarcode(SomeBarcode).CheckOutTimestamp);
            }

            [Fact]
            public void IsCheckedOutMarkedTrue()
            {
                Assert.True(GetByBarcode(SomeBarcode).IsCheckedOut);
            }

            [Fact]
            public void RescanBySamePatronIsIgnored()
            {
                scanner.AcceptBarcode(SomeBarcode);

                Assert.Equal(somePatronId, GetByBarcode(SomeBarcode).HeldByPatronId);
            }

            [Fact]
            public void SecondMaterialCheckedOutAddedToPatron()
            {
                ScanNewMaterial("XX123:1");

                CheckOut("XX123:1");

                Assert.Equal(somePatronId, GetByBarcode(SomeBarcode).HeldByPatronId);
                Assert.Equal(somePatronId, GetByBarcode("XX123:1").HeldByPatronId);
            }

            [Fact]
            public void SecondPatronCanCheckOutSecondCopyOfSameClassification()
            {
                string barcode1Copy2 = Holding.GenerateBarcode(Holding.ClassificationFromBarcode(SomeBarcode), 2);
                ScanNewMaterial(barcode1Copy2);

                var patronId2 = patronRepo.Create(new Patron());
                scanner.AcceptLibraryCard(patronId2);
                scanner.AcceptBarcode(barcode1Copy2);

                Assert.Equal(patronId2, GetByBarcode(barcode1Copy2).HeldByPatronId);
            }

            [Fact]
            public void CheckInAtSecondBranchResultsInTransfer()
            {
                var newBranchId = scanner.BranchId + 1;
                var scannerBranch2 = new ScanStation(newBranchId, classificationService.Object, holdingRepo, patronRepo);

                scannerBranch2.AcceptBarcode(SomeBarcode);

                Assert.Equal(newBranchId, GetByBarcode(SomeBarcode).BranchId);
            }

            [Fact]
            public void LateCheckInResultsInFine()
            {
                scanner.CompleteCheckout();
                const int daysLate = 2;

                CheckIn(SomeBarcode, DaysPastDueDate(SomeBarcode, now, daysLate));

                Assert.Equal(RetrievePolicy(SomeBarcode).FineAmount(daysLate), patronRepo.GetByID(somePatronId).Balance);
            }

            private CheckoutPolicy RetrievePolicy(string barcode)
            {
                var classification = Holding.ClassificationFromBarcode(barcode);
                var material = classificationService.Object.Retrieve(classification);
                return material.CheckoutPolicy;
            }

            [Fact]
            public void CheckoutByOtherPatronSucceeds()
            {
                scanner.CompleteCheckout();
                var anotherPatronId = patronRepo.Create(new Patron());
                scanner.AcceptLibraryCard(anotherPatronId);

                CheckOut(SomeBarcode, anotherPatronId);

                Assert.Equal(anotherPatronId, GetByBarcode(SomeBarcode).HeldByPatronId);
            }

            [Fact]
            public void CheckoutByOtherPatronAssessesAnyFineOnFirst()
            {
                scanner.CompleteCheckout();
                var anotherPatronId = patronRepo.Create(new Patron());

                const int daysLate = 2;
                CheckOut(SomeBarcode, anotherPatronId, DaysPastDueDate(SomeBarcode, now, daysLate));

                Assert.Equal(RetrievePolicy(SomeBarcode).FineAmount(daysLate),
                    patronRepo.GetByID(somePatronId).Balance);
            }

            private DateTime DaysPastDueDate(string barcode, DateTime fromDate, int daysLate)
            {
                return fromDate.AddDays(RetrievePolicy(barcode).MaximumCheckoutDays() + daysLate);
            }
        }

        public class WhenMaterialCheckedInTest : ScanStationTest
        {
            public WhenMaterialCheckedInTest()
            {
                ScanNewMaterial(SomeBarcode);
                CheckOut(SomeBarcode);
                scanner.CompleteCheckout();
                CheckIn(SomeBarcode);
            }

            [Fact]
            public void PatronCleared()
            {
                Assert.Equal(Holding.NoPatron, GetByBarcode(SomeBarcode).HeldByPatronId);
            }

            [Fact]
            public void HoldingMarkedAsNotCheckedOut()
            {
                Assert.False(GetByBarcode(SomeBarcode).IsCheckedOut);
            }

            [Fact]
            public void CheckOutTimestampCleared()
            {
                Assert.Null(GetByBarcode(SomeBarcode).CheckOutTimestamp);
            }

            [Fact]
            public void LastCheckedInTimestampUpdated()
            {
                Assert.Equal(now, GetByBarcode(SomeBarcode).LastCheckedIn);
            }
        }
    }
}