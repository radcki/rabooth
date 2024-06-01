using System.Collections.Concurrent;
using OpenCvSharp;

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
    public DateTime LastItemCaptureUtcTime { get; private set; }

    private Mat GetCanvas()
    {
        var canvas = new Mat(Definition.Size, MatType.CV_8UC3, new Scalar(255, 255, 255));
        var mask = GetItemsMask();
        canvas.SetTo(new Scalar(245, 245, 245), mask);
        return canvas;
    }

    public bool HasUncaptredItems()
    {
        return UncapturedItems.Count > 0;
    }

    public void AddItem(CollageItem item)
    {
        Items.Add(item);
        UncapturedItems.Enqueue(item);
        RedrawViewWithCapturedItems();
    }

    public void CaptureNextItem()
    {
        if (!UncapturedItems.TryDequeue(out var item))
            return;

        DrawItemOnCanvas(ViewWithCapturedItems, item);
        CapturedItems.Push(item);
        LastItemCaptureUtcTime = DateTime.UtcNow;
    }

    public void UpdateNextUncapturedItemSourceImage(Mat image)
    {
        if (UncapturedItems.TryPeek(out var item))
        {
            item.UpdateSourceImage(image);
        }
    }

    public IEnumerable<Mat> GetSourceItemImages()
    {
        foreach (var item in Items)
        {
            yield return item.GetSourceImage();
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
    public Mat GetView()
    {
        var view = ViewWithCapturedItems.Clone();

        return view;
    }

    public Mat GetItemsMask()
    {
        var mask = new Mat(Definition.Size, MatType.CV_8UC1, new Scalar(0, 0, 0));
        foreach (var item in Items)
        {
            mask[item.Area].SetTo(new Scalar(1));
        }
        return mask;
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

    public void Clear()
    {
        var newUncapturedItemsQueue = new ConcurrentQueue<CollageItem>();
        CapturedItems.Clear();
        foreach (var item in Items)
        {
            item.ClearSourceImage();
            newUncapturedItemsQueue.Enqueue(item);
        }
        UncapturedItems = newUncapturedItemsQueue; 
        RedrawViewWithCapturedItems();
    }

    private void DrawItemOnCanvas(Mat canvas, CollageItem item)
    {
        var itemView = item.GetImage();

        itemView.CopyTo(canvas[item.Area]);
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