namespace BaltaDataAccess.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string Summary { get; set; }
    public int Order { get; set; }
    public string Description { get; set; }
    public bool Featured { get; set; }

    public Category() { }

    public Category(string title, string url, string summary, int order, string description, bool featured)
    {
        Title = title;
        Url = url;
        Summary = summary;
        Order = order;
        Description = description;
        Featured = featured;
    }
}