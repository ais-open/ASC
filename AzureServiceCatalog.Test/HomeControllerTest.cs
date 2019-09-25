using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace AzureServiceCatalog.Web.Test
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void CheckJsonErrorObjectIsNotNull()
        {
            //Arrange
            string jsonError = "{\"error\":{\"code\":\"RoleDefinitionWithSameNameExists\",\"message\":\"A role definition cannot be updated with a name that already exists.\"}}";
            dynamic errorObject = JObject.Parse(jsonError);

            //Act
            var errorJson = errorObject.error;

            //Assert
            Assert.IsNotNull(errorJson);
        }
    }
}
