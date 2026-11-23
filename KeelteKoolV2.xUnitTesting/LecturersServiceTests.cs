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
        /// <summary>
        /// Test kontrollib, et õpetaja lisamisel tagastab teenus uue objekti,
        /// millele on antud uus id ja loomise aeg ning kõik sisestatud andmed on alles.
        /// </summary>
        [Fact]
        public async Task Should_AddNewLecturer_WhenResultIsReturned()
        {
            //ülesseade
            LecturerDTO dto = MockLecturerDTOData();

            //tegevus
            var result = await Svc<ILecturersServices>().Create(dto);

            //kontroll
            Assert.NotNull(result); //teenus tagastas objekti
            Assert.NotEqual(Guid.Empty, result.Id); //objektile genereeriti id
            Assert.Equal(dto.FirstName, result.FirstName); //eesnimi ei kadunud ära
            Assert.Equal(dto.LastName, result.LastName); //perenimi ei kadunud ära
            Assert.Equal(dto.Qualifications, result.Qualifications); //kvalifikatsioon ei kadunud ära
            Assert.NotEqual(default(DateTime), result.CreatedAt); //loomise aeg on pandud
        }

        /// <summary>
        /// Test kontrollib, et kui ees- ja perenimi on tühjad, paneb teenus neile
        /// vaikimisi väärtused "Jane" ja "Doe" ning õpetaja salvestatakse ikkagi andmebaasi.
        /// </summary>
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
            Assert.NotNull(result); //objekt tehti ikkagi
            Assert.Equal("Jane", result.FirstName); //tühi eesnimi asendati
            Assert.Equal("Doe", result.LastName); //tühi perenimi asendati
            Assert.Equal("Magistrikraad, prantsuse keel", result.Qualifications); //muud andmed jäid samaks
            Assert.NotNull(fromDb); //õpetaja on andmebaasis olemas
            Assert.Equal("Jane", fromDb.FirstName); //andmebaasi salvestati vaikimisi nimi
        }

        /// <summary>
        /// Test kontrollib, et andmebaasi lisatud õpetaja leitakse DetailAsync meetodiga
        /// tema id järgi üles ja tagastatud andmed on samad mis lisamisel.
        /// </summary>
        [Fact]
        public async Task Should_ReturnLecturerDetails_WhenGuidIsNotNull()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();

            //tegevus
            var result = await Svc<ILecturersServices>().DetailAsync(createdLecturer.Id);

            //kontroll
            Assert.NotNull(result); //midagi leiti
            Assert.Equal(createdLecturer.Id, result.Id); //leiti sama id-ga õpetaja
            Assert.Equal(createdLecturer, result); //objekt on sama mis lisati
            Assert.Equal("Kadri", result.FirstName); //eesnimi on õige
            Assert.Equal("Mägi", result.LastName); //perenimi on õige
            Assert.Equal("Magistrikraad, prantsuse keel", result.Qualifications); //kvalifikatsioon on õige
        }

        /// <summary>
        /// Test kontrollib, et õpetaja andmete muutmisel salvestatakse uued andmed
        /// samale õpetajale ja ka andmebaasist uuesti lugedes on andmed uued.
        /// </summary>
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
            Assert.NotNull(result); //teenus tagastas objekti
            Assert.Equal(dto.Id, result.Id); //muudeti sama õpetajat
            Assert.Equal(dto.FirstName, result.FirstName); //eesnimi on uus
            Assert.Equal(dto.LastName, result.LastName); //perenimi on uus
            Assert.Equal(dto.Qualifications, result.Qualifications); //kvalifikatsioon on uus
            Assert.NotEqual(oldFirstName, result.FirstName); //vana eesnimi ei jäänud alles
            Assert.Equal(dto.FirstName, fromDb.FirstName); //andmebaasis on ka uus eesnimi
        }

        /// <summary>
        /// Test kontrollib, et õige id korral kustutatakse õpetaja andmebaasist,
        /// teenus tagastab kustutatud õpetaja ja teda ei leita enam üles.
        /// </summary>
        [Fact]
        public async Task Should_DeleteLecturer_WhenValidIDIsGiven()
        {
            //ülesseade
            Lecturer createdLecturer = await AddLecturerToDB();

            //tegevus
            var deletedLecturer = await Svc<ILecturersServices>().Delete(createdLecturer.Id);
            var result = await Svc<ILecturersServices>().DetailAsync(createdLecturer.Id);

            //kontroll
            Assert.Null(result); //õpetajat ei leita enam
            Assert.NotNull(deletedLecturer); //teenus tagastas kustutatud õpetaja
            Assert.Equal(createdLecturer.Id, deletedLecturer.Id); //kustutati õige õpetaja
            Assert.Equal(createdLecturer.LastName, deletedLecturer.LastName); //andmed on samad
            Assert.NotEqual(deletedLecturer, result); //kustutatud objekt ja otsingu tulemus erinevad
            Assert.False(Svc<KeelteKoolV2Context>().Lecturers.Any(x => x.Id == createdLecturer.Id)); //andmebaasis teda pole
        }

        /// <summary>
        /// Negatiivne test: kontrollib, et kui teenusele antakse null, siis õpetajat ei lisata,
        /// tagastatakse null ning juba olemasolev õpetaja jääb muutmata.
        /// </summary>
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
            Assert.Null(result); //teenus lükkas lisamise tagasi
            Assert.Equal(countBefore, countAfter); //andmebaasi ei lisandunud ridu
            Assert.NotNull(existingFromDb); //olemasolev õpetaja on alles
            Assert.Equal(existingLecturer.FirstName, existingFromDb.FirstName); //tema eesnimi ei muutunud
            Assert.Equal(existingLecturer.LastName, existingFromDb.LastName); //tema perenimi ei muutunud
        }

        /// <summary>
        /// Negatiivne test: kontrollib, et olematu või tühja id korral DetailAsync tagastab null,
        /// aga olemasolev õpetaja leitakse ikka üles.
        /// </summary>
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
            Assert.Null(resultWrongId); //olematu id-ga õpetajat ei leitud
            Assert.Null(resultEmptyId); //tühja id-ga õpetajat ei leitud
            Assert.NotEqual(existingLecturer.Id, wrongId); //vale id on tõesti teine id
            Assert.NotNull(resultExisting); //olemasolev õpetaja leitakse
            Assert.Equal(existingLecturer.Id, resultExisting.Id); //ja see on õige õpetaja
        }

        /// <summary>
        /// Negatiivne test: kontrollib, et olematu id korral Delete ei kustuta midagi,
        /// tagastab null ja ei viska viga ning olemasolev õpetaja jääb alles.
        /// </summary>
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
            Assert.Null(result); //midagi ei kustutatud
            Assert.Equal(countBefore, countAfter); //ridade arv ei muutunud
            Assert.NotNull(existingFromDb); //olemasolev õpetaja on alles
            Assert.Equal(existingLecturer.Id, existingFromDb.Id); //see on sama õpetaja
            Assert.Equal("Mägi", existingFromDb.LastName); //tema andmed on samad
        }

        /// <summary>
        /// Negatiivne test: kontrollib, et olematu id-ga õpetaja muutmisel tagastatakse null,
        /// uut õpetajat selle id-ga ei teki ja olemasoleva õpetaja andmed ei muutu.
        /// </summary>
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
            Assert.Null(result); //midagi ei muudetud
            Assert.Null(notCreated); //uut õpetajat ei tekkinud
            Assert.NotNull(existingFromDb); //olemasolev õpetaja on alles
            Assert.Equal("Kadri", existingFromDb.FirstName); //tema eesnimi ei muutunud
            Assert.NotEqual(dto.FirstName, existingFromDb.FirstName); //uut nime talle ei pandud
        }

        /// <summary>
        /// Test kontrollib, et õpetaja muutmisel jääb loomise aeg samaks (ka siis kui vormist
        /// seda ei tule) ja muutmise aeg läheb uuemaks.
        /// </summary>
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
            Assert.NotNull(result); //muutmine õnnestus
            Assert.Equal(createdLecturer.Id, result.Id); //muudeti sama õpetajat
            Assert.Equal(oldCreatedAt, result.CreatedAt); //loomise aeg ei muutunud
            Assert.True(result.ModifiedAt > oldModifiedAt); //muutmise aeg uuenes
            Assert.True(result.ModifiedAt > result.CreatedAt); //muutmise aeg on hilisem kui loomise aeg
            Assert.Equal("Bakalaureusekraad, eesti keel", result.Qualifications); //uus kvalifikatsioon salvestus
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
