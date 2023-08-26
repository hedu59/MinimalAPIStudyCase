using MinimalAPIStudyCase.Enums;

namespace MinimalAPIStudyCase.Models
{
    public class Toy
    {
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; } = null;

        public decimal? Price { get; private set; }
        public bool IsActive { get; private set; }
        public ETypeToy TypeToy { get; private set; }


        private Toy(Guid id, string? name, string? description, decimal? price, bool isActive, ETypeToy typeToy)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            IsActive = isActive;
            TypeToy = typeToy;
        }

        public static Toy Creator(Guid id, string? name, string? description, decimal? price, bool isActive, ETypeToy typeToy)
        {
            return new Toy(id,name,description,price,isActive,typeToy);
        }

        public static Toy Converter(ToyCommand command)
        {
            return new Toy(command.Id, command.Name, command.Description, command.Price, true, command.TypeToy);
        }
    }
}
