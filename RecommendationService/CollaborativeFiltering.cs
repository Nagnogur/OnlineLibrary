using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Tensorflow.Contexts;
using static HDF.PInvoke.H5T;

namespace RecommendationSystem
{

    //,Language,Authors,Rating,RatingDist2,RatingDist5,ISBN,RatingDist3
    class BookCsv
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? RatingDist1 { get; set; }
        public int pagesNumber { get; set; }
        public string? RatingDist4 { get; set; }
        public string? RatingDistTotal { get; set; }
        public int PublishMonth { get; set; }
        public int PublishDay { get; set; }
        public string? Publisher { get; set; }
        public int CountsOfReview { get; set; }
        public int PublishYear { get; set; }
        public string? Language { get; set; }
        public string? Authors { get; set; }
        public float Rating { get; set; }
        public string? RatingDist2 { get; set; }
        public string? RatingDist5 { get; set; }
        public string? ISBN { get; set; }
        public string? RatingDist3 { get; set; }
    }

    // ID,Name,Rating
    public class UserRatingCsv
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Rating { get; set; }
    }
    public class UserRatingTransformed
    {
        public int user_id { get; set; }
        public int book_id { get; set; }
        public float rating { get; set; }
    }

    class CollaborativeFiltering
    {
        public void TrainAndPrint()
        {
            IEnumerable<BookCsv> bookData;
            IEnumerable<UserRatingTransformed> ratingData;

            using (var reader = new StreamReader("D:\\Study\\Datasets\\book1-100k.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                bookData = csv.GetRecords<BookCsv>().ToList();
            }

            using (var reader = new StreamReader("D:\\Study\\Datasets\\goodbooks-10k-master\\ratings.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ratingData = csv.GetRecords<UserRatingTransformed>().ToList();
            }

            var data = new List<BookRating>();

            foreach (var rating in ratingData)
            {
                /*float r;
                switch (rating.Rating)
                {
                    case "it was amazing":
                        {
                            r = 5;
                            break;
                        }
                    case "really liked it":
                        {
                            r = 5;
                            break;
                        }
                    case "liked it":
                        {
                            r = 5;
                            break;
                        }
                    case "it was ok":
                        {
                            r = 5;
                            break;
                        }
                    case "did not like it":
                        {
                            r = 5;
                            break;
                        }
                    default:
                        {
                            r = -1;
                            break;
                        }
                }

                if (r == -1)
                {
                    continue;
                }

                int? bookId = bookData.FirstOrDefault(b => b.Name == rating.Name)?.Id;

                if (bookId == null)
                {
                    continue;
                }*/

                BookRating br = new BookRating()
                {
                    UserId = rating.user_id,
                    BookId = rating.book_id,
                    Rating = rating.rating
                };
                data.Add(br);
            }

            // Вхідні дані
            /*var data = new List<BookRating>
            {
                new BookRating { UserId = 1, BookId = 1, Rating = 5, Genres = new List<string> { "Fantasy", "Adventure" } },
                new BookRating { UserId = 1, BookId = 2, Rating = 4, Genres = new List<string> { "Mystery", "Thriller" } },
                // Додайте інші оцінки користувачів з жанрами
            };*/

            var context = new MLContext();

            // Перетворення даних
            var dataView = context.Data.LoadFromEnumerable(data);
            var pipeline = context.Transforms.Conversion.MapValueToKey("UserId")
                .Append(context.Transforms.Conversion.MapValueToKey("BookId"))
                .Append(context.Transforms.Conversion.MapValueToKey("Rating"));

            // Побудова моделі
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserId",
                MatrixRowIndexColumnName = "BookId",
                LabelColumnName = "Rating",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };
            var trainer = context.Recommendation().Trainers.MatrixFactorization(options);
            var pipelineWithTrainer = pipeline.Append(trainer);

            // Навчання моделі
            var model = pipelineWithTrainer.Fit(dataView);

            // Використання моделі для отримання рекомендацій
            var predictionEngine = context.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(model);
            //var user = new BookRating { UserId = 1, BookId = 3, Genres = new List<string> { "Sci-Fi", "Adventure" } }; // Користувач, для якого отримуємо рекомендацію
            var user = new BookRating { UserId = 1, BookId = 3}; // Користувач, для якого отримуємо рекомендацію
            var prediction = predictionEngine.Predict(user);

            Console.WriteLine($"Рекомендована оцінка для книги: {prediction.Rating}");
        }
    }

    // Клас для представлення даних про оцінку користувача
    class BookRating
    {
        public float UserId { get; set; }
        public float BookId { get; set; }
        public float Rating { get; set; }
        //public List<string> Genres { get; set; }
    }

    // Клас для представлення прогнозу рекомендації оцінки користувача
    class BookRatingPrediction
    {
        public float Rating { get; set; }
    }

    public class RecommenderTest
    {
        private readonly MLContext mlContext;
        public RecommenderTest()
        {

            mlContext = new MLContext();

            /*var (trainingDataView, testDataView) = InitializeData();
            ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);

            //EvaluateModel(mlContext, testDataView, model);

            UseModelForSinglePrediction(mlContext, model);*/
        }

        /*public (IDataView training, IDataView test) InitializeData()
        {


            var ratingsEntities = db.Reviews.ToList();
            var ratings = mapper.Map<List<BookRating>>(ratingsEntities);

            var books = db.Books.ToList();

            var testRatings = ratings.Take(2);
            //ratings = ratings.Skip(2).ToList();


            var trainData = mlContext.Data.LoadFromEnumerable(ratings);
            var testData = mlContext.Data.LoadFromEnumerable(testRatings);

            return (trainData, testData);
        }*/
    }
}
