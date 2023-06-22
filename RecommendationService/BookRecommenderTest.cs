using Microsoft.ML.Trainers;
using Microsoft.ML;
using System.Collections.Generic;
using AutoMapper;
using CsvHelper;
using RecommendationSystem;
using System.Globalization;
using System.Data;
using Microsoft.ML.Data;
using System.Linq;
using Tensorflow;

namespace RecommendationService.Test
{
    public class UserRatingTransformed
    {
        public int user_id { get; set; }
        public int book_id { get; set; }
        public float rating { get; set; }
    }
    public class BookTag
    {
        [LoadColumn(0)]
        public float BookId;
        [LoadColumn(1)]
        public float TagId;
        [LoadColumn(2)]
        public float Count;
    }
    public class BookRecommenderTest
    {
        //private readonly IMapper mapper;
        private readonly MLContext mlContext;

        private readonly int _numberOfIterations;
        private readonly int _approximationRank;
        private readonly float _learningRate;
        public BookRecommenderTest()
        {
            mlContext = new MLContext();
        }

        public BookRecommenderTest(int numOfIter, int approxRank, float learningRate, (IDataView train, IDataView test)? split = null)
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
        }

        public (IDataView training, IDataView test) InitializeData()
        {
            /*IEnumerable<UserRatingTransformed> ratingData;

            using (var reader = new StreamReader("D:\\Study\\Datasets\\goodbooks-10k-master\\ratings.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ratingData = csv.GetRecords<UserRatingTransformed>().ToList();
            }*/

            var trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "recommendation-ratings-train.csv");
            var testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "recommendation-ratings-test.csv");


            /*var trainData = mlContext.Data.LoadFromEnumerable(ratings);
            var testData = mlContext.Data.LoadFromEnumerable(testRatings);*/

            IDataView wholeDataView = mlContext.Data.LoadFromTextFile<BookRating>("D:\\Study\\Datasets\\goodbooks-10k-master\\ratings.csv", hasHeader: true, separatorChar: ',');
            /*IDataView testDataView = mlContext.Data.LoadFromTextFile<BookRating>(testDataPath, hasHeader: true, separatorChar: ',');
*/
            IDataView tagDataView = mlContext.Data.LoadFromTextFile<BookTag>("D:\\Study\\Datasets\\goodbooks-10k-master\\book_tags.csv", hasHeader: true, separatorChar: ',');



            var split = mlContext.Data
                .TrainTestSplit(wholeDataView, testFraction: 0.1);

            var trainSet = mlContext.Data
                .CreateEnumerable<BookRating>(split.TrainSet, reuseRowObject: false);

            var testSet = mlContext.Data
                .CreateEnumerable<BookRating>(split.TestSet, reuseRowObject: false);

            var tags = mlContext.Data.CreateEnumerable<BookTag>(tagDataView, reuseRowObject: false).Where(t => t.Count >= 10);

            var groupedTags = tags
                .GroupBy(t => t.BookId)
                .Select(g => (id: g.Key, values: g.Select(t => t.TagId).ToList()));

            //var res = groupedTags.FirstOrDefault().values;

            IEnumerable<BookRatingWithTags> joinedDataTrain = from rating in trainSet
                             join tagGroup in groupedTags on rating.BookId equals tagGroup.id into tagsGroup
                             select new BookRatingWithTags
                             {
                                 UserId = rating.UserId,
                                 BookId = rating.BookId,
                                 Rating = rating.Rating,
                                 Tags = tagsGroup.FirstOrDefault().values?.ToArray(),
                             };

            IEnumerable<BookRatingWithTags> joinedDataTest = from rating in testSet
                                  join tagGroup in groupedTags on rating.BookId equals tagGroup.id into tagsGroup
                                  select new BookRatingWithTags
                                  {
                                      UserId = rating.UserId,
                                      BookId = rating.BookId,
                                      Rating = rating.Rating,
                                      Tags = tagsGroup.FirstOrDefault().values?.ToArray(),
                                  };


            var trainData = mlContext.Data.LoadFromEnumerable(joinedDataTrain);
            var testData = mlContext.Data.LoadFromEnumerable(joinedDataTest);


            return (trainData, testData);
        }

        ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            var concatTags = mlContext.Transforms.Conversion.ConvertType(outputColumnName: "TagsString", inputColumnName: "Tags", outputKind: DataKind.String);

            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: "UserId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "BookIdEncoded", inputColumnName: "BookId"))
                .Append(concatTags)
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "TagFeatures", inputColumnName: "TagsString"));

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

            return model;
        }

        void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
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

        void UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
        {
            Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BookRatingWithTags, BookRatingPrediction>(model);

            var testInput = new BookRatingWithTags { UserId = 2, BookId = 301, Tags = new float[] { 30574, 31155, 21689, 8717 } };

            var bookRatingPrediction = predictionEngine.Predict(testInput);

            if (Math.Round(bookRatingPrediction.Score, 1) > 3.5)
            {
                Console.WriteLine("Movie " + testInput.BookId + " is recommended for user " + testInput.UserId + ":  " + bookRatingPrediction.Score);
            }
            else
            {
                Console.WriteLine("Movie " + testInput.BookId + " is not recommended for user " + testInput.UserId + ":  " + bookRatingPrediction.Score);
            }
        }
    }

    public class BookRating
    {
        [LoadColumn(0)]
        public int UserId;
        [LoadColumn(1)]
        public float BookId;
        [LoadColumn(2)]
        public float Rating;
    }
    public class BookRatingWithTags
    {
        public int UserId;
        public float BookId;
        public float Rating;
        public float[] Tags;
    }
    public class BookRatingPrediction
    {
        //public float Rating;
        public float Score;
    }
}
