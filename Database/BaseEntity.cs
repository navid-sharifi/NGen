using Newtonsoft.Json;

namespace Domain
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public BaseEntity()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }
       
    }
}
