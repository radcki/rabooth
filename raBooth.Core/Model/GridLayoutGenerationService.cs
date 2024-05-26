using OpenCvSharp;

namespace raBooth.Core.Model;

public class GridLayoutGenerationService : ILayoutGenerationService
{
    public CollageLayout GenerateLayout(CollageLayoutDefinition layoutDefinition)
    {
        var layout = new CollageLayout(layoutDefinition);
        foreach (var layoutItem in GenerateItems(layoutDefinition))
        {
            layout.AddItem(layoutItem);
        }

        return layout;
    }

    private IEnumerable<CollageItem> GenerateItems(CollageLayoutDefinition layoutDefinition)
    {
        var horizontalDividersCount = 2 + (layoutDefinition.Rows.Count - 1);
        decimal rowsHeightRatioSum = layoutDefinition.Rows.Select(x => x.HeightRatio).DefaultIfEmpty(0).Sum();
        decimal imagesHeight = layoutDefinition.Size.Height - (horizontalDividersCount * layoutDefinition.StrokeWidth);
        var currentYPosition = 0;
        foreach (var row in layoutDefinition.Rows)
        {
            currentYPosition += layoutDefinition.StrokeWidth;
            var rowHeight = (int)((imagesHeight / rowsHeightRatioSum) * row.HeightRatio);
            var currentXPosition = 0;
            var verticalDividersCount = 2 + (row.Items.Count - 1);
            decimal imagesWidth = layoutDefinition.Size.Width - (verticalDividersCount * layoutDefinition.StrokeWidth);
            var itemsWidthRatioSum = row.Items.Select(x => x.WidthRatio).DefaultIfEmpty(0).Sum();

            foreach (var rowItem in row.Items)
            {
                currentXPosition += layoutDefinition.StrokeWidth;

                var itemWidth = (int)((imagesWidth / itemsWidthRatioSum) * rowItem.WidthRatio);
                var itemSize = new Size(itemWidth, rowHeight);
                var itemOrigin = new Point(currentXPosition, currentYPosition);
                yield return new CollageItem(itemSize, itemOrigin);

                currentXPosition += itemWidth;
            }

            currentYPosition += rowHeight;
        }
    }
}