namespace Cardápio.Client
{
    public interface Interface
    {
        string CategoryName { get; set; }
    }

    public class CreateCategoryModel : Interface
    {
        public string CategoryName { get; set; }
    }
}
