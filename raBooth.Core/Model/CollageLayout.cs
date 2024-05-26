using System.Collections.Concurrent;
using OpenCvSharp;
using static System.Net.Mime.MediaTypeNames;

namespace raBooth.Core.Model;

public class CollageLayout
{
    public CollageLayout(CollageLayoutDefinition definition)
    {
        Definition = definition;
        RedrawViewWithCapturedItems();
    }

    public CollageLayoutDefinition Definition { get; private set; }
    private List<CollageItem> Items { get; } = [];
    private ConcurrentQueue<CollageItem> UncapturedItems { get; set; } = [];
    private Stack<CollageItem> CapturedItems { get; set; } = [];
    private Mat ViewWithCapturedItems { get; set; }
    private Mat GetCanvas()
    {
        return new Mat(Definition.Size, MatType.CV_8UC3, new Scalar(255, 255, 255));
    }

    public void AddItem(CollageItem item)
    {
        Items.Add(item);
        UncapturedItems.Enqueue(item);
    }

    public void CaptureNextItem()
    {
        if (!UncapturedItems.TryDequeue(out var item))
            return;

        DrawItemOnCanvas(ViewWithCapturedItems, item);
        CapturedItems.Push(item);
    }

    public void UpdateNextUncapturedItemSourceImage(Mat image)
    {
        if (UncapturedItems.TryPeek(out var item))
        {
            item.UpdateSourceImage(image);
        }
    }

    public Mat GetViewWithNextUncapturedItemPreview()
    {
        var view = ViewWithCapturedItems.Clone();
        if (UncapturedItems.TryPeek(out var item))
        {
            DrawItemOnCanvas(view, item);
        }
        return view;
    }

    public void UndoLastItemCapture()
    {
        if (!CapturedItems.TryPop(out var item))
            return;
        item.ClearSourceImage();
        var newUncapturedItemsQueue = new ConcurrentQueue<CollageItem>();
        newUncapturedItemsQueue.Enqueue(item);
        foreach (var uncapturedItem in UncapturedItems)
        {
            newUncapturedItemsQueue.Enqueue(uncapturedItem);
        }

        UncapturedItems = newUncapturedItemsQueue;
        RedrawViewWithCapturedItems();
    }

    private void DrawItemOnCanvas(Mat canvas, CollageItem item)
    {
        var itemView = item.GetImage();

        itemView.CopyTo(canvas[item.Area]);

        //Cv2.ImShow("a", canvas);
        //Cv2.WaitKey(0);
    }

    private void RedrawViewWithCapturedItems()
    {
        var canvas = GetCanvas();
        foreach (var item in CapturedItems)
        {
            DrawItemOnCanvas(canvas, item);
        }
        ViewWithCapturedItems = canvas;
    }
}