using Microsoft.AspNetCore.Components;

namespace AcademicManager.Web.Components.Common
{
    public class DataTableColumn<T>
    {
        public string Title { get; set; } = "";
        public string Property { get; set; } = "";
        public string? SortProperty { get; set; }
        public string CssClass { get; set; } = "";
        public string Style { get; set; } = "";
        public string? Description { get; set; }
        public RenderFragment<T>? Template { get; set; }
        public bool Sortable { get; set; } = true;
        public bool Searchable { get; set; } = true;

        public DataTableColumn()
        {
            SortProperty = Property;
        }

        public DataTableColumn(string title, string property)
        {
            Title = title;
            Property = property;
            SortProperty = property;
        }

        public DataTableColumn(string title, string property, RenderFragment<T> template)
        {
            Title = title;
            Property = property;
            SortProperty = property;
            Template = template;
        }
    }

    public class DataColumn
    {
        public string Title { get; set; } = "";
        public string Property { get; set; } = "";
        public string? SortProperty { get; set; }
        public string CssClass { get; set; } = "";
        public string Style { get; set; } = "";
        public string? Description { get; set; }
        public bool Sortable { get; set; } = true;
        public bool Searchable { get; set; } = true;

        public DataColumn()
        {
            SortProperty = Property;
        }

        public DataColumn(string title, string property)
        {
            Title = title;
            Property = property;
            SortProperty = property;
        }
    }
}