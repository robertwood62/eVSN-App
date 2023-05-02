using Fri.DownloadService.Tests;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tests
{
    [TestClass]
    public class EntityTests
    {
        public class TestInfo
        {
            public string? NAME { get; set; }
            public string? DESCRIPTION { get; set; }
            public DateTime Created { get; set; }
            public DateTime? LastModified { get; set; }
            public bool IsDeleted { get; set; }
            public int Value { get; set; }
        }

        [Table("TestEntity")]
        public class TestEntityData :EntityBase
        {
            public TestInfo? Data { get; set; }
        }

        [Table("TestEntity")]
        public class TestEntityDictionary : EntityBase
        {
            public Dictionary<string, object?>? Data { get; set; }
        }

        [TestMethod]
        public async Task TestEntitiesAsync()
        {
            var dbManager = TestHelper.Db;

            // Create the entity record in the db (using strongly type object)
            var data = new TestEntityData
            {
                Data = new TestInfo
                {
                    NAME = "P1",
                    DESCRIPTION = "D1",
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    IsDeleted = true,
                    Value = 100
                }
            };
            await dbManager.CreateAsync(data);

            // Read the data back using the dictionary approach
            var dictionary = await dbManager.GetAsync<TestEntityDictionary>(data.Id);
            Assert.IsNotNull(dictionary);
            Assert.IsNotNull(dictionary.Data);
            Assert.IsTrue(data.Data.NAME == dictionary.Data["NAME"]?.ToString());
            Assert.IsTrue(data.Data.DESCRIPTION == dictionary.Data["DESCRIPTION"]?.ToString());
            Assert.IsTrue(data.Data.Created.ToString() == dictionary.Data["Created"]?.ToString());
            Assert.IsTrue(data.Data.IsDeleted.ToString() == dictionary.Data["IsDeleted"]?.ToString());
            Assert.IsTrue(data.Data.LastModified?.ToString() == dictionary.Data["LastModified"]?.ToString());
            Assert.IsTrue(data.Data.Value.ToString() == dictionary.Data["Value"]?.ToString());

            // Try out the JSON parser logic (2 projects)
            string json = JsonConvert.SerializeObject(new TestInfo[] { data.Data, data.Data });
            var dictionaries = JsonConvert.DeserializeObject<Dictionary<string, object?>[]>(json);
            if (dictionaries != null)
            {
                foreach (var item in dictionaries)
                {
                    Assert.IsNotNull(item);
                    Assert.IsTrue(data.Data.NAME == item["NAME"]?.ToString());
                    Assert.IsTrue(data.Data.DESCRIPTION == item["DESCRIPTION"]?.ToString());
                    Assert.IsTrue(data.Data.Created.ToString() == item["Created"]?.ToString());
                    Assert.IsTrue(data.Data.IsDeleted.ToString() == item["IsDeleted"]?.ToString());
                    Assert.IsTrue(data.Data.LastModified?.ToString() == item["LastModified"]?.ToString());
                    Assert.IsTrue(data.Data.Value.ToString() == item["Value"]?.ToString());
                }
            }


        }
    }
}
