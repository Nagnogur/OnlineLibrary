using Microsoft.ML.Trainers;
using Microsoft.ML;
using System.Data;
using Microsoft.ML.Data;
using RecommendationService.Models;
using Tensorflow.Keras.Engine;

namespace RecommendationService.Recommender
{
    public class BookRating
    {
        public string UserId { get; set; }
        public int BookId { get; set; }
        public float Rating { get; set; }
        public float[] Tags { get; set; }
        public float PageCount { get; set; }
        public float AverageRating { get; set; }
    }
    public class BookRecommender
    {
        //private readonly IMapper mapper;
        public ITransformer recommenderModel;
        public DataViewSchema trainDataSchema;
        private readonly MLContext mlContext;
        private static readonly string modelPath = Path.Combine(Environment.CurrentDirectory, "Model", "model.zip");

        private int _numberOfIterations;
        private int _approximationRank;
        private float _learningRate;
        public BookRecommender()
        {
            mlContext = new MLContext();
        }

        /*public BookRecommender(int numOfIter, int approxRank, float learningRate, (IDataView train, IDataView test)? split = null)
        {
            //this.mapper = mapper;
            mlContext = new MLContext();
            _numberOfIterations = numOfIter;
            _approximationRank = approxRank;
            _learningRate = learningRate;

            (IDataView trainingDataView, IDataView testDataView) data;

            if (split == null)
            {
                data = InitializeData();
            }
            else
            {
                data = split.Value;
            }
            //var (trainingDataView, testDataView) = InitializeData();

            var res = data.trainingDataView.Preview();

            ITransformer model = BuildAndTrainModel(mlContext, data.trainingDataView);

            EvaluateModel(mlContext, data.testDataView, model);

            UseModelForSinglePrediction(mlContext, model);
        }*/

        public void TrainModel(int numOfIter, int approxRank, float learningRate, IEnumerable<ReviewModel> reviews)
        {
            _numberOfIterations = numOfIter;
            _approximationRank = approxRank;
            _learningRate = learningRate;

            IDataView train = InitializeData(reviews);
            this.trainDataSchema = train.Schema;

            var res = train.Preview();

            ITransformer model = BuildAndTrainModel(mlContext, train);

            /*EvaluateModel(mlContext, data.testDataView, model);

            UseModelForSinglePrediction(mlContext, model);*/
        }

        public (IDataView training, IDataView test) InitializeSplitData(IEnumerable<ReviewModel> reviews)
        {
            // Retrieve book rating data from the database and transform it into the BookRatingDB format
            List<BookRating> bookRatings = reviews
                .Select(review => new BookRating
                {
                    UserId = review.UserId,
                    BookId = review.BookId,
                    Rating = review.Rating,
                    Tags = review.Book.Categories?.Select(c => (float)c.CategoryId).ToArray() ?? new float[0],
                    AverageRating = review.Book.AverageRating ?? float.NaN,
                    PageCount = review.Book.PageCount ?? float.NaN
                })
                .ToList();

            // Load the book rating data into ML.NET's IDataView
            var dataView = mlContext.Data.LoadFromEnumerable(bookRatings);

            // Split the data into training and test sets
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.1);

            return (split.TrainSet, split.TestSet);
        }

        public IDataView InitializeData(IEnumerable<ReviewModel> reviews)
        {
            // Retrieve book rating data from the database and transform it into the BookRatingDB format
            List<BookRating> bookRatings = reviews
                .Select(review => new BookRating
                {
                    UserId = review.UserId,
                    BookId = review.BookId,
                    Rating = review.Rating,
                    Tags = review.Book.Categories?.Select(c => (float)c.CategoryId).ToArray() ?? new float[0],
                    AverageRating = review.Book.AverageRating ?? float.NaN,
                    PageCount = review.Book.PageCount ?? float.NaN
                })
                .ToList();

                // Load the book rating data into ML.NET's IDataView
            var dataView = mlContext.Data.LoadFromEnumerable(bookRatings);

            return dataView;
        }
        
        /*public IDataView AddDataToTrainedModel(ReviewModel review)
        {
            List<BookRating> bookRatings = new List<BookRating>();
            // Retrieve book rating data from the database and transform it into the BookRatingDB format
            BookRating bookRating = new BookRating
            {
                UserId = review.UserId,
                BookId = review.BookId,
                Rating = review.Rating,
                Tags = review.Book?.Categories?.Select(c => (float)c.CategoryId).ToArray() ?? new float[0],
                AverageRating = review.Book?.AverageRating ?? float.NaN,
                PageCount = review.Book?.PageCount ?? float.NaN
            };
            bookRatings.Add(bookRating);


            IDataView newData = mlContext.Data.LoadFromEnumerable<BookRating>(bookRatings);
            IDataView transformedNewData = dataPrepPipeline.Transform(newData);
            // Load the book rating data into ML.NET's IDataView
            var dataView = mlContext.Data.LoadFromEnumerable(bookRatings);

            return dataView;
        }*/

        public ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            var concatTags = mlContext.Transforms.Conversion.ConvertType(outputColumnName: "TagsString", inputColumnName: "Tags", outputKind: DataKind.String);

            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: "UserId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "BookIdEncoded", inputColumnName: "BookId"))
                .Append(mlContext.Transforms.NormalizeMinMax(outputColumnName: "NormalizedPageCount", inputColumnName: "PageCount"))
                .Append(mlContext.Transforms.NormalizeMinMax(outputColumnName: "NormalizedAverageRating", inputColumnName: "AverageRating"))
                .Append(concatTags)
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "TagFeatures", inputColumnName: "TagsString"))
                .Append(mlContext.Transforms.Concatenate("Features", "NormalizedPageCount", "NormalizedAverageRating", "TagFeatures"))
                .Append(mlContext.Transforms.NormalizeMinMax(outputColumnName: "NormalizedFeatures", inputColumnName: "Features"))
                .Append(mlContext.Transforms.Conversion.ConvertType(outputColumnName: "Label", inputColumnName: "Rating"));

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "BookIdEncoded",
                LabelColumnName = "Rating",
                NumberOfIterations = _numberOfIterations,
                ApproximationRank = _approximationRank,
                LearningRate = _learningRate,
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

            //Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            recommenderModel = model;

            return model;
        }

        public void SaveModel(DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            try
            {
                mlContext.Model.Save(model, trainingDataViewSchema, modelPath);
            }
            catch (Exception ex)
            {

            }
        }

        /*public void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
        {
            Console.WriteLine("=============== Evaluating the model ===============");
            var prediction = model.Transform(testDataView);

            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Rating", scoreColumnName: "Score");


            Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
            Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
            Console.WriteLine("Loss Function: " + metrics.LossFunction.ToString());
            Console.WriteLine("Mean Absolute Error: " + metrics.MeanAbsoluteError.ToString());
            Console.WriteLine("Mean Squared Error: " + metrics.MeanSquaredError.ToString());
            
        }

        public void UseModelForSinglePrediction(ITransformer model)
        {
            Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(model);

            var testInput = new BookRating { UserId = "", BookId = 301, Tags = new float[] { 30574, 31155, 21689, 8717 } };

            var bookRatingPrediction = predictionEngine.Predict(testInput);

            if (Math.Round(bookRatingPrediction.Score, 1) > 3.5)
            {
                Console.WriteLine("Movie " + testInput.BookId + " is recommended for user " + testInput.UserId + ":  " + bookRatingPrediction.Score);
            }
            else
            {
                Console.WriteLine("Movie " + testInput.BookId + " is not recommended for user " + testInput.UserId + ":  " + bookRatingPrediction.Score);
            }
        }*/

        public ITransformer LoadModel()
        {
            ITransformer model;
            if (File.Exists(modelPath))
            {
                model = mlContext.Model.Load(modelPath, out _);
                recommenderModel = model;
            }
            else
            {
                /*// Build and train the model as before
                var data = InitializeData();
                model = BuildAndTrainModel(mlContext, data.training);*/

                return null;
            }
            return model;
        }

        public static readonly Lazy<PredictionEngine<BookRating, BookRatingPrediction>> PredictEngine = new Lazy<PredictionEngine<BookRating, BookRatingPrediction>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static BookRatingPrediction Predict(BookRating input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<BookRating, BookRatingPrediction> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(mlModel);
        }

        /*public List<float> GetRecommendedBooks(string userId, int numRecommendations)
        {
            // Create a list to store the recommended book IDs
            var recommendedBooks = new List<float>();

            // Create a single-row data view for the user
            var userDataView = mlContext.Data.LoadFromEnumerable(new[] { new { UserId = userId } });

            // Transform the user data using the existing model
            var transformedData = recommenderModel.Transform(userDataView);

            // Retrieve the predicted ratings from the transformed data
            var predictedRatings = transformedData.GetColumn<float>("Score");

            // Sort the predicted ratings in descending order
            var sortedRatings = predictedRatings.OrderByDescending(rating => rating);

            // Get the top N book IDs based on the predicted ratings
            var topBookIds = sortedRatings.Take(numRecommendations);

            // Add the recommended book IDs to the list
            recommendedBooks.AddRange(topBookIds);

            return recommendedBooks;
        }*/

        public BookRatingPrediction Prediction(BookRating userBook)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(recommenderModel);

            var bookRatingPrediction = predictionEngine.Predict(userBook);

            return bookRatingPrediction;
        }
    }

    public class BookRatingPrediction
    {
        //public float Rating;
        public float Score;
    }
}
