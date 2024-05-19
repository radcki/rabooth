using System.Collections.Concurrent;
using OpenCvSharp;

namespace raBooth.Core.Model
{
    public class CollageLayout
    {
        public CollageLayout(IEnumerable<CollageItem> collageItems)
        {
            foreach (var collageItem in collageItems)
            {
                Items.Add(collageItem);
                UncapturedItems.Enqueue(collageItem);
            }

            CurrentView = GetView(Items);
        }

        public Mat CapturedItemsView { get; private set; }
        private Mat CurrentView { get; set; }

        private List<CollageItem> Items { get; set; } = new();
        private ConcurrentQueue<CollageItem> CapturedItems { get; set; } = new();
        private ConcurrentQueue<CollageItem> UncapturedItems { get; set; } = new();
        public event EventHandler<CurrentViewUpdatedEventArgs> CurrentViewUpdated;

        public void CaptureItem()
        {
            if (UncapturedItems.TryDequeue(out var item))
            {
                CapturedItems.Enqueue(item);
            }

            CapturedItemsView = GetView(CapturedItems);
        }

        public void Reset()
        {
            CapturedItems.Clear();
            UncapturedItems.Clear();
            foreach (var collageItem in Items)
            {
                collageItem.Clear();
                UncapturedItems.Enqueue(collageItem);
            }
        }

        public void UndoLastItemCapture()
        {
            if (CapturedItems.TryDequeue(out var capturedItem))
            {
                var newUncapturedItemsQueue = new ConcurrentQueue<CollageItem>();
                newUncapturedItemsQueue.Enqueue(capturedItem);
                foreach (var uncapturedItem in UncapturedItems)
                {
                    newUncapturedItemsQueue.Enqueue(uncapturedItem);
                }

                UncapturedItems = newUncapturedItemsQueue;
                foreach (var uncapturedItem in UncapturedItems)
                {
                    uncapturedItem.Clear();
                }
            }

            CapturedItemsView = GetView(CapturedItems);
            CapturedItemsView.CopyTo(CurrentView);
        }

        public void UpdateNextUncapturedItem(Mat frame)
        {
            if (UncapturedItems.TryPeek(out var item))
            {
                item.UpdateImage(frame);
                DrawItemToView(CurrentView, item);
                CurrentViewUpdated?.Invoke(this, new CurrentViewUpdatedEventArgs(CurrentView));
            }
        }

        private Mat GetView(IEnumerable<CollageItem> itemsToDraw)
        {
            var maxX = Items.Select(x => x.Position.X + x.Size.Width).DefaultIfEmpty(0).Max();
            var maxY = Items.Select(x => x.Position.Y + x.Size.Height).DefaultIfEmpty(0).Max();
            var view = new Mat(new Size(maxX, maxY), MatType.CV_8UC3, new Scalar(255, 255,255));
            foreach (var collageItem in itemsToDraw)
            {
                DrawItemToView(view, collageItem);
            }

            return view;
        }

        private void DrawItemToView(Mat image, CollageItem itemToDraw)
        {
            itemToDraw.Image.CopyTo(image[itemToDraw.Area]);
        }
    }

    public record CurrentViewUpdatedEventArgs(Mat View);

    public class CollageItem
    {
        public CollageItem(Size size, Point position)
        {
            Size = size;
            Position = position;
            Image = new Mat(Size, MatType.CV_8UC3, new Scalar(255, 255, 255));
        }

        public Point Position { get; init; }

        public Size Size { get; init; }
        public Mat Image { get; private set; }
        public Rect Area => new(Position, Size);

        public void UpdateImage(Mat image)
        {
            var center = image.Size();
            var w = Image.Width;
            var h = Image.Height;
            var x = center.Width / 2 - w / 2;
            var y = center.Height / 2 - h / 2;
            image[y, y + h, x, x + w].CopyTo(Image);
        }

        public void Clear()
        {
            Image = new Mat(Size, MatType.CV_8UC3, new Scalar(255, 255, 255));
        }
    }
}