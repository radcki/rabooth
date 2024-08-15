using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using raBooth.Web.Core.Features.Collage.Queries;

namespace raBooth.Web.Host.Pages
{
    public class CollageListModel(IMediator mediator) : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string Date { get; set; }

        [BindProperty(SupportsGet = true)] 
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)] 
        public int PageSize { get; set; } = 20;


        public List<ListCollages.CollageDto> Collages { get; set; }
        public int Total { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(Total/(float)PageSize) : 0;

        public async void OnGet()
        {
            if (string.IsNullOrWhiteSpace(Date) || !DateTime.TryParse(Date, out var date))
            {
                return;
            }

            var fromDate = date.Date;
            var toDate = fromDate.AddDays(1);

            var getCollageResult = await mediator.Send(new ListCollages.Command(fromDate, toDate, PageIndex, PageSize));
            Collages = getCollageResult.Data;
            PageIndex = getCollageResult.Page;
            Total = getCollageResult.Total;
        }
    }
}
