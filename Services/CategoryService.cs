using Ecommerce.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ecommerce.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DoAnCuoiKy");
            _categories = database.GetCollection<Category>("Categories");
        }

        public List<Category> GetAll() => _categories.Find(_ => true).ToList();

        public Category GetById(string id) =>
            _categories.Find(c => c.Id == id).FirstOrDefault();


        public void Create(Category category) => _categories.InsertOne(category);

        public void Update(string id, Category updatedCategory) =>
            _categories.ReplaceOne(c => c.Id == id, updatedCategory);

        public void Delete(string id) =>
            _categories.DeleteOne(c => c.Id == id);
    }
}
