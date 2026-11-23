using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KeelteKoolV2.SeleniumTesting
{
    //järjesti ja PriorityOrderer on LanguageCourseFrontendTests failis, siin neid uuesti ei tehta
    [TestCaseOrderer("KeelteKoolV2.SeleniumTesting.PriorityOrderer", "KeelteKoolV2.SeleniumTesting")]
    public class LecturerFrontendTests
    {
        public static string thisRunGuid { get; set; }


        /// <summary>
        /// Test mis loob uue õpetaja, mille eesnimes on unikaalne id
        /// </summary>
        [Fact]
        [Order(3)]
        public void Should_NavigateToIndex_AddLecturerWithUniqueData_ReturnToIndex()
        {
            /* ülesseade */
            IWebDriver driver = TestSetup_NavigateToIndex();

            /* tegevus */
            IWebElement indexCreateButton = driver.FindElement(By.Id("IndexCreateNewLecturer_BTN"));
            indexCreateButton.Click(); //vajutame lisamise nuppu

            string uniqueId = DataEntry_Lecturer(driver); //sisestame andmed
            thisRunGuid = uniqueId;

            IWebElement createButton = driver.FindElement(By.Id("CU_Create_BTN"));
            createButton.Click(); //salvestame
            Thread.Sleep(1000);

            /* kontroll */
            //kogume andmed
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            string lecturerFirstNameData = "";
            string lecturerLastNameData = "";
            foreach (var row in rowsOfTable)
            {
                if (row.FindElement(By.Id("IndexTableLecturerFirstName")).Text == thisRunGuid)
                {
                    lecturerFirstNameData = thisRunGuid;
                    lecturerLastNameData = row.FindElement(By.Id("IndexTableLecturerLastName")).Text;
                }
            }
            //sulgeme akna
            Thread.Sleep(500);
            driver.Quit();
            //assertime
            Assert.Equal(uniqueId, lecturerFirstNameData);
            Assert.Equal("Tamm", lecturerLastNameData);
        }


        /// <summary>
        /// Test mis avab eelnevalt lisatud õpetaja andmed
        /// </summary>
        [Fact]
        [Order(4)]
        public void Should_NavigateToDetailsOfALecturer_WithPreviouslyUniqueData()
        {
            /* ülesseade */
            IWebDriver driver = TestSetup_NavigateToIndex();
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            bool isObjectFound = false;

            /* tegevus */
            foreach (var row in rowsOfTable)
            {
                var nameElement = row.FindElement(By.Id("IndexTableLecturerFirstName")).Text;
                if (nameElement == thisRunGuid)
                {
                    isObjectFound = true;
                    var clickAction = row
                        .FindElement(By.Id("IndexTableLecturerActions"))
                        .FindElement(By.Id("IndexTableLecturerDetails_BTN"));
                    clickAction.Click();
                    break;
                }
            }
            Thread.Sleep(1000);
            //võtame andmed vaatest
            string details_id = driver.FindElement(By.Id("DD_LecturerGuid")).Text;
            string details_firstName = driver.FindElement(By.Id("DD_LecturerFirstName")).Text;
            string details_lastName = driver.FindElement(By.Id("DD_LecturerLastName")).Text;
            string details_qualifications = driver.FindElement(By.Id("DD_LecturerQualifications")).Text;
            Thread.Sleep(500);
            driver.Quit();

            /* kontroll */
            Assert.True(isObjectFound);
            Assert.NotEmpty(details_id);
            Assert.Equal(thisRunGuid, details_firstName);
            Assert.Equal("Tamm", details_lastName);
            Assert.Equal("Inglise keele õpetaja", details_qualifications);
        }


        /// <summary>
        /// Test mis muudab eelnevalt lisatud õpetaja andmeid
        /// </summary>
        [Fact]
        [Order(6)]
        public void Should_UpdateDetailsOfUniqueLecturer_WithNewData()
        {
            /* ülesseade */
            IWebDriver driver = TestSetup_NavigateToIndex();
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            bool isObjectFound = false;

            /* tegevus */
            foreach (var row in rowsOfTable)
            {
                var nameElement = row.FindElement(By.Id("IndexTableLecturerFirstName")).Text;
                if (nameElement == thisRunGuid)
                {
                    isObjectFound = true;
                    var clickAction = row
                        .FindElement(By.Id("IndexTableLecturerActions"))
                        .FindElement(By.Id("IndexTableLecturerUpdate_BTN"));
                    clickAction.Click();
                    break;
                }
            }
            Thread.Sleep(500);
            DataClear_CreateUpdate_SaveName(driver); //puhastame väljad, nimi jääb alles
            DataEntry_Lecturer(driver, true, false); //sisestame uued andmed
            IWebElement updateClick = driver.FindElement(By.Id("CU_Update_BTN"));
            updateClick.Click();
            Thread.Sleep(1000);
            IWebElement goBack = driver.FindElement(By.Id("CU_Cancel_BTN"));
            goBack.Click();
            Thread.Sleep(1000);

            /* kontroll */
            //otsime objekti uuesti üles
            tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            foreach (var row in rowsOfTable)
            {
                var nameElement = row.FindElement(By.Id("IndexTableLecturerFirstName")).Text;
                if (nameElement == thisRunGuid)
                {
                    var clickAction = row
                        .FindElement(By.Id("IndexTableLecturerActions"))
                        .FindElement(By.Id("IndexTableLecturerDetails_BTN"));
                    clickAction.Click();
                    break;
                }
            }
            Thread.Sleep(1000);
            string details_firstName = driver.FindElement(By.Id("DD_LecturerFirstName")).Text;
            string details_lastName = driver.FindElement(By.Id("DD_LecturerLastName")).Text;
            string details_qualifications = driver.FindElement(By.Id("DD_LecturerQualifications")).Text;
            driver.Quit();

            Assert.True(isObjectFound);
            Assert.Equal(thisRunGuid, details_firstName);
            Assert.Equal("Kask", details_lastName);
            Assert.Equal("Saksa keele õpetaja", details_qualifications);
            Assert.NotEqual("Tamm", details_lastName);
        }


        /// <summary>
        /// Test mis kustutab eelnevalt lisatud õpetaja
        /// </summary>
        [Fact]
        [Order(8)]
        public void Should_DeleteUniqueLecturer_FromDeleteView()
        {
            /* ülesseade */
            IWebDriver driver = TestSetup_NavigateToIndex();
            var tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            List<IWebElement> rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            bool isObjectFound = false;

            /* tegevus */
            foreach (var row in rowsOfTable)
            {
                var nameElement = row.FindElement(By.Id("IndexTableLecturerFirstName")).Text;
                if (nameElement == thisRunGuid)
                {
                    isObjectFound = true;
                    var clickAction = row
                        .FindElement(By.Id("IndexTableLecturerActions"))
                        .FindElement(By.Id("IndexTableLecturerDelete_BTN"));
                    clickAction.Click();
                    break;
                }
            }
            Thread.Sleep(1000);
            IWebElement clickDelete = driver.FindElement(By.Id("DeleteDelete_BTN"));
            clickDelete.Click();
            Thread.Sleep(1000);

            /* kontroll */
            bool isFoundAfterDelete = false;
            tableOfIndex = driver.FindElement(By.Id("IndexTable"));
            rowsOfTable = tableOfIndex.FindElements(By.TagName("tr")).ToList();
            rowsOfTable.RemoveAt(0);
            foreach (var row in rowsOfTable)
            {
                var nameElement = row.FindElement(By.Id("IndexTableLecturerFirstName")).Text;
                if (nameElement == thisRunGuid)
                {
                    isFoundAfterDelete = true;
                    break;
                }
            }
            driver.Quit();
            Assert.True(isObjectFound);
            Assert.False(isFoundAfterDelete);
        }

        //eof

        /// <summary>
        /// Puhastab andmeväljad välja arvatud eesnime välja (seal on unikaalne id)
        /// </summary>
        /// <param name="driver">Hetkel käsitletav draiver</param>
        private void DataClear_CreateUpdate_SaveName(IWebDriver driver)
        {
            IWebElement entryLastName = driver.FindElement(By.Id("CU_Lecturer_LastName"));
            entryLastName.Clear();
            IWebElement entryQualifications = driver.FindElement(By.Id("CU_Lecturer_Qualifications"));
            entryQualifications.Clear();
        }

        /// <summary>
        /// Meetod mis sisestab vormi õpetaja andmed, valida saab kahe andmekomplekti vahel
        /// </summary>
        /// <param name="driver">WebDriver mida hetkel test kasutab</param>
        /// <param name="isOneOrTwo">Valib kahe andmekomplekti vahel</param>
        /// <param name="updateName">Kas eesnimesse pannakse uus Guid</param>
        /// <returns>Tagastab Guidi mis pandi eesnime välja</returns>
        private static string DataEntry_Lecturer(IWebDriver driver, bool isOneOrTwo = false, bool updateName = true)
        {
            Guid generatedId = Guid.NewGuid();
            if (updateName)
            {
                IWebElement entryFirstName = driver.FindElement(By.Id("CU_Lecturer_FirstName"));
                entryFirstName.SendKeys(generatedId.ToString());
            }

            if (!isOneOrTwo)
            {
                IWebElement entryLastName = driver.FindElement(By.Id("CU_Lecturer_LastName"));
                entryLastName.SendKeys("Tamm");
                IWebElement entryQualifications = driver.FindElement(By.Id("CU_Lecturer_Qualifications"));
                entryQualifications.SendKeys("Inglise keele õpetaja");
            }
            else
            {
                IWebElement entryLastName = driver.FindElement(By.Id("CU_Lecturer_LastName"));
                entryLastName.SendKeys("Kask");
                IWebElement entryQualifications = driver.FindElement(By.Id("CU_Lecturer_Qualifications"));
                entryQualifications.SendKeys("Saksa keele õpetaja");
            }
            return generatedId.ToString();
        }

        /// <summary>
        /// Ülesseade meetod mis teeb draiveri, avab lehe ja vajutab "Õpetajad" lingile
        /// </summary>
        /// <returns>Tagastab testis kasutatava draiveri</returns>
        private static IWebDriver TestSetup_NavigateToIndex()
        {
            IWebDriver driver = new FirefoxDriver(); //brauserit juhtiv draiver
            driver.Url = "https://localhost:7191/"; //avatav aadress
            Thread.Sleep(3000);
            IWebElement navigateToLecturers = driver.FindElement(By.LinkText("Õpetajad")); //otsime õpetajate lingi
            navigateToLecturers.Click(); //vajutame lingile
            return driver;
        }
    }
}
