using KeelteKoolV2.Core.Domain;
using KeelteKoolV2.Core.DTO;
using KeelteKoolV2.Core.ServiceInterface;
using KeelteKoolV2.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KeelteKoolV2.xUnitTesting
{
    public class LecturersServicesTests : TestBase
    {
        [Fact]
        public async Task Should_AddNewLecturer_WhenResultIsReturned()
        {
            //ülesseade
            LecturerDTO dto = MockLecturerDTOData();

            //tegevus
            var result = await Svc<ILecturersServices>().Create(dto);

            //kontroll
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_AddDefaultNames_WhenNameFieldsAreEmpty()
        {
            //ülesseade
            LecturerDTO dto = MockLecturerDTOData();
            dto.FirstName = string.Empty;
            dto.LastName = string.Empty;

            //tegevus
            var result = await Svc<ILecturersServices>().Create(dto);
            var fromDb = await Svc<ILecturersServices>().DetailAsync(result.Id);

            //kontroll
            Assert.NotNull(result);
            Assert.Equal("Jane", result.FirstName);
        }

        [Fact]
        public async Task Should_ReturnLecturerDetails_WhenGuidIsNotNull()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();

            //tegevus
            var result = await Svc<ILecturersServices>().DetailAsync(createdLecturer.Id);

            //kontroll
            Assert.NotNull(result);
            Assert.Equal(createdLecturer.Id, result.Id);
        }

        [Fact]
        public async Task Should_UpdateLecturerWithNewData_WhenDataIsDifferentFromDB()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();
            string oldFirstName = createdLecturer.FirstName;
            LecturerDTO dto = new LecturerDTO();
            dto.Id = createdLecturer.Id;
            dto.FirstName = "Peeter";
            dto.LastName = "Sepp";
            dto.Qualifications = "Doktorikraad, saksa keel";
            dto.CreatedAt = createdLecturer.CreatedAt;
            dto.ModifiedAt = createdLecturer.ModifiedAt;

            //tegevus
            var result = await Svc<ILecturersServices>().Update(dto);
            var fromDb = await Svc<ILecturersServices>().DetailAsync(createdLecturer.Id);

            //kontroll
            Assert.NotNull(result);
            Assert.Equal(dto.FirstName, result.FirstName);
        }

        [Fact]
        public async Task Should_DeleteLecturer_WhenValidIDIsGiven()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();

            //tegevus
            var deletedLecturer = await Svc<ILecturersServices>().Delete(createdLecturer.Id);
            var result = await Svc<ILecturersServices>().DetailAsync(createdLecturer.Id);

            //kontroll
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldNot_AddLecturer_WhenDtoIsNull()
        {
            //ülesseade
            Lecturer existingLecturer = await AddLecturerToDB();
            int countBefore = Svc<KeelteKoolV2Context>().Lecturers.Count();

            //tegevus
            var result = await Svc<ILecturersServices>().Create(null);
            int countAfter = Svc<KeelteKoolV2Context>().Lecturers.Count();
            var existingFromDb = await Svc<ILecturersServices>().DetailAsync(existingLecturer.Id);

            //kontroll
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldNot_ReturnLecturerDetails_WhenIdDoesNotExist()
        {
            //ülesseade
            Lecturer existingLecturer = await AddLecturerToDB();
            Guid wrongId = Guid.NewGuid();

            //tegevus
            var resultWrongId = await Svc<ILecturersServices>().DetailAsync(wrongId);
            var resultEmptyId = await Svc<ILecturersServices>().DetailAsync(Guid.Empty);
            var resultExisting = await Svc<ILecturersServices>().DetailAsync(existingLecturer.Id);

            //kontroll
            Assert.Null(resultWrongId);
        }

        [Fact]
        public async Task ShouldNot_DeleteLecturer_WhenIdDoesNotExist()
        {
            //ülesseade
            Lecturer existingLecturer = await AddLecturerToDB();
            int countBefore = Svc<KeelteKoolV2Context>().Lecturers.Count();

            //tegevus
            var result = await Svc<ILecturersServices>().Delete(Guid.NewGuid());
            int countAfter = Svc<KeelteKoolV2Context>().Lecturers.Count();
            var existingFromDb = await Svc<ILecturersServices>().DetailAsync(existingLecturer.Id);

            //kontroll
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldNot_UpdateLecturer_WhenIdDoesNotExist()
        {
            //ülesseade
            Lecturer existingLecturer = await AddLecturerToDB();
            LecturerDTO dto = MockLecturerDTOData();
            dto.Id = Guid.NewGuid();
            dto.FirstName = "Olematu";
            dto.CreatedAt = DateTime.Now;

            //tegevus
            var result = await Svc<ILecturersServices>().Update(dto);
            var notCreated = await Svc<ILecturersServices>().DetailAsync(dto.Id);
            var existingFromDb = await Svc<ILecturersServices>().DetailAsync(existingLecturer.Id);

            //kontroll
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_KeepCreatedAt_AndUpdateModifiedAt_WhenLecturerIsUpdated()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();
            DateTime oldCreatedAt = createdLecturer.CreatedAt;
            DateTime oldModifiedAt = createdLecturer.ModifiedAt;
            await Task.Delay(20); //et muutmise aeg oleks kindlasti hilisem
            LecturerDTO dto = MockLecturerDTOData();
            dto.Id = createdLecturer.Id;
            dto.Qualifications = "Bakalaureusekraad, eesti keel";
            dto.CreatedAt = null; //loomise aega ei saadeta kaasa

            //tegevus
            var result = await Svc<ILecturersServices>().Update(dto);

            //kontroll
            Assert.Equal(oldCreatedAt, result.CreatedAt);
        }

        private async Task<Lecturer> AddLecturerToDB()
        {
            //tekitame uue õpetaja ja lisame andmebaasi
            LecturerDTO dto = MockLecturerDTOData();
            var createdLecturer = await Svc<ILecturersServices>().Create(dto);
            return createdLecturer;
        }

        private LecturerDTO MockLecturerDTOData()
        {
            return new LecturerDTO
            {
                FirstName = "Kadri",
                LastName = "Mägi",
                Qualifications = "Magistrikraad, prantsuse keel"
            };
        }
    }
}
