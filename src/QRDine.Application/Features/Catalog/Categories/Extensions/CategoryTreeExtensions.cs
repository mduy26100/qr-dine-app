using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Extensions
{
    public static class CategoryTreeExtensions
    {
        public static List<CategoryTreeDto> BuildTree(this IEnumerable<CategoryTreeDto> flatCategories)
        {
            var categoryList = flatCategories.ToList();

            var childrenLookup = categoryList
                .Where(c => c.ParentId != null)
                .ToLookup(c => c.ParentId);

            var rootCategories = categoryList
                .Where(c => c.ParentId == null)
                .ToList();

            foreach (var root in rootCategories)
            {
                root.Children = childrenLookup[root.Id].ToList();
            }

            return rootCategories;
        }
    }
}
