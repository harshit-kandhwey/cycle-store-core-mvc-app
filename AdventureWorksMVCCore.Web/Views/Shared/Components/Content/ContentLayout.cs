using AdventureWorksMVCCore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorksMVCCore.Web.Views.Components
{
    public class ContentViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public ContentViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IViewComponentResult Invoke()
        {
            var content = new ContentLayoutModel
            {
                ProductCategories = _categoryService.GetCategoriesWithSubCategory()
            };
            return View(content);
        }
    }
}
