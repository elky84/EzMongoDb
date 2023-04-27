using EzMongoDb.Models;
using EzMongoDb.Util;
using MongoDB.Driver;
using NUnit.Framework;
using System.Threading.Tasks;

namespace UnitTest
{
    public class ObjectIdGenerateTest
    {
        private class TestModel : MongoDbHeader
        {
            public TestModel()
            {
                this.GenerateId();
            }
        }

        private IMongoDatabase? _database;

        [SetUp]
        public void Setup()
        {
            _database = new MongoClient("mongodb://localhost").GetDatabase("testdb");
        }

        [Test]
        public async Task Test()
        {
            var mongoDbUtil = new MongoDbUtil<TestModel>(_database);

            var testModel = new TestModel();

            var resultCreated = await mongoDbUtil.CreateAsync(testModel);
            Assert.NotNull(resultCreated);

            Assert.AreEqual(resultCreated.Id, testModel.Id);

            var resultFind = await mongoDbUtil.FindOneAsyncById(resultCreated.Id);
            Assert.NotNull(resultFind);

            Assert.AreEqual(resultCreated.Id, resultFind.Id);
        }
    }
}