/*using System;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using Tensorflow.NumPy;
using Tensorflow.Keras;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Optimizers;
using Tensorflow.Keras.Utils;
using TensorFlow;

namespace RecommendationService
{
    public class BookRecommendationModel
    {
        private int numFeatures;
        private int numLabels;
        private int hiddenUnits;

        private Tensor features;
        private Tensor labels;
        private Tensor wideInput;
        private Tensor deepInput;
        private Tensor deepLogits;
        private Tensor loss;
        private Tensor trainOp;
        private Tensor predictions;

        private Session sess;

        public BookRecommendationModel(int numFeatures, int numLabels, int hiddenUnits)
        {
            this.numFeatures = numFeatures;
            this.numLabels = numLabels;
            this.hiddenUnits = hiddenUnits;
        }
        public void BuildGraph()
        {
            // Побудова графу обчислень
            var graph = new TFGraph();
            var session = new TFSession(graph);

            // Визначення вхідних параметрів
            var bookNameInput = graph.Placeholder(TFDataType.String);
            var authorInput = graph.Placeholder(TFDataType.String);
            var categoryInput = graph.Placeholder(TFDataType.String);
            var yearInput = graph.Placeholder(TFDataType.Int32);
            var priceInput = graph.Placeholder(TFDataType.Float);
            var ratingInput = graph.Placeholder(TFDataType.Float);
            var numRatingsInput = graph.Placeholder(TFDataType.Int32);
            var numPagesInput = graph.Placeholder(TFDataType.Int32);

            // Побудова моделі
            var embeddingSize = 16;
            var bookNameEmbedding = graph.EmbeddingLookup(bookNameInput, 10000, embeddingSize);
            var authorEmbedding = graph.EmbeddingLookup(authorInput, 1000, embeddingSize);

        }

    }
}
*/