using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Trainers;
using Microsoft.ML;
using RecommendationService.Models;
using RecommendationService.Recommender;
using Microsoft.ML.Trainers.Recommender;

namespace RecommendationService.Controllers
{
    [Route("recommender")]
    [ApiController]
    public class RecommenderController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> TrainModel(IEnumerable<ReviewModel> reviews)
        {
            BookRecommender recommender = new BookRecommender();

            recommender.TrainModel(40, 50, 0.2f, reviews);
            recommender.SaveModel(recommender.trainDataSchema, recommender.recommenderModel);

            return Ok();
        }

        /*[HttpPost("addrating")]
        public async Task<IActionResult> AddRating(ReviewModel review)
        {
            BookRecommender recommender = new BookRecommender();

            recommender.LoadModel();

            // Extract trained model parameters
            MatrixFactorizationModelParameters originalModelParameters =
                ((ISingleFeaturePredictionTransformer<object>)recommender.recommenderModel).Model as MatrixFactorizationModelParameters;




            recommender.SaveModel(recommender.trainDataSchema, recommender.recommenderModel);

            return Ok();
        }*/

        [HttpPost("{userId}")]
        public async Task<IActionResult> GetRecommendations(string userId, IEnumerable<BookModel> books, int numberOfRecommendations = 3)
        {
            BookRecommender recommender = new BookRecommender();

            recommender.LoadModel();
            //var res = recommender.GetRecommendedBooks(userId, 3);
            List<BookRating> bookRatings = books
                .Where(b => b.UserId != userId && !b.Reviews.Where(r => r.UserId == userId).Any())
                .Select(book => new BookRating
                {
                    UserId = userId,
                    BookId = book.BookId,
                    //Rating = review.Rating,
                    Tags = book.Categories?.Select(c => (float)c.CategoryId).ToArray() ?? new float[0],
                    AverageRating = book.AverageRating ?? float.NaN,
                    PageCount = book.PageCount ?? float.NaN
                })
                .ToList();

            List<BookPrediction> predictions = new List<BookPrediction>();
            foreach (BookRating bookRating in bookRatings)
            {
                var res = recommender.Prediction(bookRating);
                if (!float.IsNaN(res.Score))
                {
                    predictions.Add(new BookPrediction { BookId = bookRating.BookId, RatingPrediction = res.Score });
                }
            }
            if (predictions.Count() < 3)
            {
                predictions.AddRange(books.OrderByDescending(b => b.AverageRating).Take(numberOfRecommendations - predictions.Count())
                    .Select(book => new BookPrediction
                    {
                        BookId = book.BookId,
                        RatingPrediction = book.AverageRating ?? 3 
                    }));
            }
            return Ok(predictions.OrderByDescending(p => p.RatingPrediction).Take(numberOfRecommendations));
        }
    }

    public class BookPrediction
    {
        public int BookId { get; set;}
        public float RatingPrediction { get; set;}
    }
}
