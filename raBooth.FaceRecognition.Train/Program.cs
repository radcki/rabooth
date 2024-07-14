namespace raBooth.FaceRecognition.Train
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var trainer = new ModelTrainer();
            trainer.LoadDataset(@"C:/temp/rabooth-dataset");
            trainer.Train();
            trainer.Test(@"C:/temp/rabooth-dataset/test1.jpg");
            trainer.Test(@"C:/temp/rabooth-dataset/test2.jpg");
        }
    }
}
