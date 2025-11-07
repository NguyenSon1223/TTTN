using Ecommerce.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace Ecommerce.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Category> _categories;

        public ProductService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DoAnCuoiKy");
            _products = database.GetCollection<Product>("Products");
            _categories = database.GetCollection<Category>("Categories");
        }

        public List<Product> GetAll() => _products.Find(_ => true).ToList();

        public Product GetById(string id) =>
            _products.Find(p => p.Id == id).FirstOrDefault();

        public void Create(Product product)
        {
            if (string.IsNullOrEmpty(product.Id))
                product.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

            _products.InsertOne(product);
        }

        public void Update(string id, Product product)
        {
            _products.ReplaceOne(p => p.Id == id, product);
        }

        public void Delete(string id)
        {
            _products.DeleteOne(p => p.Id == id);
        }

    }
}
