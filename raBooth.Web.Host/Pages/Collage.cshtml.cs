using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using raBooth.Web.Core.Features.Collage.Queries;

namespace raBooth.Web.Host.Pages
{
    public class CollageModel(IMediator mediator) : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public Guid? CollageId { get; set; }

        public GetCollage.CollageDto Collage { get; set; }

        public async void OnGet()
        {
            if (CollageId == null)
            {
                return;
            }

            var getCollageResult = await mediator.Send(new GetCollage.Command(CollageId.Value));
            Collage = getCollageResult.Collage;
        }
    }
}
